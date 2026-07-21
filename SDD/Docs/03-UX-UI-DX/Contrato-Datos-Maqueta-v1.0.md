# Contrato de datos de la maqueta — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Contrato-Datos-Maqueta-v1.0.md
**Versión:** 1.0
**Estado:** Vigente
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03M)
**Variante:** UX/UI

**Trazabilidad:**
- Modelo conceptual de 02: `SDD/Docs/02-Especificacion-Funcional/Modelo-Datos/Modelo-Conceptual-v1.0.md` (entidades y atributos correspondientes).
- Línea de base visual: `Linea-Base-Visual-v1.0.md` (identificadores `SUP-XX` de las superficies donde aparece cada campo).
- Maqueta aprobada: `SDD/Maquetas/Sai-Service-Core/assets/js/Datos-Maqueta.js` (contrato de campos y datos de ejemplo verbatim de SOLUTION-INTAKE §20.E-1..E-8).
- Regla que define este artefacto: `IA/IA.SDD/SDD/Devs/Rules/Deriva-Rules.md` §2.2.
- Validación humana: `Bitacora-Validacion-Maqueta-v1.0.md`, aprobación explícita 2026-07-20.

---

## 0. Propósito y método

Este contrato cierra la brecha entre el modelo conceptual de 02, que es abstracto, y lo que el humano efectivamente vio y aprobó en la maqueta. Cada campo que la maqueta exhibe tiene su fila `DM-XX` con nombre, tipo, obligatoriedad, ejemplo tomado de la maqueta, superficies donde aparece, correspondencia a la entidad y atributo del modelo conceptual, y la regla de negocio que lo condiciona si la hay. El formato de presentación es parte del contrato: una fecha o un valor con procedencia que en el sistema construido pierda su forma acordada es deriva, aunque el dato sea el mismo (Deriva-Rules §2.2).

La fuente única de los campos es el arreglo `D.contratoCampos` de la maqueta, exhibido en el `index.html` como «Contrato de campos que exhibe la maqueta». Ningún dato de ejemplo se inventa: sale verbatim de `Datos-Maqueta.js` (procedencia SOLUTION-INTAKE §20.E-1..E-8).

**Evidencia de origen del contrato:**
- `[EV-05 | artefacto | SDD/Maquetas/Sai-Service-Core/assets/js/Datos-Maqueta.js | D.contratoCampos (líneas 370-395) | 2026-07-20]`
- `[EV-06 | artefacto | SDD/Docs/02-Especificacion-Funcional/Modelo-Datos/Modelo-Conceptual-v1.0.md | §1 Entidades y §2 Atributos clave | 2026-07-20]`
- `[EV-07 | artefacto | SDD/Maquetas/Sai-Service-Core/index.html | tabla-contrato (líneas 53-59) | 2026-07-20]`

**Convención de obligatoriedad:** Sí = obligatorio en la entidad; Anulable = obligatorio como campo pero con nulo intencional admitido; Derivado = calculado, no ingresado.

---

## 1. Campos exhibidos (`DM-XX`)

