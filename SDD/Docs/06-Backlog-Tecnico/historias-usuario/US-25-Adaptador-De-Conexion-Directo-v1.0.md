# US-25 — Adaptador de conexión directo para equipos no cubiertos por la herramienta de acceso al SAI

**Proyecto:** Sai-Service-Core
**Documento:** US-25-Adaptador-De-Conexion-Directo-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-07 Integración e informes
**Prioridad MoSCoW:** Could
**Estimación:** 5 SP (Fibonacci)

## 1. Historia
Como administrador, quiero poder conectar un equipo que la herramienta de acceso al SAI no cubra a través de un adaptador de conexión directo, para no quedar limitado al conjunto de equipos que esa herramienta soporta hoy.

## 2. Contexto
F-27 (Could) prevé un adaptador de conexión directo sin la herramienta de acceso al SAI. El puerto del adaptador se diseña con tres implementaciones (ADR-02): la herramienta de acceso al SAI en la primera entrega, el adaptador simulado para pruebas y el directo diseñado pero no implementado en v1. Esta historia documenta la capacidad para v2 y consume el contrato del puerto (BT-14) como punto de extensión, sin abrir una implementación en la primera entrega.

## 3. Criterios de aceptación
- Given un equipo que la herramienta de acceso al SAI no soporta, When existe una implementación directa del puerto del adaptador, Then el sistema puede leer estado, probar conexión, ordenar el apagado con retorno y lanzar la prueba de batería contra ese equipo.
- Given el adaptador directo, When se conecta un equipo, Then respeta el mismo contrato de validación por efecto observado (ADR-11) que la implementación basada en la herramienta de acceso al SAI.
- Given la primera entrega, When se revisa el alcance, Then la implementación directa queda diseñada y no construida, y el sistema opera con la implementación basada en la herramienta de acceso al SAI.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-02 |
| CU cubiertos | CU-02, CU-04 |
| BT derivadas | BT-14 |
| Tests previstos | acceptance/AT-25-adaptador-directo (diferido a v2) |

## 5. Prioridad y estimación
Could: agrega valor si aparece un equipo no cubierto por la herramienta de acceso al SAI, pero puede esperar a v2; el equipo actual ya está resuelto por esa herramienta y verificado. 5 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-02 y CU-04
- [x] NB de origen (NB-02) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [ ] Datos de prueba disponibles (pendiente: requiere un equipo no cubierto por la herramienta de acceso al SAI; excepción DoR §3)

## 7. Notas y supuestos
La US no se promueve a Ready hasta que exista un equipo que la justifique, y solo sobre un SAI de banco con verdad de referencia instrumental (E-07). El contrato de tres implementaciones del puerto (BT-14) es la superficie que la hace posible sin refactor.

## 8. Control de cambios
| Versión | Fecha | Motivo |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Versión inicial de la historia. |
| 1.1 | 2026-07-21 | Corrección de conformidad: abstracción de nombres de stack a capacidad + ADR tras audit de Fase D. |
