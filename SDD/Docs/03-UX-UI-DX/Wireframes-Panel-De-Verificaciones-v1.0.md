# Wireframes — Panel de verificaciones y ventana de mantenimiento

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Panel-De-Verificaciones-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Panel-De-Verificaciones**.

Superficie de supuestos y de la ventana de mantenimiento. Muestra el estado de los cuatro supuestos de los que depende el apagado automático (nunca verificado, verificado, vencido, refutado) con su evidencia, método y vigencia, y por qué la política está bloqueada. Desde acá el administrador, con presencia física, inicia la ventana de mantenimiento que verifica uno por uno los supuestos para desbloquear el apagado con evidencia. Es el flujo crítico que rara vez pasa y no puede fallar: es destructivo (corta la energía real). Origen: CU-10 (ventana de mantenimiento y verificación), con la regla de bloqueo de CU-05; UF-8.

## 2. Layout

Shell de trabajo. Dos zonas: el estado de los cuatro supuestos (siempre visible) y, al iniciar la ventana, un stepper de verificación paso por paso (patrón "Wizard / stepper" §4.5 del base) con confirmación explícita por paso.

```text
+----------------+-----------------------------------------------------------------+
| navegacion     |  Verificaciones                                       <sello>   |  <h1> + sello
|  · Verificac.<-|  [ banner: politica en Solo aviso · 0 de 4 verificados ]        |  §bloqueo, persistente
|                |  +--------- Estado de los supuestos ----------------------+     |
|                |  | ver-presupuesto-apagado   [badge Nunca]  vigencia 180 d |     |
|                |  | ver-flag-ob               [badge Nunca]  vigencia 365 d |     |
|                |  | ver-shutdown-return       [badge Nunca]  sin caducidad  |     |
|                |  | ver-bios-autoencendido    [badge Nunca]  vigencia 365 d |     |
|                |  +---------------------------------------------------------+     |
|                |  Requiere presencia fisica · procedimiento destructivo          |  advertencia
|                |                            [====== Iniciar ventana ======]      |
|                |                                                                 |
|                |  --- durante la ventana (stepper) ---                           |
|                |  ( 1 )--( 2 )--( 3 )--( 4 )   Paso 2 de 4                        |  §4.5 stepper
|                |  Paso: cronometrar el apagado del host bajo carga               |
|                |  Resultado observado: [ registrar ]      [ Confirmar paso ]      |  efecto observado
+----------------+-----------------------------------------------------------------+
```

Identificadores de supuestos tomados del intake (E-4/UF-8); ilustrativos del contenido, no maqueta.

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Banner de bloqueo por verificación | Declarar el bloqueo y la modalidad efectiva | motivo; "n de m verificados"; modalidad efectiva | Persistente mientras haya bloqueo; `role="alert"` |
| Lista de supuestos | Estado de cada uno de los cuatro | id, estado (badge), evidencia, método, vigencia | Estados: nunca verificado, verificado, vencido, refutado |
| Advertencia de procedimiento | Avisar que es destructivo y exige presencia física | texto de advertencia | Precede al inicio de la ventana |
| Acción "Iniciar ventana" | Abrir el stepper de verificación | — | Muestra el checklist paso por paso |
| Stepper de verificación (patrón §4.5) | Guiar la verificación uno por uno | paso actual, contador "Paso X de N" | Pendiente/actual/completado; no avanza sin confirmar el paso |
| Registro de resultado observado | Capturar la evidencia del paso | tiempo cronometrado, estado observado (OB, arranque) | Valida por efecto observado (RN-03), no por ausencia de error |
| Confirmación por paso | Cerrar cada verificación con su evidencia | — | Al confirmar, el supuesto pasa a verificado con su vigencia |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Iniciar ventana | Activar "Iniciar ventana" | Se abre el stepper con el primer paso | Presencia física (declarada por el operador) |
| Confirmar paso (presupuesto) | Registrar el tiempo de apagado | `ver-presupuesto-apagado` -> verificado, vigencia 180 días | Contenedores detenidos, apagado cronometrado |
| Confirmar paso (flag OB) | Observar `ups.status = OB` al cortar la red | `ver-flag-ob` -> verificado, vigencia 365 días | Corte de red al SAI |
| Confirmar paso (corte con retorno) | Ejecutar el `shutdown.return` controlado | `ver-shutdown-return` -> verificado, sin caducidad | El SAI corta y restablece la salida |
| Confirmar paso (autoencendido) | Observar si el host arranca solo | Si arranca: `ver-bios-autoencendido` -> verificado (365 d) y la modalidad ya es efectiva. Si no: -> refutado (bloqueo permanente) | Se restauró la energía |
| Renovación por evidencia | Un corte real seguido de arranque automático | `ver-bios-autoencendido` y `ver-flag-ob` se renuevan sin repetir la prueba destructiva | Evento de corte cruzado con la evidencia de arranque |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío (cuatro nunca verificados) | Tras el alta del parque, ninguno probado | Lista con los cuatro en "nunca verificado"; banner de bloqueo; modalidad solo aviso |
| Cargando (paso en ejecución) | Se está ejecutando o registrando un paso | Indicador de progreso del paso; el resto del stepper deshabilitado |
| Con datos (estado de supuestos) | Hay supuestos con algún estado distinto de nunca | Lista con badges de estado y vigencias; contador "n de m" |
| Error (paso fallido) | La acción no se validó por efecto observado | Mensaje que declara que el efecto no se observó; el paso no se da por hecho |
| Supuesto refutado (bloqueo permanente) | El host no arrancó solo tras la restauración | `ver-bios-autoencendido` en "refutado"; bloqueo permanente hasta resolverlo; distinto de vencido |
| Supuesto vencido | Pasó la vigencia (180 o 365 días) | Badge "vencido"; pide repetir la prueba, no bloquea permanentemente |
| Ventana en curso | El stepper está abierto | Stepper con el paso actual y el contador |
| Desbloqueado | 4 de 4 verificados | El banner de bloqueo desaparece; la modalidad efectiva deja de degradar |

