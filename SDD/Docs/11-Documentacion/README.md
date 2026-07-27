# 11-Documentacion — Cuerpo documental de entrega de SAI.Service.Core

Esta carpeta es el cuerpo documental de entrega: lo que queda cuando el sistema está construido y alguien que no participó de su especificación tiene que **usarlo, mantenerlo u operarlo**. A diferencia de las categorías 00 a 09 —escritas para sostener la cadena de especificación—, acá el lector primario es un agente humano o de IA en primer contacto, que llega sin el contexto acumulado. La categoría 10 demuestra con ejemplos ejecutables; esta explica, contextualiza y enlaza sin duplicar código.

## 1. Matriz de ruteo

Buscá tu rol y tu intención; la fila indica el documento.

| Rol | Intención | Documento |
| --- | --- | --- |
| Cualquiera | Entender qué es el sistema en 10 minutos | [Vision-General-Sistema](Vision-General-Sistema-v1.0.md) |
| Operador / Mantenedor | Levantar la solución en una máquina limpia | [Guia-Inicio-Rapido](Guia-Inicio-Rapido-v1.0.md) |
| Operador | Desplegar en el host de producción | [Guia-Despliegue](Guia-Despliegue-v1.0.md) |
| Operador | Saber qué necesita el contenedor (env, puertos, volúmenes, USB) | [Guia-Contenedor](Guia-Contenedor-v1.0.md) |
| Operador | Arrancar, parar, ver salud, leer logs, resolver incidentes | [Runbook-Operacion](Runbook-Operacion-v1.0.md) |
| Mantenedor | Ubicar cada componente de la arquitectura en el árbol de código | [Recorrido-Codigo](Recorrido-Codigo-v1.0.md) |
| Mantenedor | Preparar el entorno y agregar una funcionalidad sin romper el diseño | [Guia-Contribucion](Guia-Contribucion-v1.0.md) |
| Mantenedor | Extender el adaptador de conexión con otro backend | [Guia-Extension](Guia-Extension-v1.0.md) |
| Operador / Mantenedor | Ver eventualidades ya vividas y cómo se resolvieron | [Bitacora-Eventualidades](Bitacora-Eventualidades-v1.0.md) |
| Agente de IA | Contexto de repositorio, build, tests, límites | [Contrato-Agentes](Contrato-Agentes-v1.0.md) · `AGENTS.md` en la raíz |
| Integrador | Consumir la API de ingesta | [05/Contratos-REST](../05-Arquitectura-Tecnica/Contratos-REST-v1.0.md) (ver §4) |

## 2. Estado del cuerpo documental

| Artefacto | Cuerpo | Estado | Última revisión |
| --- | --- | --- | --- |
| Vision-General-Sistema | Solución | Vigente | 2026-07-27 |
| Guia-Inicio-Rapido | Solución | Vigente | 2026-07-27 |
| Guia-Despliegue | Solución | Vigente | 2026-07-27 |
| Bitacora-Eventualidades | Solución | Vigente | 2026-07-27 |
| Contrato-Agentes | Solución | Vigente | 2026-07-27 |
| `AGENTS.md` (raíz) | Solución | Vigente | 2026-07-27 |
| Recorrido-Codigo | Mantenedor | Vigente | 2026-07-27 |
| Guia-Contribucion | Mantenedor | Vigente | 2026-07-27 |
| Guia-Extension | Mantenedor | Vigente | 2026-07-27 |
| Guia-Contenedor | Operador | Vigente | 2026-07-27 |
| Runbook-Operacion | Operador | Vigente | 2026-07-27 |

**Cuerpo integrador: omitido por gating.** Para `web-monolith`, el cuerpo integrador es opcional (solo si expone una superficie pública). La única superficie hacia terceros es la API de ingesta, cuyo contrato completo ya vive en [Contratos-REST-v1.0.md](../05-Arquitectura-Tecnica/Contratos-REST-v1.0.md) (curado desde OpenAPI, con los cuatro caminos y problem+json). No se duplica acá; si en el futuro aparece un segundo integrador, se materializa `Referencia-Api` en este cuerpo.

## 3. Orden de lectura sugerido

- **Operador nuevo:** Vision-General → Guia-Inicio-Rapido → Guia-Contenedor → Runbook-Operacion.
- **Mantenedor nuevo:** Vision-General → Recorrido-Codigo → Guia-Contribucion → Guia-Extension.
- **Agente de IA:** `AGENTS.md` (raíz) → Contrato-Agentes → Recorrido-Codigo.

## 4. Cómo se mantiene

Esta documentación es viva: se actualiza en cada cierre de sprint junto con el código que describe, no en una pasada final. Una funcionalidad no está terminada hasta que su documentación refleja el estado real del sistema (Definition of Done del incremento, categoría 07). Cada revisión actualiza el campo `last_review` del frontmatter; un documento sin revisar por más de dos cortes se marca como *Potencialmente desactualizado* en la tabla de §2, y esa marca dispara su revisión en el corte siguiente. El detalle por incremento vive en el `CHANGELOG.md` de la raíz del repositorio, que es la fuente autoritativa del avance.