| ID | Campo | Tipo | Oblig. | Ejemplo en la maqueta | Superficies | Entidad · atributo (02) | Regla de negocio |
| --- | --- | --- | --- | --- | --- | --- | --- |
| DM-01 | `input.voltage` | Tensión (V), medido | Sí | `232,9 V` | SUP-03, SUP-07, SUP-11 | Muestra · valores con origen | RN-05, RC-01 (procedencia) |
| DM-02 | `battery.voltage` | Tensión (V), medido | Sí | `13,41 V` | SUP-03, SUP-06, SUP-09 | Muestra · valores con origen | RN-05, RC-01 |
| DM-03 | `battery.charge` | Porcentaje, derivado | Derivado | `100 % [derivado]` | SUP-03 | Muestra · valor con origen (Valor con Origen) | RN-05, CA-03: siempre marcado derivado |
| DM-04 | `ups.load` | Porcentaje, medido | Sí | `12 %` | SUP-03, SUP-07 | Muestra · valores con origen | RN-05, RC-01 |
| DM-05 | `ups.status` | Enum OL/OB, medido | Sí | `OL` | SUP-03 | Muestra · valores con origen | RC-01; deriva eventos vía ReglaDerivacion (RC-09) |
| DM-06 | `cobertura` | Fracción [0..1] | Sí | `0,997` | SUP-07, SUP-11 | Agregado · cobertura | RC-04: cobertura y advertencia obligatorias |
| DM-07 | `duracionSegundos` | Entero + incertidumbre | Sí | `5 s (±10 s)` | SUP-03, SUP-11 | Evento · duración, incertidumbre | RC-09 |
| DM-08 | `reglaVersion` | Entero | Sí | `2` | SUP-03, SUP-07 | Evento · reglaVersion (ReglaDerivacion) | RC-09: evento referido a regla versionada |
| DM-09 | `modalidad` | Enum | Sí | `SoloAlerta` | SUP-03, SUP-05 | VersionPolitica · modalidad | RN-11, RC-05 |
| DM-10 | `umbralDisparoSegundos` | Segundos | Sí | `300 s` | SUP-05 | VersionPolitica · umbralDisparo | — |
| DM-11 | `tiempoReservadoApagadoSeg` | Segundos (≤ 540) | Anulable | `240 s` | SUP-05 | VersionPolitica · tiempoReservadoApagado | RN-04: no mayor a 540 s (techo duro del equipo) |
| DM-12 | `modalidadSolicitada / efectiva` | Enum / enum | Sí | `HostLuegoUpsConRetorno / SoloAlerta` | SUP-03 | Accion · modalidadSolicitada, modalidadEfectiva | RN-02, RN-03, RC-05 |
| DM-13 | `estado (verificacion)` | Enum | Sí | `NuncaVerificado / Verificado / Vencido / Refutado` | SUP-04, SUP-08 | Verificacion · estado | RN-01, RN-02 |
| DM-14 | `vigenciaDias` | Entero o null | Anulable | `180 / 365 / sin caducidad` | SUP-08 | Verificacion · vigencia | RN-02; null = sin caducidad |
| DM-15 | `caidaV (prueba)` | Tensión (V), derivado | Derivado | `-0,47 V` | SUP-06 | PruebaBateria · derivados | RN-06, RC-06, RC-07 |
| DM-16 | `veredicto / confianza` | Enum / enum | Derivado | `SinDegradacionDetectable / baja` | SUP-06 | PruebaBateria · veredicto | RN-06; calculado por el servicio, no por el equipo |
| DM-17 | `Dinero (monto, moneda, fecha)` | Importe fechado | Sí | `67.000 ARS @ 2026-09-05` | SUP-09, SUP-11 | Dinero · monto, moneda, fecha | RN-07: moneda y fecha obligatorias |
| DM-18 | `equivalenteUsd` | Importe derivado | Derivado | `52,80 USD [BNA]` | SUP-09, SUP-11 | Dinero · equivalenteNormalizado | RN-07; con fuente de cotización (BNA) |
| DM-19 | `diasEnServicio` | Entero | Derivado | `654` | SUP-09, SUP-11 | FichaVidaUtil · díasEnServicio | RN-06 |
| DM-20 | `costoPorAnioDeServicio` | Importe derivado | Derivado | `37.430 ARS → 29,50 USD` | SUP-09, SUP-11 | FichaVidaUtil · costo por año normalizado | RN-06, RN-07 |
| DM-21 | `desde / hasta (montaje)` | Intervalo temporal | Sí | `2024-11-20 → 2026-09-05` | SUP-06, SUP-09 | MontajeBateria · desde, hasta | RC-02, RC-03, RC-07 |
| DM-22 | `desde / hasta (cobertura)` | Intervalo temporal | Anulable (hasta abierto) | `2024-11-20 → abierto` | SUP-10, SUP-11 | CoberturaHost · desde, hasta | RC-02, RC-03; a lo sumo una vigente |
| DM-23 | `estado (unidad) / motivoBaja` | Enum / texto | Anulable (motivo si baja) | `DadoDeBaja / FinDeVidaUtil` | SUP-09, SUP-10, SUP-11 | UnidadFisica · estado, fechaBaja, motivoBaja | RC-08: baja lógica, sin borrado |
| DM-24 | `confianza (fuente)` | Enum | Sí | `alta (local) / media (externa)` | SUP-09, SUP-11 | FuenteDatos · confianza base | RN-09 |

Evidencia por fila: `EV-05` (definición del campo en `D.contratoCampos`) y `EV-06` (entidad y atributo correspondientes en el modelo conceptual). El ejemplo de cada fila es el `ejemplo` verbatim de `D.contratoCampos`.

---

## 2. Formato de presentación acordado

El formato es parte del contrato (Deriva-Rules §2.2 y §3, dimensión Modelo de datos). Se aprobaron estas formas de presentación:

- **Separador decimal coma** (rioplatense): `232,9`, `13,41`, `0,997`, `-0,47`. No punto decimal.
- **Unidad junto al valor**: `V`, `%`, `s`, `USD`, `ARS`.
- **Procedencia visible en el valor derivado**: `100 % [derivado]`; la carga de batería nunca se presenta como medida (DM-03).
- **Incertidumbre junto a la duración**: `5 s (±10 s)` (DM-07).
- **Importe siempre con moneda y fecha**: `67.000 ARS @ 2026-09-05` (DM-17); el equivalente lleva su fuente: `52,80 USD [BNA]` (DM-18).
- **Fechas en formato ISO corto** `AAAA-MM-DD`: `2024-11-20`.
- **Intervalo con flecha y extremo abierto explícito**: `2024-11-20 → abierto` (DM-22), no un vacío mudo.
- **Enum en su forma canónica de dominio**: `SoloAlerta`, `HostLuegoUpsConRetorno`, `NuncaVerificado`. No traducciones sueltas.

