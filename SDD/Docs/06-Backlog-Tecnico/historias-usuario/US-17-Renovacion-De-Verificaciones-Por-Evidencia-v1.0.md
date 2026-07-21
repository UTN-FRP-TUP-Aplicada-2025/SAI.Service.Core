# US-17 — Renovación de verificaciones por evidencia

**Proyecto:** Sai-Service-Core
**Documento:** US-17-Renovacion-De-Verificaciones-Por-Evidencia-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-06 Verificación y ciclo de vida de los equipos
**Prioridad MoSCoW:** Should
**Estimación:** 5 SP (Fibonacci)

## 1. Historia
Como administrador, quiero que un corte real que muestre la señal en batería o un corte seguido de arranque automático renueve el supuesto correspondiente, para no tener que repetir la ventana de mantenimiento destructiva cada vez que vence.

## 2. Contexto
NB-05 y F-25: la verificación por comportamiento (ADR-14) sigue viva sin repetir la prueba destructiva. CU-10 (FA-2) y CU-05 renuevan supuestos por evidencia: un corte real que muestre `OB` renueva `ver-flag-ob`; un corte seguido de arranque automático renueva `ver-bios-autoencendido`, cruzando los eventos propios contra los registros de arranque del sistema operativo del host.

## 3. Criterios de aceptación
- Given un corte real observado con la señal en batería, When el sistema lo registra, Then renueva `ver-flag-ob` extendiendo su vigencia sin abrir una ventana de mantenimiento.
- Given un corte seguido de un arranque automático confirmado contra los registros de arranque del sistema operativo, When se cruzan los eventos, Then renueva `ver-bios-autoencendido`.
- Given un arranque que en los registros de arranque del sistema operativo figura como `crash` y no como reinicio limpio tras el corte, When se evalúa, Then el sistema no renueva el supuesto y lo mantiene según su vigencia previa.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-05 |
| CU cubiertos | CU-10, CU-05 |
| BT derivadas | BT-25 |
| Tests previstos | acceptance/AT-17-renovacion-por-evidencia |

## 5. Prioridad y estimación
Should: el MVP funciona con la verificación manual de US-16, pero la renovación automática evita repetir la prueba destructiva y mantiene el sistema habilitado con menos fricción. 5 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-10 y CU-05
- [x] NB de origen (NB-05) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-25 y de US-16 (verificación inicial), planificadas antes

## 7. Notas y supuestos
La renovación aplica solo a los supuestos verificables por comportamiento; el presupuesto de apagado (vigencia 180 días) sigue exigiendo cronometrar bajo carga en la ventana de mantenimiento, porque la carga del host cambia (CL-05).

## 8. Control de cambios
| Versión | Fecha | Motivo |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Versión inicial de la historia. |
| 1.1 | 2026-07-21 | Corrección de conformidad: abstracción de nombres de stack a capacidad + ADR tras audit de Fase D. |
