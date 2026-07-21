# US-03 — Descubrimiento del dispositivo y prueba de conexión

**Proyecto:** Sai-Service-Core
**Documento:** US-03-Descubrimiento-Del-Dispositivo-Y-Prueba-De-Conexion-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-04 Alta de equipos y políticas de apagado
**Prioridad MoSCoW:** Must
**Estimación:** 5 SP (Fibonacci)

## 1. Historia
Como administrador, quiero descubrir el dispositivo desde el panel y probar la conexión, para empezar a registrar historia sin editar archivos de configuración a mano.

## 2. Contexto
NB-04 (ciclo de vida) y NB-02 (monitoreo) requieren identificar el equipo físico. CU-02 describe que el adaptador lista los candidatos y devuelve sus descriptores (por ejemplo `0665:5161 · INNO TECH · iSerial vacío`). El anclaje del USB por ruta física de puerto (ADR-03) hace que el binding no se rompa entre reconexiones.

## 3. Criterios de aceptación
- Given un equipo conectado físicamente al host, When el administrador pide descubrir dispositivos, Then el panel lista los candidatos con sus descriptores y permite anclarlo por su ruta física de puerto.
- Given un candidato seleccionado, When el administrador ejecuta la prueba de conexión, Then el sistema confirma el diálogo con el equipo por efecto observado y habilita continuar el alta.
- Given un candidato cuyo diálogo no responde, When se ejecuta la prueba de conexión, Then el sistema informa PRUEBA_CONEXION_FALLIDA y no permite continuar el alta.
- Given que no aparece ningún candidato en el bus, When se pide descubrir, Then el sistema informa DISPOSITIVO_NO_DESCUBIERTO sin inventar un dispositivo.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-04, NB-02 |
| CU cubiertos | CU-02 |
| BT derivadas | BT-01, BT-13, BT-14, BT-15 |
| Tests previstos | acceptance/AT-03-descubrimiento-prueba-conexion |

## 5. Prioridad y estimación
Must: sin identificar el equipo no hay sobre qué registrar historia ni monitorear. 5 SP por la interacción con el adaptador y el anclaje físico; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-02
- [x] NB de origen (NB-04) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-01 (ubicación de la herramienta de acceso al SAI), BT-13, BT-14 y BT-15, planificadas antes

## 7. Notas y supuestos
La ubicación de la herramienta de acceso al equipo (contenedor o host) se cierra en BT-01 antes de esta US. La escritura de un traductor de protocolo propio está fuera de alcance (F-33, Won't v1).

## 8. Control de cambios
| Versión | Fecha | Motivo |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Versión inicial de la historia. |
| 1.1 | 2026-07-21 | Corrección de conformidad: abstracción de nombres de stack a capacidad + ADR tras audit de Fase D. |
