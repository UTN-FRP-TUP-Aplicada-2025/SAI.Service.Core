# Decisiones de arquitectura — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Decisiones-Arquitectura-v1.0.md
**Versión:** 1.3
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)

## Objetivo

Índice navegable de los Architecture Decision Records (ADR) de la categoría 05 del proyecto Sai-Service-Core (`web-monolith`, caso degenerado de un único proyecto). Este documento no contiene el cuerpo de las decisiones: cada ADR vive como archivo individual e inmutable bajo `Adrs/`. Las decisiones ADR-01 a ADR-15 derivan de los pre-ADR PA-01 a PA-15 del intake §17 P.11; ADR-16 a ADR-18 cubren los ADR obligatorios de web-monolith no cubiertos por un PA (autenticación, manejo de errores de la API, motor de persistencia); ADR-19 a ADR-22 documentaron las cuatro decisiones abiertas de Sprint 0 (en el Sprint 0, ADR-19/ADR-20/ADR-22 quedaron superadas por ADR-25/ADR-26/ADR-27, y solo ADR-21 sigue Propuesto, diferida a la Etapa 5); ADR-23 y ADR-24 (Aceptado) registran, en la Fase F, la omisión de la categoría 10-Developer-Guide dedicada y el modelo de ambientes DEV/PROD sin staging.

## Índice de ADR

