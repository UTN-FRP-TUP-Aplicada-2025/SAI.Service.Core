# Ejemplo 02 — Cliente de la API de ingesta idempotente

**Proyecto:** Sai-Service-Core
**Documento:** Ejemplo-02-Api-Ingesta-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-11)
**Nivel:** Intermedio
**Ubicación del código:** `/samples/02-api-ingesta/`

## 1. Objetivo del sample

Ejercitar el contrato completo de `POST /api/v1/intervenciones` recorriendo sus **cuatro caminos de respuesta**: `201` (creado), `200` (reintento idempotente con `creado=false`), `409` (conflicto de idempotencia con huellas `sha256`) y `422` (invariante roto). Incluye explícitamente el **reintento**, que es el caso normal y no el excepcional: una fuente externa que no recibe la respuesta reenvía la misma petición. Al terminar, el desarrollador sabe qué cabeceras enviar (`X-Idempotency-Key`, `X-Fuente-Datos`), qué cuerpo espera el servicio y cómo interpretar cada código. Este sample corresponde al de la estrategia de demo del intake (§18).

## 2. Nivel

Intermedio. Asume que el lector ya corrió el Ejemplo 01 (datos seed) para tener el catálogo base sembrado (`ups-01`, `prov-taller-electronica-sur`, `ti-inspeccion`) y comprende la idea de idempotencia y de confianza diferenciada por fuente. Agrega, sobre el camino feliz, los tres caminos de error y el reintento, que exigen entender por qué la misma clave con cuerpo distinto devuelve `409` y no `200`.

## 3. Prerequisites

| Herramienta | Versión mínima | Uso |
|---|---|---|
| Docker Engine | 24.0 | Motor del Dev Container |
| Dev Container (spec containers.dev) | CLI `devcontainer` actual | Entorno con el SDK de .NET 10 |
| .NET SDK | 10.0 | Provisto dentro del Dev Container; corre el servicio |
| curl | cualquier versión moderna | Cliente HTTP que envía las 4 peticiones |
| Catálogo base sembrado | — | El seed de `E-1` (Ejemplo 01) debe estar cargado: las peticiones referencian `ups-01`, `prov-taller-electronica-sur` y `ti-inspeccion` |

No se necesita hardware ni NUT: la ingesta es una superficie HTTP pura de la capa `SAI.Service.Core.Api`. El sample consume la API por HTTP y no referencia código de la solución.

## 4. Cómo correrlo

Cinco pasos, en un entorno limpio, desde la raíz del repositorio:

1. Levantar el Dev Container:
   `devcontainer up --workspace-folder .`
2. Cargar el catálogo base (una vez) y correr el servicio con el adaptador simulado:
   `(cd samples/01-datos-seed && ./cargar-seed.sh) && SAI__AdaptadorConexion=Simulado ./scripts/run.sh SAI.Service.Core.Web`
3. Entrar a la carpeta del sample:
   `cd samples/02-api-ingesta`
4. Ejecutar el guion que envía las cuatro peticiones con curl:
   `./ingesta-4-caminos.sh`
5. Comparar cada respuesta impresa con el output exacto de la §6.

El guion usa las cabeceras `X-Idempotency-Key: gmao-ext-ot-88213` y `X-Fuente-Datos: fd-gmao-externo` y los cuerpos de ejemplo del escenario `§20.E-8`.

## 5. Estructura del código

```
02-api-ingesta/
├── README.md                    # propósito y reproducción del sample
├── ingesta-4-caminos.sh         # orquesta las 4 llamadas con curl e imprime código HTTP + cuerpo
├── cabeceras.env                # X-Idempotency-Key=gmao-ext-ot-88213, X-Fuente-Datos=fd-gmao-externo
└── cuerpos/                     # cuerpos de ejemplo, uno por camino (derivados de §20.E-8)
    ├── 01-crear-201.json        # intervención válida nueva (ti-inspeccion sobre ups-01, costos 12000 ARS)
    ├── 02-reintento-200.json    # misma clave, mismo cuerpo (idéntico a 01)
    ├── 03-conflicto-409.json    # misma clave, cuerpo distinto (costos.total.monto 12000 → 19500)
    └── 04-invariante-422.json   # costos que no cuadran (total 60000 ≠ Σ repuestos + mano de obra 67000)
```

## 6. Qué esperar

