# Bitácora de validación de la maqueta — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Bitacora-Validacion-Maqueta-v1.0.md
**Versión:** 1.0
**Estado:** Vigente
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03M)
**Variante:** UX/UI

**Trazabilidad:**
- Maqueta validada: `SDD/Maquetas/Sai-Service-Core/` (las 11 superficies, `assets/js/Datos-Maqueta.js`, `assets/js/Maqueta.js`, README.md).
- Documentos de 03 retroalimentados: `Experiencia-De-Uso-v1.0.md`, `Glosario-UX-v1.0.md`, `Wireframes-Acceso-Login-v1.0.md`, `Wireframes-Alta-Inicial-Administrador-v1.0.md`, `Wireframes-Alta-De-Equipos-v1.0.md`.
- Línea de base emitida a partir de esta validación: `Linea-Base-Visual-v1.0.md`, `Contrato-Datos-Maqueta-v1.0.md`.
- Regla que define este artefacto: `IA/IA.SDD/SDD/Devs/Rules/Deriva-Rules.md` §2.3.

---

## 0. Propósito

Registro de las iteraciones de validación de la maqueta con el humano durante la Fase B2. Cada iteración documenta la observación humana, la decisión de diseño derivada, el cambio aplicado sobre la maqueta y la cadena de documentos retroalimentados. La bitácora es la evidencia de tipo `humano` que respalda que la línea de base se emite desde una maqueta efectivamente mirada y aprobada, no desde una afirmación del agente (Deriva-Rules §7, anti-patrón «emitir línea de base sin Fase B2»).

Las tres iteraciones se realizaron el 2026-07-20, todas por la vía de prompt (el humano describe el cambio y el orquestador lo aplica sobre los archivos, releyendo e interpretando las diferencias como decisiones de diseño antes de propagarlas a 03).

---

## 1. Iteraciones

### It-1 · Terminología de las acciones del shell: «secreto» → «contraseña», «Salir» → «Cerrar Sesión»

- **Vía:** prompt. **Fecha:** 2026-07-20.
- **Observación humana:** el botón de la barra superior decía «Cambiar Secreto» y el enlace de salida decía «Salir».
- **Decisión de diseño:** usar el término de dominio orientado al operador. La credencial es «contraseña», no «secreto»; la salida es «Cerrar Sesión», más explícita que «Salir».
- **Cambio aplicado:** en la barra superior del shell (CMP-03), «Cambiar Secreto» → «Cambiar Contraseña» y «Salir» → «Cerrar Sesión».
- **Documento retroalimentado:** 03 (shell / acceso).
- **Evidencia:** `[EV-08 | artefacto | SDD/Maquetas/Sai-Service-Core/assets/js/Maqueta.js | barra superior del shell (líneas 827-828): «Cambiar Contraseña», «Cerrar Sesión» | 2026-07-20]`

### It-2 · Unificación de «secreto» → «contraseña» en toda la superficie de acceso

- **Vía:** prompt. **Fecha:** 2026-07-20.
- **Observación humana:** tras It-1, el término «secreto» seguía apareciendo en etiquetas, mensajes y estados de la cadena de acceso (alta inicial y login), produciendo inconsistencia con la acción ya renombrada.
- **Decisión de diseño:** unificar «secreto» → «contraseña» en toda la superficie de acceso: etiquetas de formulario, mensajes de éxito/error y nombres de estado. La coherencia del término prima sobre el nombre técnico del código de resultado.
- **Cambio aplicado:** etiquetas «Contraseña» y «Repetir contraseña» en el alta inicial y el login; mensajes de los códigos de resultado (p. ej. ACC-SECRETO-ACTUALIZADO exhibe «Contraseña actualizada»); bandas de error de requisito y de confirmación redactadas con «contraseña».
- **Documentos retroalimentados:** 03 — `Experiencia-De-Uso-v1.0.md`, `Wireframes-Acceso-Login-v1.0.md`, `Wireframes-Alta-Inicial-Administrador-v1.0.md`, `Glosario-UX-v1.0.md`.
- **Evidencia:** `[EV-09 | artefacto | SDD/Maquetas/Sai-Service-Core/assets/js/Datos-Maqueta.js | D.codigosResultado ACC-SECRETO-ACTUALIZADO / ACC-RECHAZO (líneas 40-45) | 2026-07-20]` y `[EV-10 | artefacto | SDD/Maquetas/Sai-Service-Core/assets/js/Maqueta.js | formAlta y V['Acceso-Login'] (líneas 131-190): etiquetas y bandas con «contraseña» | 2026-07-20]`