| ADR | Título | Categoría | Estado | Fecha |
| --- | --- | --- | --- | --- |
| [ADR-01](Adrs/ADR-01-Adopcion-De-Nut-Como-Acceso-Al-Sai-v1.0.md) | Adopción de NUT como mecanismo de acceso al SAI | Comunicación | Aceptado | 2026-07-20 |
| [ADR-02](Adrs/ADR-02-Adaptador-De-Conexion-Con-Tres-Implementaciones-v1.0.md) | Adaptador de conexión con tres implementaciones | Extensibilidad | Aceptado | 2026-07-20 |
| [ADR-03](Adrs/ADR-03-Anclaje-Del-Usb-Por-Ruta-Fisica-De-Puerto-v1.0.md) | Anclaje del USB por ruta física de puerto | Despliegue | Aceptado | 2026-07-20 |
| [ADR-04](Adrs/ADR-04-Historia-Append-Only-Sin-Event-Store-v1.0.md) | Historia append-only sin event store ni CQRS | Persistencia | Aceptado | 2026-07-20 |
| [ADR-05](Adrs/ADR-05-Vigencia-Como-Entidad-Con-Intervalo-v1.0.md) | Vigencia como entidad con intervalo, no como atributo | Persistencia | Aceptado | 2026-07-20 |
| [ADR-06](Adrs/ADR-06-Procedencia-Obligatoria-En-Todo-Valor-v1.0.md) | Procedencia obligatoria en todo valor almacenado | Persistencia | Aceptado | 2026-07-20 |
| [ADR-07](Adrs/ADR-07-Separacion-De-Catalogo-Inventario-E-Historia-v1.0.md) | Separación de catálogo, inventario e historia | Persistencia | Aceptado | 2026-07-20 |
| [ADR-08](Adrs/ADR-08-Agregado-No-Hereda-De-Muestra-v1.0.md) | Agregado no hereda de Muestra | Persistencia | Aceptado | 2026-07-20 |
| [ADR-09](Adrs/ADR-09-Modalidad-Ciclo-Forzado-De-Apagado-v1.0.md) | Modalidad CicloForzado: no cancelar el corte del SAI | Seguridad | Aceptado | 2026-07-20 |
| [ADR-10](Adrs/ADR-10-Arranque-Seguro-Y-Bloqueo-Por-Verificacion-v1.0.md) | Arranque seguro en SoloAlerta y bloqueo por verificación | Seguridad | Aceptado | 2026-07-20 |
| [ADR-11](Adrs/ADR-11-Validacion-Por-Efecto-Observado-v1.0.md) | Validación por efecto observado, no por ausencia de error | Observabilidad | Aceptado | 2026-07-20 |
| [ADR-12](Adrs/ADR-12-Disparo-Sin-Dependencia-Del-Flag-Lb-v1.0.md) | Disparo del apagado sin depender del flag LB | Comunicación | Aceptado | 2026-07-20 |
| [ADR-13](Adrs/ADR-13-Metodo-De-Salud-Por-Caida-De-Tension-v1.0.md) | Método de salud por tendencia de la caída de tensión | Observabilidad | Aceptado | 2026-07-20 |
| [ADR-14](Adrs/ADR-14-Verificacion-De-Bios-Por-Comportamiento-v1.0.md) | Verificación del ajuste de BIOS por comportamiento | Seguridad | Aceptado | 2026-07-20 |
| [ADR-15](Adrs/ADR-15-Clean-Architecture-En-Cinco-Assemblies-v1.0.md) | Clean Architecture en cinco assemblies hacia el dominio | Estilo | Aceptado | 2026-07-20 |
| [ADR-16](Adrs/ADR-16-Autenticacion-De-Administrador-Unico-v1.0.md) | Autenticación de administrador único | Seguridad | Aceptado | 2026-07-20 |
| [ADR-17](Adrs/ADR-17-Manejo-De-Errores-De-La-Api-De-Ingesta-v1.0.md) | Manejo de errores de la API de ingesta | Comunicación | Aceptado | 2026-07-20 |
| [ADR-18](Adrs/ADR-18-Motor-Sqlite-Con-Ef-Core-Y-Migraciones-v1.0.md) | Motor SQLite con EF Core y migraciones versionadas | Persistencia | Aceptado | 2026-07-20 |
| [ADR-19](Adrs/ADR-19-Ubicacion-De-Nut-Contenedor-O-Host-v1.0.md) | Ubicación de NUT: dentro del contenedor o en el host | Despliegue | Superado por ADR-25 | 2026-07-20 |
| [ADR-20](Adrs/ADR-20-Tls-Del-Panel-Y-La-Api-En-La-Lan-v1.0.md) | TLS del panel y de la API en la LAN | Seguridad | Superado por ADR-26 | 2026-07-20 |
| [ADR-21](Adrs/ADR-21-Contrato-Del-Endpoint-De-Rectificacion-v1.0.md) | Contrato del endpoint de rectificación del 409 | Comunicación | Propuesto | 2026-07-20 |
| [ADR-22](Adrs/ADR-22-Contrato-Del-Adaptador-De-Conexion-v1.0.md) | Forma del contrato del adaptador de conexión | Extensibilidad | Superado por ADR-27 | 2026-07-20 |
| [ADR-23](Adrs/ADR-23-Omision-De-Developer-Guide-Dedicada-v1.0.md) | Omisión de la categoría Developer Guide dedicada | Estilo | Aceptado | 2026-07-21 |
| [ADR-24](Adrs/ADR-24-Modelo-De-Ambientes-Dev-Prod-Sin-Staging-v1.0.md) | Modelo de ambientes DEV/PROD sin staging | Despliegue | Aceptado | 2026-07-21 |
| [ADR-25](Adrs/ADR-25-Nut-En-El-Contenedor-v1.0.md) | NUT en el contenedor | Despliegue | Aceptado | 2026-07-21 |
| [ADR-26](Adrs/ADR-26-Tls-Autofirmado-En-Kestrel-v1.0.md) | TLS con certificado autofirmado en Kestrel | Seguridad | Aceptado | 2026-07-21 |
| [ADR-27](Adrs/ADR-27-Contrato-Del-Puerto-Del-Adaptador-De-Conexion-v1.0.md) | Contrato del puerto del adaptador de conexión | Extensibilidad | Aceptado | 2026-07-21 |

## Trazabilidad de origen