## 6. Versión móvil o responsive

La lista de supuestos pasa a tarjetas apiladas. El stepper horizontal se vuelve vertical bajo ~768px, con el paso actual expandido y los demás colapsados. La barra lateral colapsa a drawer. El uso real de esta superficie es junto al servidor, con presencia física, por lo que la versión angosta debe ser plenamente operable. Legible a 320px.

## 7. Notas de implementación

- Accesibilidad: el estado del paso se anuncia por `aria-live`; el stepper declara el paso actual con `aria-current`; badges de estado de supuesto con texto además de color (nunca/verificado/vencido/refutado); banner de bloqueo `role="alert"`; foco gestionado al avanzar de paso; navegación por teclado completa.
- Seguridad operativa: es la superficie que materializa la regla de bloqueo (RN-01, RN-02) y la validación por efecto observado (RN-03). Ningún paso se da por hecho por ausencia de error. La distinción vencido/refutado es de primer orden: refutado bloquea, vencido solo pide repetir.
- Sin optimistic UI: en el camino crítico ninguna acción se muestra como hecha antes de confirmarse por efecto observado.
- El stepper es la única ceremonia multipaso admitida en la solución, y es correcta acá: la ventana de mantenimiento es un procedimiento físico secuencial; no contradice la regla de "un acto por superficie" del primer arranque, que es otra superficie.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único con presencia física (00, Visión §2) |
| CU origen | CU-10 (ventana de mantenimiento y verificación); CU-05 (regla de bloqueo); UF-8 |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §3.3 (desbloqueo del apagado real), §8 (bloqueo por verificación) |
| Reglas de negocio relevantes | RN-01 (arranque seguro), RN-02 (bloqueo por verificación), RN-03 (validación por efecto observado) |
| US a generar en 06 | US-10 (ventana de mantenimiento guiada), US-02 (visibilidad del bloqueo) |
| Tests previstos en 08 | Transición de supuestos a verificado con sus vigencias; refutación que bloquea; distinción entre vencido y refutado; validación por efecto observado; renovación por evidencia |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | N/A |
| Primer arranque aplicado | N/A |
| Acceso de operador único aplicado | sí (shell de trabajo) |
| Identidad de versión aplicada | sí (sello heredado del shell de trabajo) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación visual de maqueta | N/A (pendiente Fase B2) |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie Panel-De-Verificaciones: estado de los cuatro supuestos con sus vigencias, banner de bloqueo, ventana de mantenimiento como stepper con confirmación por paso y validación por efecto observado, distinción vencido/refutado, renovación por evidencia. Tabla de estados (vacío/cargando/con datos/error + refutado, vencido, ventana en curso, desbloqueado), responsive, accesibilidad AA, trazabilidad. Maqueta-aware. |
