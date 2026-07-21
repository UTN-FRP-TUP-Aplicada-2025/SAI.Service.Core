# 03-UX-UI-DX — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Categoría:** 03-UX-UI-DX
**Variante aplicada:** UX/UI (project_type: web-monolith, con UI final)
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)

Índice navegable de la categoría 03 para Sai-Service-Core: el panel de control web de un servicio que monitorea un SAI, ejecuta el apagado ordenado con reencendido garantizado, administra los equipos y expone una API REST. Un solo administrador, acceso por LAN. Layout aplanado (caso de un solo proyecto): los artefactos viven en `SDD/Docs/03-UX-UI-DX/`, sin subnivel `Proyectos/`.

Esta categoría es maqueta-aware: `requiere_maqueta` propuesto en true. Cada wireframe declara un nombre canónico de superficie estable y su tabla de estados enumera todos los estados que la maqueta de la Fase B2 deberá demostrar. La validación visual y los tres artefactos que deposita la fase (`Linea-Base-Visual`, `Contrato-Datos-Maqueta`, `Bitacora-Validacion-Maqueta`) quedan pendientes de la Fase B2.

## Artefactos

| Artefacto | Propósito | Estado |
| --- | --- | --- |
| `Experiencia-De-Uso-v1.0.md` | Marco de experiencia: audiencia, principios (Nielsen + leyes UX), frontera aplicación/entorno, flujos clave, estados por superficie, accesibilidad AA, i18n, performance percibida, errores y recuperación. Las once secciones del §4.2. | Borrador |
| `Wireframes-Alta-Inicial-Administrador-v1.0.md` | Superficie de aprovisionamiento del primer arranque, sin chrome ni cancelar. Origen CU-01 / UF-1. | Borrador |
| `Wireframes-Acceso-Login-v1.0.md` | Superficie de acceso de operador único, con sello de versión. Origen CU-01. | Borrador |
| `Wireframes-Panel-Estado-En-Vivo-v1.0.md` | Home/dashboard: estado del SAI, conectividad, supuestos, eventos recientes, banner de bloqueo. Superficie central. Origen CU-04 / CU-05 / UF-3. | Borrador |
| `Wireframes-Alta-De-Equipos-v1.0.md` | Catálogo e inventario, descubrimiento del dispositivo, siembra de verificaciones. Origen CU-02 / UF-1. | Borrador |
| `Wireframes-Configuracion-De-Politicas-v1.0.md` | Configuración dirigida por esquema de la política de apagado. Origen CU-03 / UF-2. | Borrador |
| `Wireframes-Prueba-De-Bateria-v1.0.md` | Prueba de batería a 1 Hz y veredicto de salud con confianza y reserva. Origen CU-07 / UF-5. | Borrador |
| `Wireframes-Historicos-Y-Graficas-v1.0.md` | Series de muestras y agregados con marcas de evento y cobertura. Origen CU-06 / UF-4. | Borrador |
| `Wireframes-Panel-De-Verificaciones-v1.0.md` | Supuestos y ventana de mantenimiento (stepper destructivo). Origen CU-10 / CU-05 / UF-8. | Borrador |
| `Wireframes-Registro-De-Intervenciones-v1.0.md` | Registro de intervención de servicio técnico con costos, cuadre y ficha de vida útil. Origen CU-08 / UF-6. | Borrador |
| `Wireframes-Sustitucion-Del-SAI-v1.0.md` | Ciclo de vida del SAI: reparación, cobertura suplente y días sin protección. Origen CU-09 / UF-7. | Borrador |
| `Wireframes-Informe-De-Periodo-v1.0.md` | Informe de período y comparación de marcas por costo por año en USD. Origen CU-12 / UF-9. | Borrador |
| `Linea-Base-Visual-v1.0.md` | Línea de base visual de la maqueta: capturas por superficie (SUP-XX) y su cobertura de estados. Fase B2 (validación visual). | Fase B2 |
| `Contrato-Datos-Maqueta-v1.0.md` | Contrato de datos que consume la maqueta por superficie. Fase B2 (validación visual). | Fase B2 |
| `Bitacora-Validacion-Maqueta-v1.0.md` | Bitácora de la validación visual de la maqueta y su resultado. Fase B2 (validación visual). | Fase B2 |
| `Glosario-UX-v1.0.md` | Terminología canónica de la sección; referencia al glosario de dominio de 02 sin duplicarlo. | Borrador |
| `README.md` | Este índice. | Borrador |

## Superficies y estados (mapa de cobertura maqueta-aware)

