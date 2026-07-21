# Contrato REST — API de ingesta de intervenciones

**Proyecto:** Sai-Service-Core
**Documento:** Contratos-REST-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)

---

## 1. Alcance del contrato

Este documento especifica el **único contrato formal hacia terceros** de Sai-Service-Core: la API REST de **ingesta de intervenciones**, que materializa **CU-11** (ingesta automatizada de intervenciones). El consumidor es un sistema externo de gestión de mantenimiento (GMAO), que empuja intervenciones sin intervención humana con una clave de idempotencia, de modo que un reintento de red no duplique el hecho ni corrompa el histórico. El panel Blazor y el adaptador de conexión hacia NUT **no** son parte de este contrato: el panel es interfaz de usuario sin contrato versionado, y el adaptador es una superficie saliente (el servicio es cliente de NUT), documentada en `Extensibilidad-v1.0.md`.

Los datos ingresados por esta API quedan con confianza **media** (menor que la del poller local, que es `alta`). Gobierna ADR-17; el endpoint de rectificación que sugiere el 409 queda **Propuesto en ADR-21**.

## 2. Formato

- **Estilo:** REST sobre HTTP/1.1 en la LAN, cuerpos JSON en UTF-8.
- **Especificación:** OpenAPI 3.1 inline (resumen abajo); el `.yaml` completo se materializa en `SAI.Service.Core.Api` y se sirve como documento navegable del servicio.
- **Errores:** problem+json según **RFC 7807** (`Content-Type: application/problem+json`).
- **Versión:** en la ruta, `/api/v1/`.

Resumen OpenAPI (inline):

```yaml
openapi: 3.1.0
info:
  title: Sai-Service-Core — API de ingesta
  version: "1.0"
paths:
  /api/v1/intervenciones:
    post:
      summary: Ingesta idempotente de una intervención de servicio técnico
      parameters:
        - name: X-Idempotency-Key
          in: header
          required: true
          schema: { type: string, maxLength: 200 }
        - name: X-Fuente-Datos
          in: header
          required: true
          schema: { type: string }   # fuente de datos registrada, con su confianza base
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: "#/components/schemas/IntervencionEntrada" }
      responses:
        "201": { description: Creada con clave nueva }
        "200": { description: Reintento idempotente; creado=false }
        "409": { description: Conflicto de idempotencia (misma clave, cuerpo distinto) }
        "422": { description: Invariante de negocio roto }
```

## 3. Operaciones

| Método | Ruta | Operación | Idempotente | CU |
| --- | --- | --- | --- | --- |
| POST | `/api/v1/intervenciones` | Registrar una intervención de servicio técnico con idempotencia por clave | Sí (por `X-Idempotency-Key`) | CU-11 |

Cabeceras obligatorias en toda solicitud: `X-Idempotency-Key` (clave provista por el emisor) y `X-Fuente-Datos` (fuente de datos registrada, que aporta la confianza base). El endpoint de salud (`/health` o equivalente) es público y no forma parte de este contrato de negocio.

## 4. Esquemas de datos

**DTO de entrada `IntervencionEntrada`** (derivado de la `Intervencion` del modelo conceptual, entidad 1.23):

| Campo | Tipo | Obligatorio | Semántica |
| --- | --- | --- | --- |
| `tipoIntervencionId` | string | Sí | Clase de servicio técnico (`TipoIntervencion`) |
| `dispositivoId` | string | Sí | Dispositivo afectado |
| `bateriaIds` | string[] | Condicional | Baterías afectadas (según el tipo) |
| `proveedorId` | string | No | Ejecutor externo (`Proveedor`), si aplica |
| `tiempoValido` | date-time | Sí | Cuándo ocurrió la intervención (bitemporalidad) |
| `costos` | `Costos` | Sí | Repuestos, mano de obra y total; debe cuadrar (RN-08) |
| `hallazgos` | string | No | Observaciones de la intervención |
| `disposicionFinal` | `DisposicionFinal` | No | Destino y receptor de una batería retirada (trazabilidad ambiental) |

**Objeto `Costos`** (invariante `Costos.cuadra()`): `repuestos: Dinero[]`, `manoDeObra: Dinero`, `total: Dinero`; `total` iguala la suma de repuestos y mano de obra (RN-08, I). **Objeto `Dinero`**: `monto: number`, `moneda: string` (obligatoria), `fecha: date` (obligatoria), `equivalenteNormalizado?` con su fuente de cotización (RN-07, I-18).

