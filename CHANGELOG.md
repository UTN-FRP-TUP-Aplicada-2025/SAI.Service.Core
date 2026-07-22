# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el versionado sigue [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

### Añadido

- **Etapa 3 · Incremento A — Sondeo y persistencia de muestras** (BT-17, BT-18, US-08, US-10):
  el **planificador de sondeo** (`ServicioSondeo`, un `BackgroundService`) que a cadencia
  configurable (`Sai:Sondeo:IntervaloSeg`, 5 s por defecto) lee el estado del SAI por el adaptador y
  persiste una `Muestra` **append-only** con su **calidad** (completa / parcial / perdida, tolerando
  huecos por variable) y la **procedencia por variable** (US-10: `input.voltage`/`output.voltage`/
  `ups.load` medidos, `battery.charge` **derivada**). Dominio de monitoreo nuevo: `FuenteDatos`,
  `SesionSondeo` (con el mapa variable→origen), `Muestra` y `Agregado` (que **no hereda** de
  `Muestra`, ADR-08, y conserva mínimo/máximo/promedio con cobertura y advertencia), más
  `CalculadorAgregado`. Mapeo EF (diccionarios como JSON, todas `IEntidadHistoria` protegidas por el
  interceptor append-only) y migración `EsquemaMonitoreo`. Cada ronda corre en su propio alcance de
  DI (un `DbContext` por ronda). Delimitación: la derivación de **eventos** y la alerta de pérdida de
  comunicación van al Incremento B; la purga por retención (choca con append-only) queda diferida.
  13 pruebas nuevas (dominio + integración). *El panel en vivo llega en el Incremento B.*

- **Etapa 2 · Incremento C2 — Panel de alta de equipos** (US-04, US-05, CU-02): el **wizard**
  interactivo (MudBlazor) de alta en `AltaDeEquipos.razor` con los cuatro pasos —descubrir el
  dispositivo (`IDescubridorSai`; `DISPOSITIVO_NO_DESCUBIERTO` si no hay candidatos; badge «sin marca
  ni modelo»), probar la conexión por efecto observado (`IAdaptadorConexion`; bloquea el avance si
  falla, `PRUEBA_CONEXION_FALLIDA`), declarar catálogo e inventario, y confirmar el alta (llama al
  caso de uso, traduce los errores de dominio como `VIDA_FLOTACION_SIN_TEMPERATURA`)—. El panel
  principal (`PanelEstadoEnVivo.razor`) muestra el **aviso permanente de puesta en marcha**
  («operativo · n de 4 supuestos verificados») con enlace a la ventana de mantenimiento cuando el
  servicio está en solo aviso (US-05), y el `PanelDeVerificaciones.razor` lista el estado de los
  cuatro supuestos. Los componentes interactivos usan un alcance por operación (`IServiceScopeFactory`)
  para no sostener el `DbContext` durante todo el circuito. Con esto la **Etapa 2 queda completa** y
  el alta de equipos se valida en el navegador.

- **Etapa 2 · Incremento C1 — Persistencia y alta de equipos** (US-04, US-05, parte de CU-02):
  **persistencia EF Core** del dominio de equipos sobre SQLite (ADR-18): catálogo, inventario
  (`UnidadFisica` con TPH), vínculos temporales y verificaciones, con `Vigencia`/`Valor<T>` como
  complex types, **índices únicos parciales** `WHERE Hasta IS NULL` ("a lo sumo uno vigente", RC-02),
  CHECKs (baja lógica coherente, intervalo, I-21) y migración `EsquemaEquipos`. **Dominio**: entidad
  `Verificacion` (cuatro supuestos), enum `Modalidad` y `EvaluadorModalidad` (degradación a
  `SoloAlerta` mientras no estén los cuatro supuestos verificados, RN-01/RN-02). **Caso de uso de
  alta** (`ServicioAltaEquipos`, CU-02): crea catálogo e inventario, abre `MontajeBateria` y
  `CoberturaHost` con fin abierto (validando el no solapamiento), siembra las cuatro verificaciones
  en `NuncaVerificado` y deja la modalidad efectiva forzada a `SoloAlerta`, todo transaccional (si
  algo falla no quedan entidades a medias). Decisión de mapeo: las entidades se persisten por su
  `Codigo` como clave (el dominio se referencia por código; desvío documentado del modelo lógico que
  usaba Id int), con un constructor privado de materialización que **no** introduce dependencia de
  EF en el dominio. 9 pruebas nuevas (dominio + integración contra la base real). *El panel MudBlazor
  de alta y el banner de degradación llegan en el Incremento C2.*

