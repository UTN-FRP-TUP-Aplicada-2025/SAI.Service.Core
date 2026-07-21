# ADR-06 — Procedencia obligatoria en todo valor almacenado

**Proyecto:** Sai-Service-Core
**Documento:** ADR-06-Procedencia-Obligatoria-En-Todo-Valor-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Persistencia

## 1. Contexto

El modo de falla más probable del sistema no es un error de código sino una conclusión falsa: guardar `battery.charge` (que el driver interpola) como si fuera un dato medido, y construir sobre él una decisión de salud (riesgo R-13, O-M18, severidad Alta). El sistema debe responder sin leer código a la pregunta «¿este número lo midió el aparato o lo calculó el software?». Motivan la decisión la regla conceptual RC-01 (procedencia por valor), la regla de negocio RN-05 (procedencia obligatoria) y las necesidades NB-03 (historia trazable) y NB-06 (salud de baterías).

## 2. Decisión

Todo valor almacenado lleva su `Origen` declarado, sin excepción (invariante I-7). Se modela un `Valor<T>` con `Origen` que puede ser `medido`, `derivado`, `estimadoPorDriver`, `declarado`, `imputado` o `noCalculable`. Un valor `derivado` declara `de` con al menos una variable de la que deriva (I-8). No se guarda «solo el número».

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-06 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| `Valor<T>` con `Origen` obligatorio | Trazabilidad total del origen; `aptoParaTendenciaDeSalud()` puede excluir derivados; mitiga R-13 | Cada valor ocupa más; obliga a poblar el origen en toda escritura |
| Guardar solo el número | Modelo mínimo | Es exactamente el modo de falla R-13; imposible distinguir medido de interpolado |
| Origen a nivel de tabla, no de valor | Menos repetición | No distingue variables medidas de derivadas dentro de la misma muestra |

## 5. Consecuencias positivas

1. `aptoParaTendenciaDeSalud()` devuelve `false` para `derivado`, `estimadoPorDriver` e `imputado` (I-9, RN-06): la salud nunca se calcula sobre datos que no la soportan.
2. El panel marca en pantalla todo valor derivado o estimado (por ejemplo `battery.charge` como derivado), evitando la conclusión falsa.
3. La potencia nominal desconocida queda `null` con procedencia `imputado`, nunca un número inventado (CL-19, UF-1).

## 6. Consecuencias negativas y trade-offs

1. Cada valor cuesta más almacenamiento; se mitiga declarando `o` y `de` una vez por `SesionSondeo`, no por muestra (P.4), y expandiendo solo en la API por legibilidad.
2. Toda ruta de escritura debe poblar el origen: es una carga transversal verificada por el invariante I-7.
3. El modelo de datos gana complejidad de tipos (`Valor<T>`) que el ORM debe mapear (ADR-18).

## 7. Implementación

Tipo `Valor<T>` (con `Origen` y lista `de`) en `Domain`. La procedencia y la lista de derivación se declaran una vez por `SesionSondeo` (P.4) para no duplicar constantes ~17.280 veces por día; se expanden en la API. La API de ingesta asigna confianza `media` a la fuente externa, menor que la del poller local (ADR-17). Prueba de invariante I-7 en el pipeline (M-05, objetivo cero excepciones).

## 8. Métricas de validación

- I-7 verificado como test de invariante: cero valores almacenados sin `Origen` (M-05, target 0).
- I-9: `aptoParaTendenciaDeSalud()` devuelve `false` para orígenes no aptos.
- El panel señala en pantalla todo valor derivado o estimado (CU-04).

## 9. Referencias

- Intake §17 P.4, P.10 (I-7, I-8, I-9), P.11 (PA-06); riesgo R-13; CL-19; métrica M-05.
- RC-01 Procedencia por valor; RN-05 Procedencia obligatoria de todo valor; RN-06 Aptitud de datos para tendencia de salud.
- NB-03 Historia trazable con procedencia; NB-06 Evaluación de salud de baterías. CU-04, CU-07.
- ADR relacionadas: ADR-13, ADR-17, ADR-18.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-06. |
