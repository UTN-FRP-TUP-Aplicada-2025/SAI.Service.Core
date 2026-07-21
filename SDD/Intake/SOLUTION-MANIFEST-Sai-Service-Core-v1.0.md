# SOLUTION-MANIFEST — SAI.Service.Core

Artefacto derivado. El orquestador SDD lo construyó a partir de `SOLUTION-INTAKE-Sai-Service-Core-v1.0.md` §13, siguiendo las reglas de derivación de `Intake-Rules.md` §4 y el formato de `SOLUTION-MANIFEST-template.md`. No se completa a mano.

---

## §1 Bloque de solución

| Campo | Valor |
|---|---|
| Nombre de solución | SAI.Service.Core |
| `Nombre-Solucion` | `Sai-Service-Core` |
| `NombreSolucionCodigo` | `SAI.Service.Core` |
| Proyecto principal | `Sai-Service-Core` |
| Intake (origen) | `SOLUTION-INTAKE-Sai-Service-Core-v1.0.md` (de su §13 se deriva este manifiesto) |
| Documento | `SOLUTION-MANIFEST-Sai-Service-Core-v1.0.md` |
| Versión | 1.1 |
| Fecha | 2026-07-20 |
| Estado | En revisión |

Nota de derivación: el intake §13 declara que el nombre legible ya es dotted PascalCase y se conserva literal (`SAI.Service.Core`), con la sigla `SAI` en mayúscula completa. El algoritmo de normalización de `Master-Prompt.md` §3.2 se aplica al `Nombre-Solucion` (Título-Con-Guiones), que resulta `Sai-Service-Core`.

### §1.1 Perfil de convención de nombres

| Parámetro | Valor | Notas |
|---|---|---|
| Forma del nombre de solución en código | PascalCase con segmentos separados por punto | `SAI.Service.Core`, conservado literal del nombre legible |
| Separador de segmentos | `.` | Separa la raíz de la solución del sufijo de capa |
| Prefijo de paquetes redistribuibles | `Aplicada` | No se usa: no hay proyectos redistribuibles en esta solución |

---

## §2 Tabla de proyectos

| `Nombre-Proyecto` | `nombre-proyecto-codigo` | `project_type` (D8) | Rol en la solución | `redistribuible` | Dependencias | Path `/src` |
|---|---|---|---|---|---|---|
| `Sai-Service-Core` | `SAI.Service.Core` | `web-monolith` | Servicio web único que monitorea el SAI, decide y ejecuta el apagado ordenado, administra el ciclo de vida de los equipos y expone panel y API REST (principal) | false | — | `src/SAI.Service.Core/` |

Nota de derivación: el sufijo orientativo `.Web` de `SOLUTION-MANIFEST-template.md` §2.1 no se aplica a la raíz del proyecto. El intake §13 fija que el nombre de código del proyecto es `SAI.Service.Core`, materializado como cinco assemblies `SAI.Service.Core.<Capa>` (`Domain`, `Application`, `Infrastructure`, `Api`, `Web`) bajo `src/`. Esas capas son internas al único proyecto D8, no proyectos de la solución.

---

## §3 Grafo de dependencias

```text
[Sai-Service-Core]
```

Grafo de un solo nodo, sin aristas: trivialmente acíclico.

Orden topológico:

```text
nivel 0: Sai-Service-Core
```

Caso degenerado (solución de un único proyecto): el orquestador aplana el layout según `Master-Prompt.md` §3.5. Las categorías 00 a 11 se generan directamente bajo `SDD/Docs/`, sin el subnivel `Proyectos/<Nombre-Proyecto>/` ni la carpeta `Solucion/`. La vista de solución y el pipeline de solución se omiten.

---

## §4 Checklist de validación del manifiesto derivado

- [x] El bloque de solución tiene nombre, `Nombre-Solucion`, `NombreSolucionCodigo`, proyecto principal y referencias de intake completos.
- [x] El perfil de convención de nombres está declarado (forma PascalCase, separador, prefijo de redistribuibles).
- [x] La tabla de proyectos tiene al menos una fila y todos los campos obligatorios completos.
- [x] Cada `project_type` pertenece al conjunto cerrado D8 de 8 valores (`web-monolith`).
- [x] Hay exactamente un proyecto principal.
- [x] No hay colisiones de `Nombre-Proyecto` ni de `nombre-proyecto-codigo` (un solo proyecto).
- [x] Cada dependencia referencia un proyecto existente en la tabla (no hay dependencias).
- [x] El grafo de dependencias es acíclico (un nodo, sin aristas).
- [x] Cada proyecto marcado `redistribuible: true` arranca su nombre de código con el prefijo de organización (no aplica: no hay redistribuibles).
- [x] El control de cambios refleja la versión y fecha del documento.

---

## Control de cambios

| Versión | Fecha | Cambios | Autor |
|---|---|---|---|
| 1.0 | 2026-07-20 | Manifiesto inicial derivado de `SOLUTION-INTAKE-Sai-Service-Core-v1.0.md` §13 durante la Fase de validación de intake. | Orquestador SDD |
| 1.1 | 2026-07-20 | Actualización de la descripción de rol: «ciclo de vida del parque» → «ciclo de vida de los equipos», por unificación de terminología de la Fase B2. | Orquestador SDD |
