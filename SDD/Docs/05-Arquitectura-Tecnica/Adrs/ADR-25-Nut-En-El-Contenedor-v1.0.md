# ADR-25 — NUT en el contenedor

**Proyecto:** Sai-Service-Core
**Documento:** ADR-25-Nut-En-El-Contenedor-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Despliegue

## 1. Contexto

ADR-19 dejó abierta, como decisión de Sprint 0 (pendiente P-03), la ubicación de NUT (la herramienta de acceso al SAI, ADR-01): dentro del contenedor del servicio o en el host con el servicio como cliente TCP de `upsd`. La decisión debía garantizar que un único consumidor posea el nodo USB anclado por ruta física de puerto (ADR-03), resolviendo la competencia por el nodo (O-U1/O-U2, riesgos R-05/R-08). Este ADR cierra esa decisión en el Sprint 0.

## 2. Decisión

NUT corre **dentro del contenedor del servicio**. La imagen incluye el driver `nutdrv_qx` y `upsd`; el contenedor recibe el dispositivo USB del host anclado por ruta física de puerto (regla `udev` de ADR-03, mapeada al contenedor por su ruta `by-path`). El servicio es el único consumidor del nodo USB: en el host no corre una instancia de NUT que compita por el dispositivo. Es un único artefacto desplegable.

## 3. Estado

Aceptado — 2026-07-21. Supera a ADR-19, que pasa a estado `Superado por ADR-25`.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
|---|---|---|
| NUT en el contenedor (elegida) | Un único artefacto desplegable; el respaldo es una sola imagen; coherente con la simplicidad operativa de un solo host y un solo administrador (T-01) | El contenedor necesita el dispositivo USB mapeado; hay que garantizar que el host no levante NUT sobre el mismo nodo |
| NUT en el host, servicio cliente TCP de `upsd` | El contenedor no necesita el dispositivo | Dos piezas a operar; credenciales de `upsd` como secreto en runtime; competencia con NUT del host (O-U1) |
| Ambas conviviendo sin coordinación | — | Descartada en ADR-19: dos procesos disputan el nodo USB |

## 5. Consecuencias positivas

1. Un solo artefacto desplegable y un solo respaldo (la imagen), alineado con la simplicidad operativa buscada.
2. El anclaje del USB por ruta física (ADR-03) se mapea al contenedor sin intermediario en el host.
3. No hay credenciales de `upsd` que gestionar como secreto de red entre host y contenedor.

## 6. Consecuencias negativas y trade-offs

1. El contenedor debe recibir el dispositivo USB (por ruta física de puerto); el `docker run`/compose declara el mapeo del dispositivo. Trade-off aceptado.
2. El host no debe levantar su propia instancia de NUT sobre el mismo nodo; se documenta como precondición operativa de PROD.
3. La imagen es más grande (incluye el stack NUT); aceptable para un único despliegue.

## 7. Implementación

La imagen de PROD (runtime-only, ADR de la guía de publicación) incluye `nut` (`nutdrv_qx`, `upsd`, `upsc`). El `Entornos-Deploy` de 09 declara el mapeo del dispositivo USB por ruta física al contenedor y la precondición de que el host no ocupe el nodo. El adaptador NUT de `Infrastructure` habla con `upsd` local (dentro del contenedor). En DEV, el Dev Container puede correr con el adaptador simulado sin dispositivo.

## 8. Métricas de validación

- El contenedor de PROD accede al SAI por su nodo USB anclado por ruta física; `upsc` dentro del contenedor lista las variables del equipo.
- No hay un proceso NUT en el host compitiendo por el nodo (verificable por inspección del host).

## 9. Referencias

- ADR-19 (decisión abierta que este ADR cierra), ADR-01 (NUT como acceso), ADR-03 (anclaje del USB por ruta física), ADR-24 (ambientes; el mapeo del dispositivo vive en PROD).
- Intake §17 P.11 (P-03), §17 P.8 (empaquetado), riesgos R-05/R-08, caso límite CL-28.

## 10. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Cierre de la decisión de Sprint 0 (P-03): NUT corre en el contenedor. Supera a ADR-19. |