**DTO de salida** (respuestas 201/200): `{ id: string, creado: boolean, confianza: "media", tiempoValido, tiempoRegistrado }`. En 201 `creado=true`; en 200 `creado=false` y `id` es el del registro previo.

## 5. Manejo de errores

Cuatro caminos de respuesta (§17 P.3, CU-11 §6). Los cuerpos de error siguen problem+json.

| Código | Condición | Cuerpo | Regla |
| --- | --- | --- | --- |
| **201 Created** | Cuerpo válido y clave nueva | `{ id, creado: true, confianza: "media", tiempoValido, tiempoRegistrado }` | CU-11 FP; confianza media por origen externo |
| **200 OK** | Clave ya procesada con **el mismo** cuerpo | `{ id, creado: false }` (el mismo id) | RN-09; el reintento es el caso normal (CL-21) |
| **409 Conflict** `conflicto_idempotencia` | Misma clave con **cuerpo distinto** | problem+json con `sha256Original`, `sha256Recibido` y `accionSugerida`; **nunca 200** | RN-09; *"Devolver 200 sería peor que duplicar"* (CL-21) |
| **422 Unprocessable Entity** | Invariante de negocio roto | problem+json con `campo` e `invariante` violado | Ver desglose abajo |

Desglose del 422 (`type` en problem+json):

| `invariante` | Causa | Regla |
| --- | --- | --- |
| `validacion` (cuadre de costos) | El total no iguala repuestos + mano de obra | RN-08 (`Costos.cuadra()`); CL-22 |
| `validacion` (dinero incompleto) | Un `Dinero` sin moneda o sin fecha | RN-07 (I-18) |
| `coherencia_temporal` | Intervención fechada para operar sobre una entidad **después** de su baja | RN-12; referenciar la entidad solo para consultar su historial sí es válido (CL-20) |

Errores reservados: los códigos de `type` (`conflicto_idempotencia`, `validacion`, `coherencia_temporal`) son estables y forman parte del contrato; no se renombran sin cambio de versión.

## 6. Versionado del contrato

- **Versión en la ruta:** `/api/v1/`. La estrategia sigue SemVer 2.0.0 del proyecto (§17 P.7).
- **Cambios aditivos** (campos opcionales nuevos en el DTO de entrada, nuevos `type` de error no reservados): **no rompen** versión; se sirven bajo `/api/v1/`.
- **Cambios incompatibles** (quitar o renombrar un campo obligatorio, cambiar la semántica de un código de estado, cambiar un `type` reservado): abren **`/api/v2/`**. `v1` se mantiene mientras haya al menos un consumidor declarado (hoy, el GMAO externo `fd-gmao-externo`).
- **Deprecación:** una versión se marca obsoleta cuando su último consumidor migra; hasta entonces coexisten.
- **Idempotencia y compatibilidad:** la semántica de `X-Idempotency-Key` es parte del contrato estable; su ausencia es un error del cliente, no un caso soportado.

## 7. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| CU cubiertos | CU-11 (ingesta automatizada de intervenciones) |
| RN aplicables | RN-07 (importe con moneda y fecha), RN-08 (cuadre de costos), RN-09 (idempotencia de la ingesta), RN-12 (baja lógica y coherencia temporal) |
| ADR que lo gobiernan | ADR-17 (manejo de errores de la API de ingesta); ADR-21 [Propuesto] (contrato del endpoint de rectificación del 409) |
| NFR relacionados | N-23 (idempotencia 100 %), N-24 (procedencia en todo valor) |
| Escenario de datos | §20.E-8 (ingesta desde servicio externo con reintento y casos de error) |
| Sample | `samples/ingesta-gmao/` — cliente HTTP que ejercita los cuatro caminos (§16.1) |
| Tests previstos en 08 | Los cuatro caminos 201/200/409/422, idempotencia por clave, degradación de confianza, dos tiempos (`tiempoValido`/`tiempoRegistrado`) |

**Pendiente declarado.** El **endpoint de rectificación** que sugiere la `accionSugerida` del 409 tiene su contrato **sin definir**: es el pendiente P-05, documentado como **ADR-21 en estado Propuesto**. Hasta cerrarlo, la corrección de un hecho ya ingresado se marca como pendiente y su flujo no se especifica en esta versión.

## Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Contrato inicial de `POST /api/v1/intervenciones`: 7 secciones del §4.5, OpenAPI inline, problem+json RFC 7807, esquemas DTO/headers, cuatro caminos 201/200/409/422, versionado en ruta y trazabilidad a CU-11, RN-07/08/09/12 y ADR-17/21. |
