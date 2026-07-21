# Ejemplo 01 — Precarga de datos de ejemplo con adaptador simulado

**Proyecto:** Sai-Service-Core
**Documento:** Ejemplo-01-Datos-Seed-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-11)
**Nivel:** Básico
**Ubicación del código:** `/samples/01-datos-seed/`

## 1. Objetivo del sample

Precargar el servicio con un juego de datos completo (equipos, baterías, vínculos temporales, muestras, eventos, prueba de batería e informe de período, tomados de los escenarios `E-1`…`E-8` del intake) y arrancarlo contra el **adaptador de conexión simulado** (ADR-02), de modo que el desarrollador explore el panel en vivo, los históricos, la prueba de batería y el informe de período **sin el SAI físico, sin hardware USB y sin NUT**. Al terminar, sabe cómo se ve el sistema andando y por qué arranca degradado a `SoloAlerta`, que es el onboarding «ver el sistema funcionando sin equipo».

## 2. Nivel

Básico. Es el punto de entrada absoluto: no requiere hardware, ni entender la API de ingesta, ni conocer el modelo de dominio. Todo el sample se reduce a levantar el entorno, cargar el seed y abrir el panel. El Ejemplo 02 (API de ingesta) parte de este estado sembrado y agrega la complejidad de un contrato HTTP con idempotencia.

## 3. Prerequisites

| Herramienta | Versión mínima | Uso |
|---|---|---|
| Docker Engine | 24.0 | Motor del Dev Container (único requisito del host, §17 P.1) |
| Dev Container (spec containers.dev) | CLI `devcontainer` actual | Orquesta el entorno; el SDK vive dentro del contenedor |
| .NET SDK | 10.0 (sin *fallback* a versiones anteriores) | Provisto dentro del Dev Container; no se instala en el host |
| Navegador | Chromium 120 / Firefox 121 | Acceso al panel Blazor Server por WebSocket |

No se necesita NUT, ni dispositivo USB, ni el SAI físico: el adaptador simulado (ADR-02) reemplaza el acceso al equipo. La base de datos es SQLite en archivo, creada por las migraciones de EF Core al cargar el seed.

## 4. Cómo correrlo

Cuatro pasos hasta ver el panel poblado, todos desde la raíz del repositorio en un entorno limpio:

1. Levantar el Dev Container (trae el SDK de .NET 10):
   `devcontainer up --workspace-folder .`
2. Aplicar las migraciones y cargar el dataset `E-1`…`E-8`:
   `cd samples/01-datos-seed && ./cargar-seed.sh`
3. Correr el servicio con el adaptador de conexión simulado:
   `SAI__AdaptadorConexion=Simulado ./scripts/run.sh SAI.Service.Core.Web`
4. Abrir `http://localhost:5000` en Chromium ≥120 o Firefox ≥121 y autenticarse con el administrador único.

El adaptador simulado reproduce la secuencia de estados descrita en `config/adaptador-simulado.json` (régimen normal, un microcorte y un corte prolongado), de modo que el panel muestra estado en vivo sin ningún equipo conectado.

## 5. Estructura del código

```
01-datos-seed/
├── README.md                        # propósito y reproducción del sample
├── cargar-seed.sh                   # aplica migraciones EF Core y ejecuta el seeder
├── datos/                           # dataset de ejemplo, uno por escenario §20
│   ├── E-1-catalogo-inventario-vinculos.json   # alta base: fabricantes, ups-01/ups-02, bat-2024-a, montaje y cobertura, 4 verificaciones en NuncaVerificado
│   ├── E-2-sondeo-normal.json                   # sesión de sondeo y muestras con procedencia por variable
│   ├── E-3-microcorte.json                      # evento Microcorte derivado, sin acción
│   ├── E-4-corte-prolongado.json                # CorteSuministro que dispara y queda BloqueadaPorVerificacion
│   ├── E-5-prueba-bateria.json                  # PruebaBateria trimestral con veredicto calculado por el servicio
│   ├── E-6-recambio-bateria.json                # intervención: baja lógica y cierre de vigencia
│   ├── E-7-informe-periodo.json                 # consulta inversa «qué pasó en este período»
│   └── E-8-ingesta-externa.json                 # intervención ingresada por API (referencia del Ejemplo 02)
├── config/
│   └── adaptador-simulado.json      # guion de estados que reproduce el adaptador simulado (ADR-02)
└── SeedRunner/                      # invocador que llama al caso de uso de alta (CU-02) con los datos de datos/
    └── README.md                    # cómo el seeder respeta invariantes y procedencia al insertar
```

El seeder no inserta filas a mano: llama a los casos de uso de alta (CU-02) para que los datos entren respetando las reglas de negocio (procedencia obligatoria, montajes sin solapamiento, verificaciones sembradas en `NuncaVerificado`).

## 6. Qué esperar

