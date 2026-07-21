# US-26 — Capa de add-ons de dialecto de protocolo (diseñada)

**Proyecto:** Sai-Service-Core
**Documento:** US-26-Capa-De-Add-Ons-De-Dialecto-De-Protocolo-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-07 Integración e informes
**Prioridad MoSCoW:** Could
**Estimación:** 3 SP (Fibonacci)

## 1. Historia
Como administrador, quiero poder extender el soporte de dialectos de protocolo mediante add-ons, para caracterizar un equipo nuevo sin tener que modificar el núcleo del servicio.

## 2. Contexto
F-26 (Could) prevé una capa de add-ons de dialecto de protocolo, diseñada pero no implementada en la primera entrega: su interfaz «no tiene sentido especificarla antes de tener el servicio». La superficie de extensión es el puerto del adaptador de tres implementaciones (ADR-02, BT-14) y la documentación de extensibilidad de 05. Esta historia documenta la capacidad para v2 y su punto de enganche, sin abrir una implementación en v1.

## 3. Criterios de aceptación
- Given un equipo con un dialecto no soportado, When existe la capa de add-ons, Then se puede registrar un add-on de dialecto que el adaptador use sin modificar el núcleo del servicio.
- Given un add-on de dialecto, When se caracteriza un equipo, Then el sistema dispara el procedimiento de caracterización descripto para una sustitución por otro modelo (CL-27).
- Given la primera entrega, When se revisa el alcance, Then la capa queda diseñada y documentada en extensibilidad, sin implementación construida.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-02 |
| CU cubiertos | CU-04 |
| BT derivadas | BT-14 |
| Tests previstos | acceptance/AT-26-add-ons-dialecto (diferido a v2) |

## 5. Prioridad y estimación
Could: solo aporta cuando aparezca un equipo que la herramienta de acceso al SAI no soporte y sobre un SAI de banco con verdad de referencia instrumental; puede esperar a v2. 3 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-04
- [x] NB de origen (NB-02) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [ ] Datos de prueba disponibles (pendiente: requiere un equipo con dialecto no soportado; excepción DoR §3)

## 7. Notas y supuestos
La escritura del traductor del equipo actual está fuera de alcance (F-33, Won't v1): ya resuelto y verificado por la herramienta de acceso al SAI. Esta capa es para equipos futuros, no para el actual. Su interfaz se especifica cuando el servicio exista, no antes.

## 8. Control de cambios
| Versión | Fecha | Motivo |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Versión inicial de la historia. |
| 1.1 | 2026-07-21 | Corrección de conformidad: abstracción de nombres de stack a capacidad + ADR tras audit de Fase D. |
