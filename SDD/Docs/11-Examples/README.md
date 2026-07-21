# 11-Examples — Samples de Sai-Service-Core

Esta carpeta contiene los markdown explicativos de los samples ejecutables del proyecto **Sai-Service-Core** (`web-monolith`). Cada markdown especifica un sample cuyo código se materializa en `/samples/<carpeta>/` del repositorio durante la codificación. Los samples son la primera puerta de entrada práctica para un desarrollador que consume el producto: se clonan, se ejecutan en un entorno limpio y se modifican como punto de partida.

El tipo D8 `web-monolith` no exige samples (son recomendados). Se producen igualmente **dos**, justificados por las dos superficies de la solución que un tercero puede querer ejercitar: el panel en vivo (demostrado con datos seed y adaptador simulado) y la API de ingesta (única superficie formal hacia terceros).

## Tabla maestra de samples

| Sample | Nivel | Tiempo de setup | CU ilustrados | Ubicación |
|---|---|---|---|---|
| `Ejemplo-01-Datos-Seed-v1.0.md` | Básico | < 5 min | CU-02, CU-04, CU-06, CU-12 | `/samples/01-datos-seed/` |
| `Ejemplo-02-Api-Ingesta-v1.0.md` | Intermedio | 10–15 min | CU-11 | `/samples/02-api-ingesta/` |

El Ejemplo 02 depende del catálogo base sembrado por el Ejemplo 01, por lo que se recomienda recorrerlos en orden.

## Convenciones de los samples

- **Autocontenidos.** Cada sample trae su propio `README.md`, sus prerequisites con versión mínima y su dataset o cuerpos de ejemplo. No dependen de servicios externos no triviales: el adaptador simulado (ADR-02) reemplaza al SAI físico y a NUT.
- **Ejecutables en entorno limpio en ≤ 5 pasos.** Todo sample arranca con `devcontainer up` y llega a su primer resultado en cinco pasos o menos. El único requisito del host es Docker; el SDK de .NET 10 vive dentro del Dev Container.
- **Nivel declarado.** Cada markdown declara su nivel (Básico o Intermedio) en su §2, con justificación respecto al anterior.
- **Output esperado exacto.** Cada sample documenta en su §6 el output textual exacto (estado del panel, cuerpos JSON de respuesta), tomado de los escenarios `§20` del intake. No hay valores inventados; los datos `reconstruido` de las fixtures viajan marcados como tales.
- **Trazabilidad obligatoria.** Cada sample enlaza en su §8 al menos un CU, ADR o RN vigente y los escenarios `§20` que ilustra.
- **Nomenclatura por capacidad, no por dominio.** Los samples se nombran por nivel o por capacidad demostrada (`datos-seed`, `api-ingesta`), nunca por una entidad del dominio.

## Cómo agregar un sample nuevo

1. Elegir el nombre por nivel o por capacidad (nunca por dominio), en `Título-Con-Guiones`, con sufijo `-v<X.Y>.md`.
2. Copiar la estructura de las nueve secciones obligatorias del §4.2 de las reglas constructivas (`11-Rules-Examples.md`).
3. Crear la carpeta ejecutable correspondiente en `/samples/XX-<capacidad>/` con su `README.md` propio.
4. Declarar la trazabilidad a CU/ADR/RN y a los escenarios `§20` del intake.
5. Agregar la fila a la tabla maestra de arriba con su nivel, tiempo de setup, CU ilustrados y ubicación.

El detalle normativo está en el §6 (criterios de aceptación) de `SDD/Devs/Rules/11-Rules-Examples.md`.

## Vínculo con el resto de la documentación

- **Arquitectura (05).** Los contratos que los samples respetan viven en `05-Arquitectura-Tecnica/`: `Contratos-REST-v1.0.md` (contrato de ingesta que ejercita el Ejemplo 02), `ADR-02` (adaptador de conexión simulado que hace posible el Ejemplo 01) y `ADR-17` (manejo de errores de la API).
- **Developer guide (10) omitida.** No hay categoría 10 (developer guide dedicada): se omitió por **ADR-23**. Por eso estos samples asumen el rol de material de arranque práctico y son autoexplicativos; los conceptos que en otros proyectos se documentarían en 10 se materializan aquí, ejecutándose.
- **README raíz del repositorio (Fase H).** La categoría 11 es el material de la fase de ejemplos del ciclo SDD; el README raíz del repositorio la enlaza como punto de entrada para consumidores externos.

## Matriz tipo D8 → `/samples`

| Tipo D8 | Estructura de `/samples` |
|---|---|
| web-monolith (base §2.3) | `01-datos-seed/`, `02-tema-custom/` (este último solo si hay punto de extensión visual) |
| web-monolith (este proyecto) | `01-datos-seed/`, `02-api-ingesta/` |
| (resto) | Ver §2.3 de `11-Rules-Examples.md`. |

**Desviación justificada respecto a la base §2.3.** El segundo sample de la plantilla `web-monolith` es `02-tema-custom/`, que **no aplica**: Sai-Service-Core no tiene punto de extensión visual (el panel Blazor con MudBlazor no es personalizable por terceros). En su lugar, el segundo sample es `02-api-ingesta/`, que ejercita la única superficie formal consumida por un tercero (la API de ingesta declarada en §18 del intake). El nombre es por capacidad (`api-ingesta`), no por el dominio del consumidor.

## Nota sobre validación en CI

Un pipeline que compile y ejecute los samples en cada push (criterio §6 ítem 14 de las reglas) es **recomendado fuerte** para `web-monolith` —obligatorio solo para `library`, `rest-api` y `cli-tool`—. La categoría 09 (DevOps) puede incorporar un job que levante el servicio con el adaptador simulado, cargue el seed y verifique los cuatro caminos de la ingesta; queda como recomendación, no como exigencia para este tipo de proyecto.
