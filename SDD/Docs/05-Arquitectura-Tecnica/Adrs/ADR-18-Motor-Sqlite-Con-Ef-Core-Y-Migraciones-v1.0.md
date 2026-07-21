# ADR-18 — Motor SQLite con EF Core y migraciones versionadas

**Proyecto:** Sai-Service-Core
**Documento:** ADR-18-Motor-Sqlite-Con-Ef-Core-Y-Migraciones-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Persistencia

## 1. Contexto

El sistema persiste catálogo, inventario, vínculos temporales e historia append-only, con un volumen de ~6,3 millones de filas/año a 5 s de intervalo, retención por agregación (muestras `P30D`, agregados `PT1H` por `P10Y`, eventos indefinidos) y un único proceso escritor. La decisión PA-04 fija la disciplina append-only pero no el motor ni el mecanismo de esquema. El stack impone SQLite y EF Core. Este ADR cierra el ADR obligatorio de web-monolith «persistencia» en su dimensión de motor y versionado de esquema. Motivan la decisión los casos de uso CU-06 (históricos) y CU-12 (informes), y los NFR de volumen y retención de §17 P.10.

## 2. Decisión

El motor es SQLite, accedido con Entity Framework Core. El esquema se versiona con migraciones de EF Core, guardadas en el repositorio y aplicadas al arranque del servicio; no hay generación automática de esquema en producción. Un solo archivo SQLite, un solo proceso escritor. La agregación y la retención (muestras a 30 días, agregados horarios a 10 años, eventos indefinidos) se ejecutan como proceso interno.

## 3. Estado

Aceptado el 2026-07-20. ADR obligatorio de web-monolith (persistencia, motor y migraciones), complementa a PA-04 (ADR-04). Motor impuesto por el stack (§17 P.1, P.4).

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| SQLite + EF Core + migraciones versionadas | Un archivo, respaldo trivial, sin servidor de base; adecuado a un proceso escritor único; migraciones revisables | Sin concurrencia de escritura ni escalado horizontal (aceptado, T-01); tamaño máximo tras agregación sin dimensionar (P-08) |
| Motor cliente-servidor (relacional en proceso aparte) | Concurrencia, escalado | Infraestructura sin contrapartida: un usuario, un dispositivo, un escritor (T-01) |
| Generación automática de esquema (sin migraciones) | Menos archivos | Imposible auditar cambios; anti-patrón de modelo sin migración versionada (05-Rules §4.7) |

## 5. Consecuencias positivas

1. Respaldo del servicio = copia de un único archivo; el rollback de versión no pierde hechos porque la historia es append-only (ADR-04, P.8).
2. Cada migración es un archivo versionado y revisable; el esquema se reconstruye y audita (evita el anti-patrón de modelo sin migración).
3. Adecuado al patrón de acceso: un proceso escritor, sin contención de escritura que justifique un motor cliente-servidor (T-01).

## 6. Consecuencias negativas y trade-offs

1. Se renuncia a la escalabilidad horizontal y a la concurrencia de escritura (T-01); no hay requisito que las pida.
2. El tamaño máximo del archivo tras la agregación no está dimensionado (P-08, riesgo R-07): se valida antes de producción.
3. Las migraciones se diseñan aditivas siempre que sea posible; una migración destructiva exige respaldo previo del archivo (P.8).

## 7. Implementación

EF Core con proveedor SQLite en `Infrastructure`; el `Domain` no lo referencia (ADR-15). Migraciones versionadas en el repositorio, aplicadas al arranque. El mapeo cubre `Valor<T>` con `Origen` (ADR-06), los tipos separados `Muestra`/`Agregado` (ADR-08) y las tablas de Identity (ADR-16). La procedencia `o` y la lista `de` se declaran una vez por `SesionSondeo` (P.4). Un proceso interno agrega muestras a `PT1H` y descarta las de más de 30 días, preservando min/max/promedio de `input.voltage` y poblando `cobertura` (ADR-08, I-20). El detalle vive en `Modelo-Datos-Logico` de esta categoría.

## 8. Métricas de validación

- Integración: EF Core contra SQLite físico con migraciones aplicadas (P.6, quality gate 4).
- La agregación reduce muestras `P30D` a agregados `PT1H` sin perder microcortes (min/max preservados).
- P-08/R-07: tamaño del archivo tras un año de agregación validado antes de producción.

## 9. Referencias

- Intake §17 P.1, P.4, P.10, P.8; trade-off T-01; pendiente P-08; riesgo R-07.
- CU-06 Históricos y gráficas; CU-12 Informe de período; F-19.
- ADR relacionadas: ADR-04, ADR-06, ADR-08, ADR-15, ADR-16.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Motor de persistencia, complementa a ADR-04. |
