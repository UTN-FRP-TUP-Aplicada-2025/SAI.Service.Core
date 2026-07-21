# 01 — Necesidades de Negocio · Sai-Service-Core

Índice navegable de las necesidades de negocio (NB) del proyecto Sai-Service-Core. El punto de entrada formal es el índice maestro [Necesidades-Negocio-v1.0.md](Necesidades-Negocio-v1.0.md); este README agrega la vista de navegación, el mapa de dependencias, el orden de lectura y el RACI.

Son 8 NB, por lo que esta sección incluye README de acuerdo con §3.4 de las reglas constructivas.

## Tabla de NB

| NB | Título | Impacto | Prioridad MoSCoW | Estado | Enlace |
| --- | --- | --- | --- | --- | --- |
| NB-01 | Apagado ordenado y reencendido automático garantizado del host | Continuidad del servicio y camino crítico irreversible | Must Have | Borrador | [NB-01](Necesidades-De-Negocio/NB-01-Apagado-Ordenado-Y-Reencendido-Garantizado-v1.0.md) |
| NB-02 | Monitoreo en vivo del estado del SAI y alertas de conectividad | Visibilidad operativa e insumo del apagado | Must Have | Borrador | [NB-02](Necesidades-De-Negocio/NB-02-Monitoreo-En-Vivo-Y-Alertas-v1.0.md) |
| NB-03 | Historia trazable de métricas con procedencia y corrección retroactiva | Confiabilidad de las conclusiones y auditabilidad | Must Have | Borrador | [NB-03](Necesidades-De-Negocio/NB-03-Historia-Trazable-Con-Procedencia-v1.0.md) |
| NB-04 | Gestión del ciclo de vida de los equipos y baterías | Trazabilidad de los equipos y decisión de compra con datos | Must Have | Borrador | [NB-04](Necesidades-De-Negocio/NB-04-Ciclo-De-Vida-De-Los-Equipos-v1.0.md) |
| NB-05 | Seguridad operativa: arranque seguro, bloqueo por verificación y validación por efecto observado | Garantía de no apagar sin poder probar el reencendido | Must Have | Borrador | [NB-05](Necesidades-De-Negocio/NB-05-Seguridad-Operativa-Bloqueo-Por-Verificacion-v1.0.md) |
| NB-06 | Evaluación de la salud de las baterías por tendencia, con confianza y reservas | Planificación del recambio y diferenciador de valor | Must Have | Borrador | [NB-06](Necesidades-De-Negocio/NB-06-Evaluacion-De-Salud-De-Baterias-v1.0.md) |
| NB-07 | Configuración de políticas de apagado versionadas | Control y explicabilidad del camino crítico | Must Have | Borrador | [NB-07](Necesidades-De-Negocio/NB-07-Configuracion-De-Politicas-De-Apagado-v1.0.md) |
| NB-08 | Ingesta automatizada de intervenciones desde un sistema externo | Alimentación automática del histórico con integridad ante reintentos | Must Have | Borrador | [NB-08](Necesidades-De-Negocio/NB-08-Ingesta-Automatizada-De-Intervenciones-v1.0.md) |

## Mapa de dependencias

| NB | Depende de | Es prerequisito de |
| --- | --- | --- |
| NB-01 | NB-02, NB-05, NB-07 | (ninguna) |
| NB-02 | NB-03, NB-04 | NB-01, NB-05, NB-07 |
| NB-03 | NB-04 | NB-02, NB-06, NB-08 |
| NB-04 | (ninguna) | NB-02, NB-03, NB-06, NB-08 |
| NB-05 | NB-02, NB-07 | NB-01 |
| NB-06 | NB-03, NB-04 | (ninguna) |
| NB-07 | NB-02 | NB-01, NB-05 |
| NB-08 | NB-03, NB-04 | (ninguna) |

El grafo es acíclico y ninguna NB depende de más de 3 otras. NB-04 es la raíz fundacional (sin dependencias) y NB-01 es el sumidero del camino crítico.

## Orden de lectura sugerido

1. NB-04 — Ciclo de vida de los equipos (fundacional: unidades y vínculos temporales).
2. NB-03 — Historia trazable con procedencia (se apoya en el inventario).
3. NB-02 — Monitoreo en vivo y alertas (persiste sobre la historia).
4. NB-07 — Configuración de políticas de apagado (evalúa el estado en vivo).
5. NB-05 — Seguridad operativa (gobierna el bloqueo de las políticas).
6. NB-01 — Apagado ordenado y reencendido (el valor central; consume las anteriores).
7. NB-06 — Evaluación de salud de baterías (Must Have; se apoya en historia e inventario).
8. NB-08 — Ingesta automatizada de intervenciones (Must Have; se apoya en historia e inventario).

## RACI

R = Responsable de la redacción y construcción · A = Aprobador · C = Consultado · I = Informado. Al ser un proyecto de un único responsable que concentra propietario, implementador y beneficiario, el RACI se expresa por rol funcional, no por personas distintas.

| NB | Responsable (implementador) | Aprobador (propietario) | Revisor / Consultado |
| --- | --- | --- | --- |
| NB-01 | Administrador único (rol implementador) | Administrador único (rol propietario) | AG-01 (autoría); AG-02 (trazabilidad a CU) |
| NB-02 | Administrador único (rol implementador) | Administrador único (rol propietario) | AG-01 (autoría); AG-02 (trazabilidad a CU) |
| NB-03 | Administrador único (rol implementador) | Administrador único (rol propietario) | AG-01 (autoría); AG-02 (trazabilidad a CU) |
| NB-04 | Administrador único (rol implementador) | Administrador único (rol propietario) | AG-01 (autoría); Proveedor / técnico externo (consultado) |
| NB-05 | Administrador único (rol implementador) | Administrador único (rol propietario) | AG-01 (autoría); AG-02 (trazabilidad a CU) |
| NB-06 | Administrador único (rol implementador) | Administrador único (rol propietario) | AG-01 (autoría); AG-02 (trazabilidad a CU) |
| NB-07 | Administrador único (rol implementador) | Administrador único (rol propietario) | AG-01 (autoría); AG-02 (trazabilidad a CU) |
| NB-08 | Administrador único (rol implementador) | Administrador único (rol propietario) | AG-01 (autoría); Sistema externo de gestión de mantenimiento (informado) |

## Nota de trazabilidad

Cada NB declara su trazabilidad upstream al SOLUTION-INTAKE y a los documentos de la categoría 00 (Vision-Producto-v1.0.md, Alcance-Proyecto-v1.0.md), y su trazabilidad downstream a los CU que la cubren en 02-Especificacion-Funcional, hoy vigentes y en estado aprobada (los 12 CU alineados con los flujos UF-1 a UF-10 más la autenticación CU-01). El detalle NB por NB está en la §7 de cada NB y en la tabla resumen del índice maestro. Los targets tomados del SOLUTION-INTAKE §8 son propuestos y requieren ratificación del administrador (pendiente P-01 del intake).

La autenticación del administrador único (F-15, Must Have) se materializa en CU-01 y no tiene NB propia por decisión de alcance (un único administrador, sin gestión de usuarios ni roles); queda anclada como habilitador transversal a NB-05 (Seguridad operativa), documentada en su §1 y en la §2 del índice maestro.
