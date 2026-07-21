# US-09 — Derivación de eventos y alerta de pérdida de comunicación

**Proyecto:** Sai-Service-Core
**Documento:** US-09-Derivacion-De-Eventos-Y-Alerta-De-Perdida-De-Comunicacion-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-05 Monitoreo, salud e históricos
**Prioridad MoSCoW:** Must
**Estimación:** 5 SP (Fibonacci)

## 1. Historia
Como administrador, quiero que el servicio derive eventos de energía por reglas versionadas y me alerte cuando pierde comunicación con el equipo, para no confundir un fallo del enlace con un equipo sano.

## 2. Contexto
NB-02 pide alerta de conectividad. CU-04 evalúa reglas de derivación sobre la ventana reciente y genera eventos (`Microcorte`, `CorteSuministro`, `RetornoRed`, `DesconexionUsb`, `TensionFueraDeRango`). Cada evento guarda su `reglaDerivacionId` y versión (CL-15). La validación por efecto observado (ADR-11) exige vigilar la propia conectividad: tres sondeos consecutivos sin respuesta generan `DesconexionUsb` y alerta visual (CL-14).

## 3. Criterios de aceptación
- Given una ventana reciente de muestras que cruza un umbral de una regla vigente, When el planificador la evalúa, Then genera el evento correspondiente guardando la regla y su versión.
- Given tres sondeos consecutivos sin respuesta del equipo, When se detecta la racha, Then el sistema genera `DesconexionUsb` y muestra la alerta de pérdida de comunicación en el panel.
- Given una sola muestra que atrapa un corte, When se evalúan las reglas, Then el evento se genera igual (no se exige dos muestras consecutivas, CL-11).
- Given una tensión sostenida fuera del rango de 198–242 V durante 30 s, When se evalúa, Then se genera `TensionFueraDeRango`.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-02 |
| CU cubiertos | CU-04 |
| BT derivadas | BT-17, BT-19, BT-24 |
| Tests previstos | acceptance/AT-09-eventos-perdida-comunicacion |

## 5. Prioridad y estimación
Must: sin derivación de eventos y alerta de conectividad el panel muestra un equipo aparentemente sano cuando el enlace cayó. 5 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-04
- [x] NB de origen (NB-02) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-17, BT-19 y BT-24, planificadas antes o en paralelo

## 7. Notas y supuestos
Un microcorte de 3 s con sondeo cada 5 s no es detectable de forma confiable (CL-10): la duración lleva incertidumbre estructural. Un informe que sume duraciones sin propagar la incertidumbre produce un total con error del 100 %.
