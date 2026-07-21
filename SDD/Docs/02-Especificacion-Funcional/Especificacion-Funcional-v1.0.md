# Especificación funcional — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Especificacion-Funcional-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito y alcance

Índice maestro de la especificación funcional del proyecto Sai-Service-Core, un servicio web de tipo web-monolith que monitorea el equipo de alimentación, decide y ejecuta el apagado ordenado del host y su reencendido, administra el ciclo de vida del parque y expone un panel y una interfaz de integración, para un único administrador. Consolida los casos de uso, las reglas de negocio, el modelo conceptual y sus reglas conceptuales, y traza cada artefacto a las necesidades de negocio del catálogo 01. Define el qué del sistema, sin invadir el cómo.

Insumos: SOLUTION-INTAKE de la solución (§1 a §12 de negocio, §17 P.4 modelo y P.10 invariantes, §20 escenarios E-1 a E-8, §21 invariantes I-1 a I-21); categoría 00-Contexto; catálogo de 8 necesidades de negocio NB-01 a NB-08 de 01-Necesidades-Negocio.

## 2. Matriz NB → CU → RN → US

| NB | Necesidad | CU que la cubren | RN aplicables | US a generar en 06 |
| --- | --- | --- | --- | --- |
| NB-01 | Apagado ordenado y reencendido garantizado | CU-05, CU-10 | RN-01, RN-02, RN-03, RN-04, RN-11 | US-02, US-04, US-10 |
| NB-02 | Monitoreo en vivo y alertas | CU-04 (y CU-02 en el alta) | RN-05, RN-06, RN-03 | US-02, US-04, US-05 |
| NB-03 | Historia trazable con procedencia | CU-06 (y CU-08 en la reatribución) | RN-05, RN-10 | US-05, US-06 |
| NB-04 | Ciclo de vida del parque | CU-02, CU-08, CU-09, CU-12 | RN-07, RN-08, RN-12, RN-13 | US-01, US-08, US-09, US-11 |
| NB-05 | Seguridad operativa y bloqueo por verificación | CU-01, CU-05, CU-10 | RN-01, RN-02, RN-03 | US-02, US-10, US de autenticación |
| NB-06 | Evaluación de salud de baterías | CU-07, CU-08, CU-12 | RN-05, RN-06, RN-07 | US-07, US-11 |
| NB-07 | Configuración de políticas de apagado | CU-03 (y CU-05 en la ejecución) | RN-04, RN-11, RN-02 | US-03 |
| NB-08 | Ingesta automatizada de intervenciones | CU-11 (y CU-08 relacionada) | RN-07, RN-08, RN-09, RN-12 | US-12 |

Cobertura bidireccional: cada una de las 8 NB tiene al menos un CU y cada uno de los 12 CU declara al menos una NB. No hay CU huérfano ni NB sin CU. La autenticación (CU-01) se traza a NB-05 por proximidad; ver ambigüedad declarada en §6.

## 3. Tabla de casos de uso

| CU | Título | Flujo de usuario | Actor primario | NB | Estado |
| --- | --- | --- | --- | --- | --- |
| CU-01 | Autenticación y gestión de la sesión del administrador | Autenticación | Administrador | NB-05 | Borrador |
| CU-02 | Alta del parque y puesta en marcha | UF-1 | Administrador | NB-04 | Borrador |
| CU-03 | Configuración de políticas de apagado versionadas | UF-2 | Administrador | NB-07 | Borrador |
| CU-04 | Monitoreo en vivo del estado del SAI | UF-3 | Administrador | NB-02 | Borrador |
| CU-05 | Ejecución del apagado ordenado ante corte sostenido | UF-3 / E-4 | Planificador interno | NB-01, NB-05 | Borrador |
| CU-06 | Históricos y gráficas de evolución del suministro | UF-4 | Administrador | NB-03 | Borrador |
| CU-07 | Prueba de batería y veredicto de salud | UF-5 | Administrador | NB-06 | Borrador |
| CU-08 | Registro de recambio de batería y ficha de vida útil | UF-6 | Administrador | NB-04, NB-06 | Borrador |
| CU-09 | Reparación y sustitución del SAI con cobertura suplente | UF-7 | Administrador | NB-04 | Borrador |
| CU-10 | Ventana de mantenimiento y verificación de supuestos | UF-8 | Administrador con presencia física | NB-01, NB-05 | Borrador |
| CU-11 | Ingesta automatizada de intervenciones | UF-10 | Sistema externo de gestión | NB-08 | Borrador |
| CU-12 | Informe de período y comparación de marcas | UF-9 | Administrador | NB-04, NB-06 | Borrador |

Los diez flujos de usuario UF-1 a UF-10 del intake quedan cubiertos: UF-1 por CU-02, UF-2 por CU-03, UF-3 por CU-04 y CU-05, UF-4 por CU-06, UF-5 por CU-07, UF-6 por CU-08, UF-7 por CU-09, UF-8 por CU-10, UF-9 por CU-12, UF-10 por CU-11; más la autenticación por CU-01.