- **Etapa 2 · Incremento B — Adaptador de conexión NUT real + descubrimiento** (BT-15, US-03):
  `AdaptadorConexionNut` que habla con el SAI a través de **NUT** (Network UPS Tools) por su
  protocolo de red, sin traductor de dialecto propio (ADR-01). Cliente NUT mínimo (`ClienteNut`
  sobre `IClienteNut`, con parseo puro `ProtocoloNut`), **descubrimiento** de dispositivos
  (`IDescubridorSai`: `LIST UPS` + descriptores `vendor:product · marca · serie`, US-03) y **prueba
  de conexión por efecto observado** (lee `ups.status` real, mide latencia; una excepción de
  transporte se traduce a "no alcanzable", ADR-11). Mapeo NUT→`EstadoSai`: `input.voltage`/
  `output.voltage`/`ups.load` como **medidos**, `battery.charge` como **estimado por el driver**
  (nunca medido, RN-05). El adaptador se elige por configuración (`Sai:Adaptador` = `Simulado` por
  defecto o `Nut`). El apagado y el test de batería por NUT quedan diferidos a la Etapa 4 (escritura
  con credenciales). El anclaje USB por ruta física (BT-13) es configuración de despliegue
  (udev + `--device`, ADR-03/ADR-25), no código. Pruebas: parser puro + adaptador con cliente falso
  (16 nuevas) y 3 pruebas **en vivo** opcionales (validadas contra el SAI real por NUT).

- **Etapa 2 · Incremento A — Dominio del ciclo de vida de equipos** (BT-11, BT-12): modelo de
  dominio framework-free en tres capas (**catálogo** `Fabricante`/`ModeloDispositivo`/`ModeloBateria`;
  **inventario** `UnidadFisica` con `Host`/`Dispositivo`/`Bateria`, baja lógica y máquina de estados;
  **vínculos temporales** `MontajeBateria`/`CoberturaHost`). `Vigencia` como intervalo semiabierto
  `[desde, hasta)` (ADR-05) con no-solapamiento por clave (`Vigencias.AdmiteNuevo`) y `ResolutorTemporal`
  que resuelve la unidad vigente en un instante (RC-07). Los invariantes **I-1, I-2, I-3, I-4** (vigencia
  y sucesión sin hueco), **I-5, I-6** (baja lógica consultable y máquina de estados) e **I-21** (vida de
  flotación exige temperatura de referencia) corren como pruebas (mitigación del riesgo R-10). 35 pruebas
  de dominio nuevas (65 en total). Aún sin persistencia ni UI: llegan en incrementos posteriores.

- **Etapa 1 — Persistencia y acceso**: primera etapa con lógica real sobre el andamiaje
  del Sprint 0. **Persistencia** EF Core + SQLite con migración inicial y `DbContext`
  (`IdentityDbContext`) con el interceptor append-only (ADR-04, ADR-18); la migración se
  aplica al arranque y siembra el rol único `administrador` de forma idempotente.
  **Autenticación del administrador único** (ADR-16) con ASP.NET Core Identity: alta
  inicial, inicio de sesión por cookie, cambio de contraseña y cierre de sesión, resueltos
  con formularios SSR estáticos (setean la cookie desde `HttpContext`). **Guarda de primer
  arranque en tres capas** (ruteo / superficie / acción, Design-Rules-Primer-Arranque): un
  middleware de la capa de ruteo desvía las superficies del panel a `/alta-inicial` mientras
  no exista ningún administrador. **API REST autenticada con Bearer JWT vía ROPC**
  (**ADR-28**, complementa a ADR-16): `POST /api/v1/token` emite el token con las
  credenciales del administrador y los endpoints de `/api/v1` lo exigen por la policy `Api`,
  con esquema dual (la cookie sigue sirviendo al panel). Se aplica el **tema visual de la
  maqueta** (paleta verde/chrome oscuro, tipografía Inter) al panel MudBlazor. **Pruebas de
  integración** (WebApplicationFactory) del acceso y del flujo de token, con base SQLite
  aislada por prueba. La configuración de la cadena de conexión y de la firma JWT se lee de
  forma **diferida** (desde el `ServiceProvider` / `IOptions`), de modo que firmar y validar
  usan siempre el mismo secreto y las pruebas quedan aisladas. El keyring de **DataProtection**
  (que cifra la cookie de sesión y los tokens antiforgery) se **persiste en un volumen** con
  nombre de aplicación estable (**ADR-29**), para que reiniciar o redesplegar el contenedor no
  invalide sesiones ni rompa los formularios; y los formularios SSR emiten **un único** token
  antiforgery (el de `EditForm`), evitando el campo duplicado que devolvía HTTP 400, y el
  middleware antiforgery se ejecuta **después** de la autenticación para que los formularios
  autenticados (cambio de contraseña) validen el token contra el usuario correcto. Índice de
  decisiones → v1.5 (29 ADR).