| Superficie (nombre canónico) | CU origen | Estados declarados |
| --- | --- | --- |
| Alta-Inicial-Administrador | CU-01 | vacío (N/A, acto único), cargando (resolviendo destino), con datos, error (requisito / confirmación / fuera de tiempo), enviando, aprovisionado |
| Acceso-Login | CU-01 | vacío (N/A), cargando, con datos, error (credenciales / restricción temporal / formulario vencido), identidad creada, contraseña actualizada, sesión expirada |
| Panel-Estado-En-Vivo | CU-04, CU-05 | vacío (orientación), cargando, con datos, error (sin conexión SAI / circuito caído), política degradada, tensión fuera de rango |
| Alta-De-Equipos | CU-02 | vacío (sin dispositivos), cargando, con datos, error (conexión / dato inválido), descubierto sin marca |
| Configuracion-De-Politicas | CU-03 | vacío (defaults), cargando, con datos, error (fuera de límites / propuesta rechazada), simulación, previsualización, ranura deshabilitada |
| Prueba-De-Bateria | CU-07 | vacío (sin pruebas), cargando (1 Hz), con datos (veredicto), error (precondición / muestras perdidas), no comparable |
| Historicos-Y-Graficas | CU-06 | vacío (sin datos), cargando, con datos (muestras / agregados), error (cobertura insuficiente), serie con advertencia |
| Panel-De-Verificaciones | CU-10, CU-05 | vacío (4 nunca verificados), cargando (paso), con datos, error (paso fallido), refutado, vencido, ventana en curso, desbloqueado |
| Registro-De-Intervenciones | CU-08 | vacío (sin intervenciones), cargando, con datos, error (costos no cuadran / importe sin moneda o fecha / coherencia temporal), efecto aplicado, fuente externa (confianza media) |
| Sustitucion-Del-SAI | CU-09 | vacío (sin sucesión), cargando, con datos, error (cobertura solapada / coherencia temporal), host sin cobertura (alerta), cobertura suplente activa, SAI en reparación |
| Informe-De-Periodo | CU-12 | vacío (sin selección), cargando, con datos, error (período sin datos / agregado sin cobertura), informe con advertencia de cobertura, comparación con confianza baja |

## Catálogo de diseño aplicado

Insumo normativo consumido vía `References/Design/Index-Design-Rules.md`:

- `Design-Rules-Web-Generico-v1.0.md` (base, siempre).
- `Design-Rules-Blazor-Mudblazor-v1.0.md` (especialización de stack: Blazor Interactive Server + MudBlazor, declarado en el intake §17 P.1).
- `Design-Rules-Config-Esquema-v1.0.md` (extensión: configuración de políticas dirigida por esquema).
- `Design-Rules-Primer-Arranque-v1.0.md` (extensión: alta inicial del administrador, arranque sin config mínima).
- `Design-Rules-Acceso-Monousuario-v1.0.md` (extensión: una sola identidad de operación).
- `Design-Rules-Identidad-De-Version-v1.0.md` (extensión: identidad de versión de la imagen desplegable).

Este proyecto es el arquetipo de panel monolítico y carga las cuatro extensiones a la vez, además del base y la especialización de stack. Modelo UX-UI de `Modelos-UX-UI/`: no se eligió uno; rige el catálogo base (la elección ocurre en el paso 1 de la Fase B2).

## Trazabilidad de la categoría

- Upstream: 00 (Visión-Producto §2 persona objetivo; Alcance; Compatibilidad) y 02 (CU-01..CU-12, las 13 RN, el Modelo-Conceptual). Intake SOLUTION-INTAKE-Sai-Service-Core-v1.0 (§5 historias, §6 flujos, §17 P.5 seguridad/auth, P.10 NFR, §20 escenarios E-1..E-8).
- Downstream: alimenta 05-Arquitectura-Tecnica (requisitos de la capa de presentación; contrato de descriptores, predicado de aprovisionamiento, contrato de identidad de versión), 06-Backlog-Tecnico (US con criterios de aceptación visuales) y 08-Calidad-Y-Pruebas (snapshot, accesibilidad AA, estados por superficie).
- Pendientes referidos: TLS del panel/API (P-04, configuración de entorno), contrato del endpoint de rectificación del 409 (P-05), SLO y NFR de panel/accesibilidad/i18n formal (P-09, se cierran en 08).

## Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Índice inicial de la categoría 03 (variante UX/UI) para Sai-Service-Core: marco de experiencia, ocho wireframes con nombre canónico, glosario UX, catálogo de diseño aplicado y mapa de superficies/estados maqueta-aware. |
| 1.1 | 2026-07-20 | Cobertura de wireframes completada (hallazgo H-6 del audit de Fase B): se suman tres superficies maqueta-aware — Registro-De-Intervenciones (CU-08), Sustitucion-Del-SAI (CU-09) e Informe-De-Periodo (CU-12) —, total 11 wireframes. Actualizados el índice de artefactos y el mapa de superficies/estados. |
| 1.2 | 2026-07-20 | Retroalimentación de la Fase B2 de validación de maqueta: unificación de 'parque' → 'equipos' y 'secreto' → 'contraseña'; superficie renombrada a Alta-De-Equipos (CU-02); alta de tres artefactos de validación visual en el índice (Linea-Base-Visual, Contrato-Datos-Maqueta, Bitacora-Validacion-Maqueta). |