Cambiar cualquiera de estas formas en el sistema construido es deriva de formato de presentación, aunque el dato subyacente sea el mismo.

---

## 3. Campos del modelo conceptual no exhibidos

Campos y entidades del modelo conceptual de 02 que ninguna superficie de la maqueta exhibe. Un campo que el humano nunca vio no está validado visualmente (Deriva-Rules §2.2).

| Entidad · atributo (02) | Motivo de no exhibición |
| --- | --- |
| Intervencion · claveIdempotencia | Clave interna de idempotencia (RN-08); no es dato de usuario, no se presenta. |
| MontajeBateria · posicion | No ejemplificada en §20.E-1..E-8; la maqueta muestra el montaje sin la posición. |
| ModeloBateria · vidaFlotacionEsperada | Aparece solo como condición del error «dato obligatorio inválido» (RN-13) en SUP-04, no como valor exhibido. |
| Agregado · funcion | La maqueta exhibe la resolución (`Agregados PT1H`) y la cobertura, pero no la función de agregación como campo. |
| ReglaDerivacion · umbral (p. ej. microcorte 60 s) | Se exhibe la versión de la regla (DM-08), no su parametrización interna. |
| Verificacion · evidencia | Se exhibe estado, método y vigencia (DM-13, DM-14); la evidencia solo aparece como nota, no como campo estructurado. |
| Politica (agrupador) · id | Se exhibe la VersionPolitica vigente (`vp-001`) y la propuesta (`vp-003`), no la Politica agrupadora (RC-05). |
| SesionSondeo · driver, versión, dialecto | Solo el `dialecto` (`megatec/qx`) y el intervalo aparecen tangencialmente en SUP-04/SUP-03; el resto de la sesión no se exhibe. |
| UnidadFisica · fechaBaja (como campo aislado) | Se exhibe el estado y el motivo de baja (DM-23); la fecha de baja aparece dentro de las intervenciones, no como campo propio. |
| Fabricante (entidad) | No se exhibe como entidad: la marca aparece como valor declarado del SAI (`Sin identificar`) dentro de SUP-04, no como catálogo de fabricantes. |

Estos campos existen en el modelo conceptual y deberán implementarse, pero su presentación no está validada visualmente y no forma parte de la línea de base de sensado hasta que una superficie los exhiba.

---

## 4. Discrepancias detectadas maqueta ↔ modelo / especificación

- **Sello de versión (`versionLegible`, `identificadorDeConstruccion`)**: la maqueta exhibe estos campos (CMP-04) pero provienen de la extensión Identidad-De-Version, no del modelo conceptual de 02. En la maqueta van marcados «sin dato de ejemplo» (`esPreliminar = true`, artefacto `-alpha.N`). No son campos del modelo de datos de dominio: no reciben `DM-XX` y se documentan aquí como exhibidos fuera del modelo conceptual.
- **Datos de sustitución (SUP-10)**: la sucesión de coberturas está marcada en la maqueta como «reconstruida para la maqueta» (riesgo R-11 del intake): el flujo no tiene escenario §20 completo. Los campos correspondientes (DM-22, DM-23) son válidos, pero sus valores de ejemplo en SUP-10 son reconstruidos, no verbatim de §20.
- **`intervaloSondeoSegundos`**: descriptor exhibido en SUP-05 y como intervalo de conectividad en SUP-03; corresponde a `SesionSondeo.intervaloSegundos` en el modelo. No figura en `D.contratoCampos`, por lo que no recibe `DM-XX` propio; se registra como campo exhibido fuera del contrato de campos declarado.

Ninguna discrepancia es un campo que aparezca en la maqueta y no exista en el modelo conceptual: todos los `DM-XX` tienen correspondencia. No se agregó ninguna fila sin respaldo en el modelo.

---

## 5. Resumen del contrato

| Concepto | Cantidad |
| --- | --- |
| Campos exhibidos (`DM-XX`) | 24 |
| Campos del modelo conceptual no exhibidos (declarados con motivo) | 10 |
| Discrepancias registradas | 3 |

Los 24 `DM-XX` cubren la totalidad del arreglo `D.contratoCampos` de la maqueta. Cada uno tiene correspondencia a una entidad y atributo del modelo conceptual de 02.