Tras el paso 4, el panel muestra el estado del escenario base sembrado (valores medidos de `E-1`/`E-2`, 2026-07-19):

- **Estado en vivo:** `ups.status = OL`, `input.voltage = 232.9 V`, `output.voltage = 232.9 V`, `battery.voltage = 13.41 V`, `ups.load = 12–13 %`. `battery.charge = 100 %` aparece **marcado como derivado** (interpolación del driver sobre umbrales estimados; no usar como umbral duro).
- **Conectividad:** correcta; sin alerta de `DesconexionUsb` (el adaptador simulado responde).
- **Panel de supuestos:** aviso permanente en la pantalla principal «operativo · **0 de 4 supuestos verificados**», con la política `pol-apagado-por-corte` degradada a **`SoloAlerta`**. Es el estado esperado al alta: sin la ventana de mantenimiento (CU-10), el sistema avisa pero no apaga.
- **Eventos recientes:** un `Microcorte` (`E-3`, duración 5 s con incertidumbre ±10 s, sin acción) y un `CorteSuministro` (`E-4`, 370 s, severidad crítica) cuyo resultado de acción es `BloqueadaPorVerificacion`.
- **Históricos (CU-06):** gráfica de `input.voltage` con el agregado horario de `E-2` (promedio 232.4 V, mínimo 229.8 V, máximo 235.1 V, cobertura 0.997), con las marcas de eventos superpuestas.
- **Prueba de batería (E-5):** caída `-0.47 V`, recuperación ~35 s, veredicto `SinDegradacionDetectable` con confianza `baja` (calculado por el servicio, no por el equipo; el SAI no reporta veredicto).
- **Informe de período (CU-12, E-7):** para el año 2026, `diasConProteccion = 365`, `disponibilidadRespaldo = 1.0`, eventos `Microcorte: 31 · CorteSuministro: 2 · DesconexionUsb: 1 · TensionFueraDeRango: 6`, y costo de mantenimiento `67.000,00 ARS ≈ 52,80 USD` (equivalente marcado como derivado, cotización `BNA-divisa-venta`).

Ningún número está inventado: todos provienen de los escenarios `§20` del intake, y los valores `reconstruido` de las fixtures viajan marcados como tales.

## 7. Variaciones sugeridas

| Variación | Qué cambiar | Resultado esperado |
|---|---|---|
| Cambiar el escenario que arranca el panel | En `config/adaptador-simulado.json`, seleccionar el guion `E-3` (microcorte) o `E-4` (corte prolongado) | El panel refleja la transición `OL→OB→OL` y genera el evento correspondiente en vivo |
| Simular un corte prolongado con supuestos sin verificar | Dejar las 4 verificaciones en `NuncaVerificado` y reproducir `E-4` | La política dispara pero la acción queda `BloqueadaPorVerificacion`; la modalidad efectiva sigue en `SoloAlerta`. Es la garantía de seguridad central (RN-02) |
| Desbloquear el apagado | Marcar las 4 verificaciones como `Verificado` en el seed y reproducir `E-4` | La modalidad `HostLuegoUpsConRetorno` pasa a efectiva; la acción se ejecuta contra el adaptador simulado sin riesgo |
| Forzar pérdida de comunicación | En el guion del adaptador simulado, devolver 3 sondeos sin respuesta consecutivos | El panel levanta la alerta `DesconexionUsb` (RN de detección de pérdida de comunicación) |

## 8. Trazabilidad

| Artefacto upstream | Tipo | Cómo lo ilustra este sample |
|---|---|---|
| CU-02 (Alta de equipos y puesta en marcha) | Caso de uso | El seeder ejecuta el alta de catálogo, inventario y vínculos, y siembra las 4 verificaciones en `NuncaVerificado` |
| CU-04 (Monitoreo en vivo del estado del SAI) | Caso de uso | El panel muestra estado, conectividad, panel de supuestos y eventos recientes contra el adaptador simulado |
| CU-06 (Históricos y gráficas de evolución) | Caso de uso | Grafica `input.voltage` con agregados horarios y marcas de eventos |
| CU-12 (Informe de período y comparación de marcas) | Caso de uso | Muestra el informe de período con cobertura, eventos y costos en USD |
| ADR-02 (Adaptador de conexión con tres implementaciones) | Decisión arquitectónica | El sample corre íntegramente sobre la implementación **simulada**, sin NUT ni hardware |
| RN-01 / RN-02 (Arranque seguro en `SoloAlerta` y bloqueo por verificación) | Regla de negocio | El sistema arranca degradado y se niega a apagar mientras los supuestos no estén verificados |
| Escenarios §20 `E-1`…`E-8` | Fixture de datos | Es la fuente de todos los valores del panel, los históricos, la prueba y el informe |

## 9. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Versión inicial. Sample de datos seed con adaptador simulado para onboarding sin hardware. |
