# Maqueta de validación visual — Sai-Service-Core

Maqueta navegable **estática** de la Fase B2 del framework SDD. No es el producto:
es una **línea de base de validación** que materializa la especificación UX/UI ya
redactada (categoría 03 del proyecto) para que un humano la valide en el navegador.

- Tecnología: HTML5 semántico + Bootstrap 5 (por CDN) + CSS/JS vanilla. **Sin build,
  sin `node_modules`, sin backend.** Lo que se edita es lo que se sirve.
- Modelo UX-UI aplicado: **Catálogo base + Blazor-MudBlazor + 4 extensiones** (aproximado
  con Bootstrap 5; no usa MudBlazor real).
- Iteración: **2026-07-20**.

## Cómo se abre

Cualquiera de estas formas sirve los mismos archivos:

1. **Servidor estático de línea de comandos** (recomendado; habilita la recarga automática):
   ```bash
   cd SDD/Maquetas/Sai-Service-Core
   python3 -m http.server 8080
   ```
   y abrir `http://localhost:8080`.
2. **Servidor liviano del editor** (Live Server de VS Code o equivalente): recarga sola
   en cada guardado. Es el más cómodo si vas a corregir a mano.
3. **Abrir el archivo directamente**: `index.html` en el navegador. Sirve para una mirada
   rápida, pero sobre `file://` la recarga automática de la barra de validación queda
   **deshabilitada con su razón visible** (algunos navegadores restringen operaciones en `file://`).

El punto de entrada es `index.html`: lista las 11 superficies y el contrato de campos que
la maqueta exhibe.

## Barra de validación (no forma parte del producto)

Cada superficie tiene abajo una barra rotulada **«Barra de validación de maqueta — no forma
parte del producto»**. Permite:

- **Alternar los estados** de la superficie en curso sin recargar (selector «Estado»).
- **Recarga automática**: interruptor apagado por defecto, estado persistido en el navegador
  (`localStorage`), intervalo de ~3 s, suspendida cuando la pestaña no está visible y
  deshabilitada con su razón sobre `file://`. Detecta cambios por `ETag`/`Last-Modified`
  (HEAD), no por descarga completa.

Nada de la barra se traslada al producto: es un instrumento de la maqueta.

## Superficies cubiertas (11) y CU que materializan

| Superficie (archivo) | CU origen | Qué materializa |
| --- | --- | --- |
| `Alta-Inicial-Administrador.html` | CU-01 (alta inicial) | Aprovisionamiento del primer arranque, sin chrome ni cancelar (extensión Primer-Arranque). |
| `Acceso-Login.html` | CU-01 (ingreso) | Acceso del operador único; sin registro/selector/recuperación; catálogo de códigos de resultado (extensión Acceso-Monousuario); sello de versión. |
| `Panel-Estado-En-Vivo.html` | CU-04 + CU-05 | Estado del SAI, batería (carga derivada), conectividad, supuestos, eventos, banner de bloqueo, orientación posterior; sello de versión. |
| `Alta-De-Equipos.html` | CU-02 | Descubrimiento USB, datos declarados del SAI/batería/host, siembra de verificaciones. |
| `Configuracion-De-Politicas.html` | CU-03 | Configuración dirigida por esquema: descriptores, presets, simulación, «en palabras», ranura del asistente deshabilitada, previsualización de propuesta (extensión Config-Esquema). |
| `Prueba-De-Bateria.html` | CU-07 | Prueba densa a 1 Hz, veredicto con confianza y reserva, historial con comparabilidad. |
| `Historicos-Y-Graficas.html` | CU-06 | Evolución por período; distinción muestras/agregados con cobertura y advertencia. |
| `Panel-De-Verificaciones.html` | CU-10 + CU-05 | Estado de los 4 supuestos y ventana de mantenimiento (stepper) por efecto observado. |
| `Registro-De-Intervenciones.html` | CU-08 | Intervención con costos y cuadre, disposición final, ficha de vida útil, fuente local/externa. |
| `Sustitucion-Del-SAI.html` | CU-09 | Cobertura vigente, sucesión de coberturas, días sin protección, aviso de caracterización (datos R-11 reconstruidos). |
| `Informe-De-Periodo.html` | CU-12 | Informe por período y comparación de marcas por costo por año normalizado a USD. |

## Estructura de archivos

```
Sai-Service-Core/
├── index.html                     # punto de entrada (catálogo + contrato de campos)
├── <11 superficies>.html          # una por wireframe, mismo nombre canónico
├── assets/css/Estilos-Maqueta.css # tokens del catálogo como variables CSS
├── assets/js/Datos-Maqueta.js     # FUENTE ÚNICA de datos (§20.E-1..E-8), contrato, descriptores
├── assets/js/Maqueta.js           # render, navegación, conmutación de estados, barra de validación
└── README.md
```

**Ningún HTML hardcodea datos.** Todo se renderiza desde `Datos-Maqueta.js`. Los HTML de
superficie son cascarones mínimos con un atributo `data-superficie`; `Maqueta.js` construye
el shell, el contenido, el pie con sello de versión y la barra de validación.

## Cómo se corrige a mano

Dos vías, las dos soportadas por el orquestador:

1. **Por prompt**: describí el cambio y se aplica sobre los archivos.
2. **A mano**:
   - Datos de ejemplo, contrato de campos o descriptores → `assets/js/Datos-Maqueta.js`.
   - Estructura, lógica de estados o navegación → `assets/js/Maqueta.js` (funciones en el
     objeto `V` por superficie; estados declarados en `D.superficies`).
   - Tokens/estilos → `assets/css/Estilos-Maqueta.css` (variables CSS; sin literales sueltos).
   - Cascarón/CDN de una superficie → su `.html`.

   Guardá y refrescá (o activá «Recarga automática»). Cuando termines, avisale al orquestador
   **«revisá la maqueta y tomá las correcciones»**: relee los archivos, interpreta las
   diferencias como decisiones de diseño y las confirma con vos antes de propagarlas a la
   documentación de 03.

## Fuente de los datos y datos reconstruidos

Los datos salen **verbatim** de `SOLUTION-INTAKE-Sai-Service-Core-v1.0.md`, Parte D §20,
escenarios E-1..E-8. No se inventan datos. Donde una superficie necesita un campo que la
documentación no ejemplifica, se marca **«sin dato de ejemplo»** o **«reconstruido para la
maqueta»** de forma visible:

- **Sustitución del SAI**: el flujo UF-7 no tiene escenario de datos completo (riesgo R-11 del
  intake). La sucesión de coberturas de sustitución está marcada como *reconstruida para la
  maqueta* y no proviene de §20.
- **Sello de versión**: la cadena `versionLegible` y el `identificadorDeConstruccion` no están
  ejemplificados en la documentación; van marcados como *sin dato de ejemplo* en el detalle de
  diagnóstico. `esPreliminar` = verdadero (artefacto `-alpha.N`).

## Accesibilidad (WCAG 2.2 AA, piso)

Landmarks (`header`/`nav`/`main`/`footer`), `<h1>` por vista, enlace de salto al contenido,
`label` asociados a los controles, foco visible en todos los controles, recorrido por teclado,
`aria-live` para el anuncio de cambios de estado, `role="alert"`/`role="status"` en las bandas,
badges con texto además de color, contraste de texto ≥ 4.5:1, `prefers-reduced-motion` respetado.