- **Sprint 0 — Arranque (primer código)**: andamiaje de la solución .NET 10 en cinco
  assemblies (Clean Architecture: `Domain`, `Application`, `Infrastructure`, `Api`, `Web`),
  con tres proyectos de prueba (xUnit + FluentAssertions), Dev Container, scripts de
  build/run, pipeline de CI inicial y el panel base en Blazor interactive server +
  MudBlazor (menú lateral, barra superior y sello de versión según la maqueta aprobada).
  Es esqueleto: compila y corre, sin lógica de negocio (stubs para el adaptador simulado,
  el `DbContext` y la ranura de Identity). Se cierran las tres decisiones de Sprint 0 con
  **ADR-25** (NUT en el contenedor), **ADR-26** (TLS autofirmado en Kestrel) y **ADR-27**
  (contrato del puerto `IAdaptadorConexion`); ADR-19/20/22 pasan a *Superado*. ADR-21
  (contrato del 409) queda diferido a la Etapa 5.

- **Fase H del SDD — README raíz y handoff**: `SDD/Docs/README.md` (índice maestro de
  toda la documentación, tabla de proyectos, mapa de las categorías 00→11 con las
  omisiones declaradas —04 por no-LLM, 10 por ADR-23—, flujos de lectura por audiencia,
  glosario y la sección de onboarding del desarrollador comprometida en ADR-23). Auditoría
  final consolidada del entregable completo: **APROBADO CON OBSERVACIONES, apto para el
  handoff a codificación** (13/13 enlaces del README sin roturas, cadena de trazabilidad
  D6 completa sin huérfanos, terminología consistente). Con esto la documentación SDD
  (Fases A-H) queda generada y auditada.

- **Fase G del SDD — 11-Examples**: dos samples documentados —`Ejemplo-01-Datos-Seed`
  (básico: explorar el sistema con datos precargados y el adaptador de conexión simulado,
  sin hardware) y `Ejemplo-02-Api-Ingesta` (intermedio: los cuatro caminos del contrato de
  ingesta 201/200/409/422)— más el README con la tabla maestra. Nombrados por capacidad,
  no por dominio. Auditoría independiente de la fase.

- **Fase F del SDD — 09-Devops**: pipeline CI/CD (10 stages que ejecutan la
  Definition-of-Done de 08 como quality gates), estrategia de versionado (SemVer 2.0.0,
  Conventional Commits, MinVer, trunk-based), entornos de despliegue, guía de publicación
  de la imagen Docker y política de supply-chain (SBOM CycloneDX, firma cosign). La
  categoría 10-Developer-Guide se omite (proyecto de un solo desarrollador sin portal),
  registrada en **ADR-23**; el modelo de dos ambientes DEV/PROD sin staging se registra
  en **ADR-24**. Índice de decisiones a 24 ADR. Auditoría independiente de la fase.

- **Fase E del SDD — 08-Calidad-Y-Pruebas**: estrategia de calidad (ISO 25010,
  10 quality gates), estrategia de testing (pirámide 70/25/5, cobertura por capa con
  Domain 90/85 y global 80/70), plan de pruebas por etapa, matriz de cobertura
  (12 CU / 13 RN / 25 NFR / 21 invariantes ↔ tests), 40 casos de prueba referenciales,
  criterios de validación de release, Definition-of-Done de cuatro capas, guía de testing
  de extensibilidad del adaptador de conexión, y la `Matriz-Sensado-Deriva-v1.0.md`
  (142 comprobaciones que contrastan lo construido contra la línea de base de la maqueta
  de la Fase B2). Auditoría independiente de la fase.

- **Fase D del SDD — 06-Backlog-Tecnico y 07-Plan-Sprint**: Product Backlog
  (7 épicas, 26 historias de usuario en archivos individuales, MoSCoW 18/6/2 con
  estimación Fibonacci), Backlog Técnico (30 tareas técnicas trazadas a ADR/componentes/
  contratos), Definition-of-Ready y, por ser proyecto de un solo desarrollador,
  `Mini-Plan-v1.0.md` (6 etapas: Sprint 0 de arranque + una por flujo, 17 riesgos) en
  lugar del plan de sprint completo. Auditoría independiente de la fase.

- **Fase C del SDD — 05-Arquitectura-Tecnica**: documento maestro de arquitectura
  (cuatro vistas, cross-cutting, 25 NFR numéricos, riesgos; estilo Clean Architecture
  en cinco assemblies justificado contra dos alternativas), 22 ADR individuales
  (18 Aceptado derivados de los pre-ADR del intake + autenticación, errores de la API y
  motor SQLite; 4 Propuesto por las decisiones abiertas de Sprint 0) con su índice,
  modelo de datos lógico (23 tablas SQLite con EF Core, herencia TPH, 26 índices,
  migración inicial, trazado al modelo conceptual), flujo de ejecución del planificador,
  contratos REST de la API de ingesta y documento de extensibilidad del adaptador de
  conexión. Auditoría independiente de la fase.

