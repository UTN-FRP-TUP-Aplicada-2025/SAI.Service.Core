# 05 — Arquitectura técnica · Sai-Service-Core

**Proyecto:** Sai-Service-Core (`web-monolith`, caso degenerado de un único proyecto)
**Autor:** Orquestador SDD (AG-05)
**Fecha:** 2026-07-20

Índice navegable de la arquitectura técnica: documento maestro, decisiones, modelo de datos lógico, flujo del camino crítico, contrato REST, extensibilidad y NFR. Punto de entrada para los revisores de 06 a 11 (AG-02, AG-06, AG-08, AG-09).

## Documento maestro

| Documento | Contenido |
| --- | --- |
| [Arquitectura-Solucion-v1.0.md](Arquitectura-Solucion-v1.0.md) | Las 10 secciones del §4.2: objetivo, estilo (Clean Architecture 5 assemblies, justificado vs capas tradicionales y event-driven), vistas lógica / procesos / despliegue / datos, cross-cutting, 25 NFR con objetivo numérico, 14 riesgos y trazabilidad CU/RN/ADR |

## Artefactos de la sección

| Documento | Contenido |
| --- | --- |
| [Flujo-Ejecucion-v1.0.md](Flujo-Ejecucion-v1.0.md) | Pipeline del planificador (ronda de sondeo) y del apagado ordenado (temporizador con cancelación, decisión con degradación, ejecución por efecto observado). CU-05, CU-10; ADR-09/10/11/12 |
| [Contratos-REST-v1.0.md](Contratos-REST-v1.0.md) | API de ingesta `POST /api/v1/intervenciones`: OpenAPI inline, problem+json RFC 7807, cuatro caminos 201/200/409/422, versionado. CU-11; ADR-17, ADR-21 [Propuesto] |
| [Extensibilidad-v1.0.md](Extensibilidad-v1.0.md) | Puerto del adaptador de conexión: contrato mínimo de 4 operaciones, tres implementaciones (NUT, directo+add-on, simulada), add-ons de dialecto diseñados. ADR-02, ADR-22 [Propuesto] |
| `Modelo-Datos-Logico-v1.0.md` | Modelo lógico con tipos físicos, índices, restricciones y migración inicial; referencia al `Modelo-Conceptual-v1.0.md` de 02. ADR-04/05/06/07/08/18 *(artefacto hermano de la sección)* |

## Decisiones de arquitectura (ADR)

Índice completo con estado y fecha: [Decisiones-Arquitectura-v1.0.md](Decisiones-Arquitectura-v1.0.md). Cada ADR vive como archivo individual e inmutable en [`Adrs/`](Adrs/). Total: **22 ADR (18 Aceptado, 4 Propuesto)**.

