# ADR-19 — Ubicación de NUT: dentro del contenedor o en el host

**Proyecto:** Sai-Service-Core
**Documento:** ADR-19-Ubicacion-De-Nut-Contenedor-O-Host-v1.0.md
**Versión:** 1.0
**Estado:** Propuesto
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Despliegue

## 1. Contexto

El acceso al SAI es por NUT (ADR-01), pero queda abierto dónde corre NUT: dentro del contenedor del servicio o en el host con el servicio como cliente TCP de `upsd`. En ambos casos hay que resolver la competencia por el nodo USB (O-U1: NUT en el host compite con el contenedor; O-U2: competencia con otro contenedor). Es una decisión de Sprint 0 a cerrar antes de codificar la infraestructura. Motivan la decisión los riesgos R-05 (competencia por el nodo USB) y R-08 (decisión abierta), y el caso límite CL-28. Es una decisión abierta: este ADR documenta el problema y las opciones, no cierra una elección.

## 2. Decisión

PENDIENTE (P-03, Sprint 0). Se documentan las dos opciones y qué falta para decidir. No se adopta ninguna todavía. La decisión debe garantizar que un solo consumidor posea el nodo USB anclado por ruta física de puerto (ADR-03).

## 3. Estado

Propuesto el 2026-07-20. Decisión abierta de Sprint 0 (§17 P.11; pendiente P-03). Se convertirá en una ADR aceptada nueva cuando se resuelva, y esta pasará a `Superado por ADR-YY`.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| NUT dentro del contenedor | Un único artefacto desplegable, más limpio; todo el stack en una imagen | El contenedor necesita el dispositivo USB mapeado; hay que evitar que el host u otro contenedor tomen el nodo |
| NUT en el host, servicio como cliente TCP de `upsd` | El contenedor no necesita el dispositivo; separa la responsabilidad de driver del servicio | Requiere credenciales de `upsd` como secreto en runtime (P.5); dos piezas a operar; competencia con NUT del host (O-U1) |
| Ambas conviviendo sin coordinación | — | Descartada: dos procesos disputan el nodo USB; O-U1/O-U2 se materializan |

## 5. Consecuencias positivas

1. (Esperadas al cerrar) Un único poseedor del nodo USB, sin disputa (R-05).
2. Independientemente de la opción, el anclaje por ruta física de puerto (ADR-03) se mantiene.
3. Cerrar la decisión desbloquea la codificación de la infraestructura de acceso al SAI.

## 6. Consecuencias negativas y trade-offs

1. La opción «en el host» introduce credenciales de `upsd` a gestionar como secreto en runtime (P.5).
2. La opción «en el contenedor» acopla el driver a la imagen y exige el device mapping en el despliegue.
3. Mientras la decisión siga abierta, la vista de despliegue de 09 no puede fijarse.

## 7. Implementación

Qué falta para decidir: (a) determinar si algún otro contenedor o el propio host necesitan NUT en paralelo (O-U2); (b) evaluar el costo operativo de gestionar credenciales de `upsd` (opción host) frente al device mapping (opción contenedor); (c) validar la competencia por el nodo USB con la regla `udev` de ADR-03 en cada opción. La decisión se cierra en Sprint 0 y se materializa en la vista de despliegue de 09.

## 8. Métricas de validación

- Un único proceso posee el nodo USB; cero disputas del bus tras el despliegue (R-05).
- Al cerrar: el servicio lee estado y ordena apagado por el camino elegido, validado por efecto observado (ADR-11).

## 9. Referencias

- Intake §17 P.11 (Sprint 0), P.1, P.5; CL-28; riesgos R-05, R-08; pendiente P-03.
- CU-04 Monitoreo en vivo; CU-05 Ejecución del apagado ordenado ante corte.
- ADR relacionadas: ADR-01, ADR-03, ADR-11.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Decisión abierta de Sprint 0 (P-03). |
