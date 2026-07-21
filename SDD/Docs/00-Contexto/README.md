# 00-Contexto — SAI.Service.Core

Categoría de contexto del producto para la solución SAI.Service.Core (proyecto único `Sai-Service-Core`, tipo `web-monolith`). Estos documentos definen el porqué del sistema, su alcance, su hoja de ruta y sus plataformas soportadas. Son el inicio de la cadena de trazabilidad: no tienen documentos upstream más allá del SOLUTION-INTAKE, y alimentan a todas las categorías siguientes.

La solución es de un único proyecto (caso degenerado), por lo que el layout está aplanado: los documentos cuelgan directamente de `SDD/Docs/00-Contexto/`, sin subnivel `Proyectos/<Nombre>/`.

## Documentos de la categoría

| Orden | Documento | Propósito | Estado |
|---|---|---|---|
| 1 | `Vision-Producto-v1.0.md` | Por qué existe el sistema, audiencia, propuesta de valor, visión a 3 años, objetivos SMART, métricas de éxito, restricciones, riesgos y glosario del dominio | Borrador |
| 2 | `Alcance-Proyecto-v1.0.md` | Qué entra y qué queda afuera: capacidades incluidas, exclusiones justificadas, supuestos, restricciones, criterios de aceptación y gestión de cambios de alcance | Borrador |
| 3 | `Roadmap-Producto-v1.0.md` | Fases del producto con objetivo, épicas, entregable y criterios de transición verificables; deriva del esquema de delivery por etapas del intake | Borrador |
| 4 | `Compatibilidad-Plataformas-v1.0.md` | Sistema operativo, runtime, contenedores, navegadores y acceso al equipo soportados, con versiones mínimas y cláusula de no-soporte | Borrador |

Orden de lectura sugerido: visión, alcance, roadmap y compatibilidad. La visión da el marco; el alcance lo acota; el roadmap ordena la construcción; la compatibilidad fija el entorno de ejecución.

## Documento omitido

`Acuerdo-Equipo-v1.0.md` se omite por aplicación de la regla §2.2 de `00-Rules-Contexto.md`: se genera solo para equipos de más de dos personas y se omite en proyectos de un único desarrollador. Este es un proyecto interno de una sola persona que concentra los roles de propietario, implementador y beneficiario, por lo que no hay convenciones de equipo, ceremonias ni coordinación entre personas que documentar. Las convenciones técnicas de trabajo (control de versiones, estrategia de ramas, commits) se documentan en las categorías técnicas correspondientes (05 y 09), no aquí.

## Stakeholders del proyecto

| Rol | Categoría | Responsabilidad principal |
|---|---|---|
| Administrador único | Propietario, implementador y beneficiario (una sola persona) | Aprueba el alcance, da de alta el parque, configura políticas, monitorea, consulta históricos, dispara pruebas, carga intervenciones, ejecuta la ventana de mantenimiento con presencia física y emite informes |
| Servidor protegido | Beneficiario (sistema) | Es el objeto de la protección: el apagado ordenado y el reencendido automático operan sobre él |
| Proveedor / técnico externo | Beneficiario indirecto / ejecutor externo | Ejecuta recambios de batería, reparaciones e inspecciones; recibe las baterías retiradas para trazabilidad ambiental |
| Sistema externo de gestión de mantenimiento | Integrador / consumidor de la interfaz de integración | Empuja intervenciones de forma automatizada, con confianza declarada menor que la del dato medido localmente |

Los tres roles de propietario, implementador y beneficiario recaen en una única persona; los demás stakeholders participan de forma puntual o son sistemas.

## Trazabilidad

- Upstream: `SDD/Intake/SOLUTION-INTAKE-Sai-Service-Core-v1.0.md` y `SDD/Intake/SOLUTION-MANIFEST-Sai-Service-Core-v1.0.md`.
- Downstream: 01-Necesidades-Negocio, 02-Especificacion-Funcional, 03-UX-UI-DX, 05-Arquitectura-Tecnica, 06-Backlog-Tecnico, 07-Plan-Sprint, 09-Devops y 11-Examples.

## Decisiones abiertas heredadas del intake

Estos documentos tratan como decisiones abiertas los pendientes del intake, sin cerrarlos ni inventarlos: ratificación de los targets de las métricas (P-01), normativa local de disposición de baterías (P-02), ubicación de la herramienta de acceso al equipo (P-03), cifrado del acceso en la red local (P-04), contrato del endpoint de rectificación (P-05) y firma del contrato del adaptador de conexión (P-06). Su cierre corresponde a las categorías 02, 05, 08 y 09.