El guion imprime, en orden, las cuatro respuestas con su código y su cuerpo exactos, tomados de `§20.E-8`:

**Camino 1 — `201 Created` (clave nueva, cuerpo válido):**

```json
{
  "id": "int-20261002-001",
  "creado": true,
  "fuenteDatosId": "fd-gmao-externo",
  "confianzaAsignada": "media",
  "motivoConfianza": "Origen ApiExterna sin verificación cruzada. Los valores no fueron medidos por este servicio.",
  "registradoEn": "2026-10-02T09:04:22-03:00"
}
```

**Camino 2 — `200 OK` (reintento: misma clave, mismo cuerpo):**

```json
{
  "id": "int-20261002-001",
  "creado": false,
  "nota": "Clave de idempotencia ya procesada (gmao-ext-ot-88213). No se duplicó el registro.",
  "registradoEn": "2026-10-02T09:04:22-03:00"
}
```

**Camino 3 — `409 Conflict` (misma clave, cuerpo distinto):**

```json
{
  "error": "conflicto_idempotencia",
  "detalle": "La clave gmao-ext-ot-88213 ya fue procesada con un cuerpo diferente.",
  "huellaOriginal": "sha256:4f2a…",
  "huellaRecibida": "sha256:9b71…",
  "accionSugerida": "Emitir una clave nueva si es una intervención distinta, o corregir por el endpoint de rectificación si el original estaba mal."
}
```

**Camino 4 — `422 Unprocessable Entity` (invariante roto: costos que no cuadran):**

```json
{
  "error": "validacion",
  "campo": "costos.total",
  "detalle": "total (60000 ARS) ≠ Σ repuestos + manoDeObra (67000 ARS)",
  "invariante": "Costos.cuadra()"
}
```

Puntos clave verificables: el `201` asigna confianza `media` (menor que la del poller local, que es `alta`); el reintento devuelve `200` con `creado=false` y el **mismo** `id`, sin duplicar el registro; el `409` nunca degrada a `200` (devolver `200` haría creer al emisor que su corrección se aplicó); el `422` nombra el campo y el invariante violado.

## 7. Variaciones sugeridas

| Variación | Qué cambiar | Resultado esperado |
|---|---|---|
| Reintento idempotente | Reenviar `cuerpos/01-crear-201.json` con la misma `X-Idempotency-Key` | `200` con `creado=false` y el mismo `id` (I-19) |
| Conflicto de idempotencia | Reenviar con la misma clave pero `cuerpos/03-conflicto-409.json` (monto 19500) | `409 conflicto_idempotencia` con `huellaOriginal` y `huellaRecibida` |
| Dinero sin moneda ni fecha | Enviar `costos.total` con solo `monto` | `422` con `campo: "costos.total"`, invariante `I-18` (todo `Dinero` lleva moneda y fecha) |
| Referencia a entidad dada de baja | Fechar una intervención sobre `bat-2024-a` posterior a su baja (`2026-11-01`) | `422 coherencia_temporal`: la entidad se puede consultar pero no operar |

## 8. Trazabilidad

| Artefacto upstream | Tipo | Cómo lo ilustra este sample |
|---|---|---|
| CU-11 (Ingesta automatizada de intervenciones) | Caso de uso | Ejecuta el flujo de ingesta externa end-to-end por los cuatro caminos |
| F-20 (API REST de ingesta idempotente) | Capacidad funcional | Materializa el contrato `201/200/409/422` con `X-Idempotency-Key` |
| ADR-17 (Manejo de errores de la API de ingesta) | Decisión arquitectónica | Reproduce los cuerpos de error `409` (huellas `sha256`) y `422` (campo + invariante) |
| RN-09 (Idempotencia de la ingesta) | Regla de negocio | Misma clave + mismo cuerpo ⇒ `200`; misma clave + cuerpo distinto ⇒ `409` |
| RN-08 (Cuadre de costos de intervención) | Regla de negocio | `Costos.cuadra()` rompe primero: `422` cuando el total no coincide con la suma |
| RN-07 (Todo importe con moneda y fecha) | Regla de negocio | `I-18`: `Dinero` sin moneda o fecha ⇒ `422` |
| Escenario §20.E-8 | Fixture de datos | Fuente de las cabeceras, los cuerpos y las respuestas exactas |

## 9. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Versión inicial. Cliente HTTP de referencia de los cuatro caminos de `POST /api/v1/intervenciones`. |