## 4. Tabla de reglas de negocio

| RN | Título | Invariante de origen | CU afectados | Estado |
| --- | --- | --- | --- | --- |
| RN-01 | Arranque seguro en solo aviso | PA-10 | CU-02, CU-05, CU-10 | Borrador |
| RN-02 | Bloqueo por verificación y degradación de modalidad | I-11 | CU-05, CU-03, CU-10, CU-04 | Borrador |
| RN-03 | Validación por efecto observado | PA-11, CL-07 | CU-05, CU-10, CU-07 | Borrador |
| RN-04 | Techo duro del tiempo reservado de apagado | I-10 | CU-03, CU-05 | Borrador |
| RN-05 | Procedencia obligatoria y origen declarado de todo valor | I-7, I-8 | CU-04, CU-06, CU-07, CU-08, CU-12 | Borrador |
| RN-06 | Aptitud de datos para la tendencia de salud | I-9, I-16 | CU-07, CU-04, CU-08 | Borrador |
| RN-07 | Todo importe con moneda y fecha | I-18 | CU-08, CU-11, CU-12, CU-02 | Borrador |
| RN-08 | Cuadre de costos de una intervención | Costos.cuadra() / CL-22 | CU-08, CU-11 | Borrador |
| RN-09 | Idempotencia de la ingesta externa | I-19 | CU-11 | Borrador |
| RN-10 | Agregado con cobertura y advertencia obligatorias | I-20 | CU-06, CU-12 | Borrador |
| RN-11 | Acción referida a una versión de política | I-13 | CU-05, CU-03 | Borrador |
| RN-12 | Baja lógica y coherencia temporal de las intervenciones | I-5, I-6 / CL-20 | CU-08, CU-09, CU-11, CU-12 | Borrador |
| RN-13 | Vida de flotación esperada con temperatura de referencia | I-21 | CU-02 | Borrador |

## 5. Modelo conceptual y reglas conceptuales

El modelo conceptual está en `Modelo-Datos/Modelo-Conceptual-v1.0.md`, con 27 entidades y objetos de valor en cuatro capas (catálogo, inventario, vínculos temporales, historia append-only) más la proyección de ficha de vida útil, y un diagrama entidad-relación embebido. Al superar las diez entidades, se acompañan de nueve reglas conceptuales en `Modelo-Datos/reglas-conceptuales-de-modelo/`:

| RC | Título | Estado |
| --- | --- | --- |
| RC-01 | Procedencia por valor | Borrador |
| RC-02 | Vigencia como entidad con intervalo | Borrador |
| RC-03 | Sucesión de vínculos sin hueco al cerrar y abrir | Borrador |
| RC-04 | Agregado no hereda de Muestra | Borrador |
| RC-05 | Acción referida a versión de política | Borrador |
| RC-06 | Historia append-only | Borrador |
| RC-07 | Resolución temporal de la batería | Borrador |
| RC-08 | Baja lógica y coherencia temporal de la unidad física | Borrador |
| RC-09 | Evento referido a regla de derivación versionada | Borrador |

## 6. Ambigüedades y pendientes declarados

- Autenticación sin NB dedicada. La autenticación es capacidad Must Have del intake (F-15) pero el catálogo 01 no la asocia a ninguna NB. CU-01 se traza a NB-05 por proximidad. Requiere decisión del orquestador: crear una NB de control de acceso o confirmar la autenticación como habilitador transversal sin NB propia.
- Pendiente P-05: contrato del endpoint de rectificación que sugiere la respuesta de conflicto de la ingesta. Se referencia en CU-11 y queda por cerrar en esta categoría; hasta entonces la corrección de un hecho ya ingresado se marca como pendiente.
- Riesgo R-11: el flujo de sustitución del SAI (CU-09) no tiene aún un escenario de datos completo; se propone un escenario E-9 al implementarlo.
- Divergencia de granularidad con el catálogo 01. El índice de NB previó 19 CU de grano fino (CU-01 a CU-19). Esta especificación adopta 12 CU alineados uno a uno con los flujos de usuario UF-1 a UF-10 más autenticación, según la titularidad del analista funcional (regla 02 §2.2 y §3.3). Cada CU previsto del catálogo 01 queda subsumido en el CU de flujo correspondiente; la cobertura de las 8 NB se mantiene completa.

## 7. Estado de los artefactos

Todos los artefactos de esta categoría están en estado Borrador, fecha 2026-07-20, autor Orquestador SDD (AG-02): el presente índice, 12 casos de uso, 13 reglas de negocio, el modelo conceptual y 9 reglas conceptuales, más el README de la sección.

## 8. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Especificación funcional inicial: matriz NB-CU-RN-US, 12 CU, 13 RN, modelo conceptual y 9 RC, derivados del SOLUTION-INTAKE y del catálogo NB-01 a NB-08 |