- **Fase B2 del SDD — validación visual de maqueta**: maqueta navegable estática en
  `SDD/Maquetas/Sai-Service-Core/` (11 superficies, HTML/CSS/JS con Bootstrap 5, sin
  build), aprobada por el administrador. Emite en `03-UX-UI-DX/` los artefactos de línea
  de base del sensado de deriva: `Linea-Base-Visual-v1.0.md`, `Contrato-Datos-Maqueta-v1.0.md`
  y `Bitacora-Validacion-Maqueta-v1.0.md`.

### Cambiado

- **Unificación de terminología** (retroalimentación de la Fase B2, propagada a toda la
  cadena: intake, 00, 01, 02, 03 y maqueta):
  - Dominio: **«parque» → «equipos»** (el término se juzgó jerga; «Dispositivo» e
    «Inventario» colisionaban con entidades/capas del modelo conceptual, «equipos» no).
    El flujo UF-1 pasa a «Alta de equipos y puesta en marcha».
  - Acceso: **«secreto» → «contraseña»** en la superficie de login (los «Secretos en
    runtime/CI» del intake §17 P.5 no se tocan: son gestión de secretos, otro concepto).
  - Renombrado de `NB-04`, `CU-02` y el wireframe de alta a sus nuevos slugs; intake a
    v1.2, manifiesto a v1.1.

- Intake actualizado a v1.1: **Parte D — Anexos de datos** (§20 escenarios `E-1`…`E-8`
  con JSON completo; §21 cobertura, invariantes I-1 a I-21 y flujos end-to-end). El intake
  pasa a ser autocontenido.
- `SOLUTION-MANIFEST-Sai-Service-Core-v1.0.md`, manifiesto derivado de §13 del intake
  durante la fase de validación del orquestador SDD (caso degenerado: un proyecto
  `web-monolith`, layout aplanado).
- **Fase A del SDD** — documentación de nivel solución:
  - `00-Contexto/`: Visión de producto, Alcance, Roadmap y Compatibilidad de plataformas.
    `Acuerdo-Equipo` omitido por ser proyecto de un solo desarrollador.
  - `01-Necesidades-Negocio/`: índice más 8 necesidades de negocio (NB-01 a NB-08),
    todas trazables al intake.
- **Fase B del SDD** — especificación y experiencia:
  - `02-Especificacion-Funcional/`: 12 casos de uso (CU-01 a CU-12), 13 reglas de negocio,
    modelo conceptual (27 entidades) y 9 reglas conceptuales de integridad.
  - `03-UX-UI-DX/`: marco de experiencia, 11 wireframes y glosario UX, con las cuatro
    extensiones de capacidad del arquetipo de panel monolítico (configuración por esquema,
    primer arranque, acceso monousuario, identidad de versión).
  - `04-Prompts-AI` omitida (el proyecto no usa modelos de lenguaje).
- Informes de auditoría independiente de las Fases A y B en `SDD/Docs/Audit/`.

### Notas

- Cadena de trazabilidad D6 cubierta hasta el modelo funcional: Intake → 00 → NB → CU →
  RN → modelo conceptual, sin artefactos huérfanos.
- Pendiente: Fase B2 (validación visual de maqueta) y Fases C a H.

## [0.1.0] - 2026-07-20

### Añadido

- Estructura base de documentación SDD (`SDD/Docs/`, `SDD/Intake/`, `SDD/Maquetas/`).
- `SOLUTION-INTAKE-Sai-Service-Core-v1.0.md` (v1.0, estado borrador), documento de intake
  de la solución con:
  - **Parte A — Negocio**: idea y problema, audiencia y stakeholders, propuesta de valor,
    alcance funcional MoSCoW, historias de usuario, flujos típicos, casos límite, métricas
    de éxito, exclusiones, restricciones, riesgos y glosario del dominio (§1–§12).
  - **Parte B — Composición**: proyectos de la solución, estilo arquitectónico, esquema de
    descomposición y delivery, y estructura de repositorio (§13–§16).
  - **Parte C — Técnica**: bloque técnico de `Sai-Service-Core`, estrategia de demo/samples
    y checklist de completitud (§17–§19).
  - Sección de trazabilidad downstream y control de cambios.
- Definición del stack principal: .NET 10 + Blazor (interactive server) + Entity Framework
  Core + SQLite.

### Notas

- El intake marca explícitamente cada afirmación como **[derivado]** o **PENDIENTE** según
  su respaldo en las fuentes; los ítems PENDIENTE requieren respuesta del stakeholder antes
  de derivar el `SOLUTION-MANIFEST`.

## [0.0.1] - 2026-07-18

### Añadido

- Commit inicial: `README.md` y `.gitignore`.