### It-3 · Término de dominio «parque» → «equipos» y renombre canónico de la superficie de alta

- **Vía:** prompt. **Fecha:** 2026-07-20.
- **Observación humana:** la superficie «Alta del parque» y el término «parque» a lo largo de la cadena documental resultaban jerga poco clara para el operador.
- **Decisión de diseño:** reemplazar el término de dominio «parque» por «equipos» en la superficie y en toda la cadena. Se evaluaron y descartaron las alternativas «Dispositivo» e «Inventario» porque colisionan con entidades y capas del modelo conceptual (Dispositivo §1.8, capa de inventario). «Equipos» no colisiona con ninguna entidad del modelo.
- **Cambio aplicado:**
  - Renombre canónico: `Alta-Del-Parque` → `Alta-De-Equipos`; archivo `Alta-De-Equipos.html`; título «Alta de equipos».
  - Renombre de archivos correspondientes en 01, 02 y 03.
  - Propagación del término «equipos» al intake y a la cadena documental completa.
- **Documentos retroalimentados:** cadena completa (01, 02, 03 y el intake).
- **Evidencia:** `[EV-11 | artefacto | SDD/Maquetas/Sai-Service-Core/assets/js/Datos-Maqueta.js | D.superficies id «Alta-De-Equipos», archivo «Alta-De-Equipos.html», título «Alta de equipos» (líneas 444-454) | 2026-07-20]` y `[EV-12 | artefacto | SDD/Docs/03-UX-UI-DX/Wireframes-Alta-De-Equipos-v1.0.md | archivo renombrado presente en 03 | 2026-07-20]`

---

## 2. Registro sintético

| Iteración | Vía | Fecha | Cambio | Documentos retroalimentados |
| --- | --- | --- | --- | --- |
| It-1 | Prompt | 2026-07-20 | «Cambiar Secreto» → «Cambiar Contraseña»; «Salir» → «Cerrar Sesión» | 03 (shell / acceso) |
| It-2 | Prompt | 2026-07-20 | Unificación «secreto» → «contraseña» en toda la superficie de acceso | 03: Experiencia-De-Uso, Acceso-Login, Alta-Inicial-Administrador, Glosario |
| It-3 | Prompt | 2026-07-20 | «parque» → «equipos»; renombre canónico Alta-Del-Parque → Alta-De-Equipos; propagación al intake | Cadena completa (01, 02, 03, intake) |

---

## 3. Terminología vigente tras la validación

- La superficie antes llamada «Alta del parque» es ahora **Alta de equipos** (canónico `Alta-De-Equipos`, archivo `Alta-De-Equipos.html`).
- El término **«parque» ya no se usa**: es **«equipos»**.
- La credencial es **«contraseña»**, no **«secreto»**.
- La acción de salida es **«Cerrar Sesión»**; la de cambio de credencial, **«Cambiar Contraseña»**.

Esta terminología es la que rige la línea de base visual y el contrato de datos emitidos en esta misma fase.

---

## 4. Aprobación

- **Aprobación humana explícita:** 2026-07-20.
- **Alcance aprobado:** las 11 superficies de la maqueta con la terminología vigente, sus estados demostrados y el contrato de campos exhibido.
- **Evidencia:** `[EV-13 | humano | Bitacora-Validacion-Maqueta-v1.0.md §4 | aprobación explícita 2026-07-20]`

A partir de esta aprobación, AG-03M emite la línea de base de sensado de deriva (`Linea-Base-Visual-v1.0.md` y `Contrato-Datos-Maqueta-v1.0.md`). Toda modificación posterior de la maqueta que separe la línea de base del sistema construido debe declararse y, si actualiza la línea de base, subir versión con nueva aprobación humana y re-disparar la propagación (Deriva-Rules §3, vía 2).

---

## 5. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Bitácora inicial de la Fase B2: registra It-1 (contraseña / cerrar sesión), It-2 (unificación de contraseña en acceso) e It-3 (equipos en lugar de parque, con renombre canónico) y la aprobación humana explícita del 2026-07-20. |