| ADR | Título | Categoría | Estado |
| --- | --- | --- | --- |
| [ADR-01](Adrs/ADR-01-Adopcion-De-Nut-Como-Acceso-Al-Sai-v1.0.md) | Adopción de NUT como acceso al SAI | Comunicación | Aceptado |
| [ADR-02](Adrs/ADR-02-Adaptador-De-Conexion-Con-Tres-Implementaciones-v1.0.md) | Adaptador de conexión con tres implementaciones | Extensibilidad | Aceptado |
| [ADR-03](Adrs/ADR-03-Anclaje-Del-Usb-Por-Ruta-Fisica-De-Puerto-v1.0.md) | Anclaje del USB por ruta física de puerto | Despliegue | Aceptado |
| [ADR-04](Adrs/ADR-04-Historia-Append-Only-Sin-Event-Store-v1.0.md) | Historia append-only sin event store ni CQRS | Persistencia | Aceptado |
| [ADR-05](Adrs/ADR-05-Vigencia-Como-Entidad-Con-Intervalo-v1.0.md) | Vigencia como entidad con intervalo | Persistencia | Aceptado |
| [ADR-06](Adrs/ADR-06-Procedencia-Obligatoria-En-Todo-Valor-v1.0.md) | Procedencia obligatoria en todo valor | Persistencia | Aceptado |
| [ADR-07](Adrs/ADR-07-Separacion-De-Catalogo-Inventario-E-Historia-v1.0.md) | Separación de catálogo, inventario e historia | Persistencia | Aceptado |
| [ADR-08](Adrs/ADR-08-Agregado-No-Hereda-De-Muestra-v1.0.md) | Agregado no hereda de Muestra | Persistencia | Aceptado |
| [ADR-09](Adrs/ADR-09-Modalidad-Ciclo-Forzado-De-Apagado-v1.0.md) | Modalidad CicloForzado | Seguridad | Aceptado |
| [ADR-10](Adrs/ADR-10-Arranque-Seguro-Y-Bloqueo-Por-Verificacion-v1.0.md) | Arranque seguro y bloqueo por verificación | Seguridad | Aceptado |
| [ADR-11](Adrs/ADR-11-Validacion-Por-Efecto-Observado-v1.0.md) | Validación por efecto observado | Observabilidad | Aceptado |
| [ADR-12](Adrs/ADR-12-Disparo-Sin-Dependencia-Del-Flag-Lb-v1.0.md) | Disparo sin depender del flag LB | Comunicación | Aceptado |
| [ADR-13](Adrs/ADR-13-Metodo-De-Salud-Por-Caida-De-Tension-v1.0.md) | Método de salud por caída de tensión | Observabilidad | Aceptado |
| [ADR-14](Adrs/ADR-14-Verificacion-De-Bios-Por-Comportamiento-v1.0.md) | Verificación de BIOS por comportamiento | Seguridad | Aceptado |
| [ADR-15](Adrs/ADR-15-Clean-Architecture-En-Cinco-Assemblies-v1.0.md) | Clean Architecture en cinco assemblies | Estilo | Aceptado |
| [ADR-16](Adrs/ADR-16-Autenticacion-De-Administrador-Unico-v1.0.md) | Autenticación de administrador único | Seguridad | Aceptado |
| [ADR-17](Adrs/ADR-17-Manejo-De-Errores-De-La-Api-De-Ingesta-v1.0.md) | Manejo de errores de la API de ingesta | Comunicación | Aceptado |
| [ADR-18](Adrs/ADR-18-Motor-Sqlite-Con-Ef-Core-Y-Migraciones-v1.0.md) | Motor SQLite con EF Core y migraciones | Persistencia | Aceptado |
| [ADR-19](Adrs/ADR-19-Ubicacion-De-Nut-Contenedor-O-Host-v1.0.md) | Ubicación de NUT: contenedor o host | Despliegue | Propuesto |
| [ADR-20](Adrs/ADR-20-Tls-Del-Panel-Y-La-Api-En-La-Lan-v1.0.md) | TLS del panel y de la API en la LAN | Seguridad | Propuesto |
| [ADR-21](Adrs/ADR-21-Contrato-Del-Endpoint-De-Rectificacion-v1.0.md) | Contrato del endpoint de rectificación del 409 | Comunicación | Propuesto |
| [ADR-22](Adrs/ADR-22-Contrato-Del-Adaptador-De-Conexion-v1.0.md) | Forma del contrato del adaptador de conexión | Extensibilidad | Propuesto |

## NFR (atributos de calidad)

La tabla completa de 25 NFR con objetivo numérico, mecanismo de medición y ADR relacionada está en el **§8 de [Arquitectura-Solucion-v1.0.md](Arquitectura-Solucion-v1.0.md#8-quality-attributes-nfr)**. Camino crítico: retardo de corte ≤ 540 s, grace Docker 150 s, latencia de decisión < 1 s, intervalo de sondeo 5 s. Cobertura bloqueante: 80/70 global y 90/85 en `Domain`. Marcados **PENDIENTE**: resto del apagado del SO (se mide en CU-10), tamaño del archivo SQLite (R-07) y SLO de disponibilidad del servicio.

## Notas

- Estructura de repositorio: **layout aplanado** del caso degenerado (un solo proyecto); las categorías 00 a 11 cuelgan directo de `SDD/Docs/`, sin `Proyectos/<Nombre>/` ni `Solucion/`.
- No se produce vista de solución (`Solucion/`): con un único proyecto, el mapa tendría un solo nodo y el grafo ninguna arista (05-Rules §4.8).
- Terminología vigente: "equipos" (no "parque"), "contraseña" (no "secreto").