| ADR | Origen (PA / obligatorio D8 / Sprint 0) | Motivación upstream principal |
| --- | --- | --- |
| ADR-01 | PA-01 | CU-04, CU-05, F-02 |
| ADR-02 | PA-02 | CU-05, F-24 |
| ADR-03 | PA-03 | CU-04, CU-09, R-05, CL-27 |
| ADR-04 | PA-04 | RC-06, NB-03, NB-04 |
| ADR-05 | PA-05 | RC-02, RC-03, RC-07, CU-08, CU-09 |
| ADR-06 | PA-06 | RC-01, RN-05, RN-06, R-13, NB-03, NB-06 |
| ADR-07 | PA-07 | RN-12, CU-08, CU-12, NB-04 |
| ADR-08 | PA-08 | RC-04, RN-10, CU-06 |
| ADR-09 | PA-09 | CU-05, NB-01, CL-01 |
| ADR-10 | PA-10 | RN-01, RN-02, NB-05, CU-10, R-12 |
| ADR-11 | PA-11 | RN-03, CU-04, CU-05, CL-07, CL-14 |
| ADR-12 | PA-12 | CU-05, NB-01, CL-06, R-04 |
| ADR-13 | PA-13 | RN-06, CU-07, NB-06, R-14 |
| ADR-14 | PA-14 | RN-02, CU-10, NB-01, CL-03 |
| ADR-15 | PA-15 | Estilo y separación de capas (obligatorios D8); R-10 |
| ADR-16 | Obligatorio web-monolith (autenticación) | CU-01, F-15 |
| ADR-17 | Obligatorio web-monolith (manejo de errores) | CU-11, RN-07, RN-08, RN-09, F-20 |
| ADR-18 | Obligatorio web-monolith (persistencia, motor) | CU-06, CU-12, F-19 |
| ADR-19 | Sprint 0 (P-03) | R-05, R-08, CL-28, CU-05 |
| ADR-20 | Sprint 0 (P-04) | CU-01, §17 P.5 |
| ADR-21 | Sprint 0 (P-05) | RN-09, CU-11, CL-21 |
| ADR-22 | Sprint 0 (P-06) | CU-05, F-24, E-07 |
| ADR-23 | Fase F (omisión de categoría 10, gating por tipo D8 y `tiene_portal_developers`) | Master-prompt §14; 10-Rules §2.2 |
| ADR-24 | Fase F (desviación del piso de ambientes de 09-Rules §2.2) | Intake §17 P.8; 09-Rules §2.2 |
| ADR-25 | Sprint 0 — cierre de ADR-19 (P-03) | R-05, R-08, CL-28, ADR-03 |
| ADR-26 | Sprint 0 — cierre de ADR-20 (P-04) | CU-01, §17 P.5, ADR-16 |
| ADR-27 | Sprint 0 — cierre de ADR-22 (P-06) | ADR-02, ADR-11, RN-03 |

## Notas de versionado e inmutabilidad

- Cada ADR es un archivo individual e inmutable (05-Rules §3.3). Una decisión aceptada no se edita; si evoluciona, se crea una ADR nueva con identificador siguiente y la anterior pasa a estado `Superado por ADR-YY`, sin moverse de `Adrs/`.
- En el Sprint 0 se cerraron tres de las cuatro decisiones abiertas: ADR-19 → ADR-25, ADR-20 → ADR-26 y ADR-22 → ADR-27 (las originales pasan a `Superado por ADR-YY`). Queda una sola en estado Propuesto: ADR-21 (contrato del endpoint de rectificación del 409), diferida a la Etapa 5 por depender del endpoint de ingesta.

## Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Índice inicial de 22 ADR (18 Aceptado, 4 Propuesto). |
| 1.1 | 2026-07-21 | Se incorpora ADR-23 (Aceptado): omisión de la categoría 10-Developer-Guide dedicada, registrada en la Fase F. Total: 23 ADR (19 Aceptado, 4 Propuesto). |
| 1.2 | 2026-07-21 | Se incorpora ADR-24 (Aceptado): modelo de ambientes DEV/PROD sin staging, desviación del piso de 09-Rules §2.2 registrada en la Fase F. Total: 24 ADR (20 Aceptado, 4 Propuesto). |
| 1.3 | 2026-07-21 | Sprint 0: se cierran tres decisiones abiertas con ADR-25 (NUT en el contenedor), ADR-26 (TLS autofirmado en Kestrel) y ADR-27 (contrato del puerto del adaptador); ADR-19/20/22 pasan a Superado. Total: 27 ADR (23 Aceptado, 1 Propuesto —ADR-21—, 3 Superado). |
