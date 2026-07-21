# ADR-24 — Modelo de ambientes DEV/PROD sin staging

**Proyecto:** Sai-Service-Core
**Documento:** ADR-24-Modelo-De-Ambientes-Dev-Prod-Sin-Staging-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Despliegue

## 1. Contexto

Las reglas de la categoría 09 (`09-Rules-Devops.md` §2.2) fijan para `web-monolith` un piso de ambientes DEV / QA / STAGING / PROD, y advierten que quitar cualquiera del piso requiere un ADR que lo justifique. Este proyecto opera con solo dos ambientes: DEV (Dev Container en la máquina del desarrollador) y PROD (contenedor en el host `i7infra`). La razón es del dominio: el sistema administra un SAI físico concreto conectado por USB al host protegido, y su comportamiento crítico —el apagado ordenado y el reencendido— solo se puede validar de extremo a extremo contra ese equipo. El intake lo declara textualmente en §17 P.8: «No hay ambiente de *staging*: no habría a qué SAI conectarlo». Un ambiente intermedio sin el equipo real validaría únicamente la lógica ya cubierta por el adaptador simulado en las pruebas de 08, sin agregar garantía sobre el camino físico.

## 2. Decisión

Se adopta un modelo de dos ambientes: DEV (Dev Container, depuración por F5) y PROD (contenedor en `i7infra`, con el dispositivo USB anclado por ruta física de puerto según ADR-03). Se omiten QA y STAGING del piso de `09-Rules` §2.2. La validación de extremo a extremo del camino crítico se realiza en PROD durante la ventana de mantenimiento (UF-8), y la lógica de decisión se cubre antes con el adaptador simulado (08).

## 3. Estado

Aceptado — 2026-07-21.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
|---|---|---|
| Piso completo DEV/QA/STAGING/PROD | Cumple el piso de la regla sin desviación | QA y STAGING no tendrían un SAI al que conectarse; validarían solo lo que el adaptador simulado ya cubre; ambientes ceremoniales sin garantía adicional |
| DEV + PROD (elegida) | Refleja la realidad física: la validación end-to-end solo es posible contra el equipo real; menos superficie operativa para un solo desarrollador | Se aparta del piso de la regla; exige este ADR; la validación del camino físico se concentra en la ventana de mantenimiento en PROD |
| DEV + STAGING con SAI de laboratorio | Un entorno de prueba con hardware real | No hay un segundo SAI ni presupuesto para adquirirlo (intake §10); alcance físico declarado: un solo SAI activo |

## 5. Consecuencias positivas

1. El modelo de ambientes refleja la restricción física real en vez de simularla con ambientes vacíos.
2. Menor superficie operativa para un proyecto de un solo desarrollador.
3. La validación del camino crítico queda explícitamente asignada a la ventana de mantenimiento en PROD (UF-8), no diluida en un staging sin equipo.

## 6. Consecuencias negativas y trade-offs

1. No hay un ambiente intermedio donde ensayar un despliegue antes de PROD; se mitiga con el smoke test del pipeline (stage 8) y con el rollback por tag de imagen.
2. Si en el futuro se dispone de un segundo SAI de laboratorio, convendrá reintroducir un STAGING; el trade-off de no anticiparlo está aceptado.
3. La desviación del piso queda formalmente aceptada acá; sin este ADR, el pipeline y los entornos estarían en incumplimiento de `09-Rules` §2.2.

## 7. Implementación

`Entornos-Deploy-v1.0.md` documenta los dos ambientes con su configuración 12-factor, secretos por variables de entorno y la promoción DEV→PROD. El pipeline (`Pipeline-CI-CD-v1.0.md`) tiene una única transición de promoción (DEV→PROD) con su aprobador. La reintroducción de un STAGING futuro se haría con una ADR nueva que supere a esta.

## 8. Métricas de validación

- `Entornos-Deploy-v1.0.md` declara exactamente dos ambientes (DEV, PROD).
- El pipeline no tiene stages ni gates que asuman QA o STAGING.
- El camino físico de apagado/reencendido queda cubierto por la evidencia de UF-8 en PROD (referida en 08 como prueba no automatizable, F-3).

## 9. Referencias

- Intake §17 P.8 (pipeline y ambientes; justificación textual del no-staging) y §10 (sin presupuesto, un solo SAI activo).
- `09-Rules-Devops.md` §2.2 (piso de ambientes web-monolith y regla de desviación por ADR).
- ADR-03 (anclaje del USB por ruta física en PROD); ADR-19 (ubicación de NUT, Sprint 0, condiciona el `docker run` de PROD).
- `Entornos-Deploy-v1.0.md`, `Pipeline-CI-CD-v1.0.md`.

## 10. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Registro del modelo de dos ambientes (DEV/PROD) y de la omisión justificada de QA y STAGING del piso de 09-Rules §2.2, con respaldo en el intake §17 P.8. |
