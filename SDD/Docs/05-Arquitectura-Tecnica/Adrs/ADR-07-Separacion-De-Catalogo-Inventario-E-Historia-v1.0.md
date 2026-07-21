# ADR-07 — Separación de catálogo, inventario e historia

**Proyecto:** Sai-Service-Core
**Documento:** ADR-07-Separacion-De-Catalogo-Inventario-E-Historia-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Persistencia

## 1. Contexto

Para comparar marcas y modelos por costo por año de servicio, el sistema debe distinguir el modelo del producto (lo que se compra) de la unidad física (el ejemplar concreto) y de la historia (lo que le pasó). Sin esa distinción, agrupar fichas de vida útil por modelo es imposible (carencia C-2). Motivan la decisión el caso de uso CU-12 (informe y comparación de marcas), CU-08 (ficha de vida útil) y la necesidad NB-04 (ciclo de vida de los equipos).

## 2. Decisión

El modelo se organiza en cuatro capas separadas: catálogo (qué es), inventario (cuál es, con baja lógica), vínculos temporales (ADR-05) e historia (append-only, ADR-04). Catálogo: `Fabricante`, `ModeloDispositivo`, `ModeloBateria`, `TipoIntervencion`, `Proveedor`. Inventario: `Host`, `Dispositivo`, `Bateria`, todas derivadas de `UnidadFisica` con `estado`, `fechaBaja` y `motivoBaja`. La historia guarda dispositivo e instante, no la batería directamente.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-07 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Catálogo / inventario / historia separados | Permite agrupar fichas de vida útil por modelo; comparación de marcas; baja lógica | Más entidades y relaciones que mantener |
| Una sola tabla de dispositivos | Modelo mínimo | No distingue modelo de unidad física; comparar marcas es imposible (C-2) |
| Catálogo embebido como texto en cada unidad | Sin joins | Duplica datos de modelo; corromper la comparación por variantes de escritura |

## 5. Consecuencias positivas

1. Agrupar `FichaVidaUtil` por `ModeloBateria` habilita `costoPorAnioDeServicio` normalizado a USD (CU-12, F-23).
2. La baja lógica (`estado` + `fechaBaja` + `motivoBaja`) reemplaza el borrado físico, que no existe en este dominio (CL-20, RN-12).
3. La historia desacoplada de la batería permite la reatribución temporal (ADR-05).

## 6. Consecuencias negativas y trade-offs

1. Más entidades y joins; el modelo lógico (05, `Modelo-Datos-Logico`) debe documentarlos con cuidado.
2. La coherencia entre capas (una unidad referencia un modelo del catálogo) exige claves foráneas y validación.
3. La distinción modelo/unidad obliga a un alta en dos pasos (catálogo primero, inventario después) en UF-1.

## 7. Implementación

Jerarquía `UnidadFisica` → `Host`/`Dispositivo`/`Bateria` en `Domain`, con `estado` (`EnStock`, `EnServicio`, `EnReparacion`, `DadoDeBaja`). Catálogo como entidades propias. La baja lógica se aplica siempre; una entidad dada de baja se consulta con su historial pero no se opera: una intervención fechada después de la baja se rechaza con 422 `coherencia_temporal` (ADR-17, RN-12). Persistido con EF Core (ADR-18).

## 8. Métricas de validación

- CU-12: comparación de al menos dos modelos por `costoPorAnioDeServicio` (M-04).
- CL-20/RN-12: intervención posterior a la baja rechazada con 422 `coherencia_temporal`.
- Ninguna entidad borrada físicamente en las pruebas de dominio.

## 9. Referencias

- Intake §17 P.4, P.11 (PA-07); CL-19, CL-20; métrica M-04.
- RN-12 Baja lógica y coherencia temporal; RC-08.
- CU-08 Recambio de batería y ficha de vida útil; CU-12 Informe de período y comparación de marcas; NB-04.
- ADR relacionadas: ADR-04, ADR-05, ADR-17, ADR-18.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-07. |
