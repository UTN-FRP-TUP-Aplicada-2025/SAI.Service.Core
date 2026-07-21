# Compatibilidad de Plataformas

**Proyecto:** Sai-Service-Core
**Documento:** Compatibilidad-Plataformas-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-00)
**Trazabilidad upstream:** SOLUTION-INTAKE §10, §17 P.1, §17 P.9
**Trazabilidad downstream:** 09-Devops, 05-Arquitectura-Tecnica, 08-Calidad-Y-Pruebas

## 1. Resumen ejecutivo

El servicio es exclusivamente Linux, empaquetado como contenedor, y corre en el mismo servidor que protege. El target es único y deliberadamente estrecho: administra un SAI conectado a un servidor Linux concreto, así que la portabilidad a otras plataformas no tendría consumidor y se renuncia a ella a cambio de un solo target que probar, un solo pipeline y un solo conjunto de scripts. El panel se consulta desde la red local con navegadores basados en Chromium o con Firefox. El acceso al equipo se hace a través del ecosistema de herramientas libres ya verificado sobre el equipo relevado. Este documento fija las versiones mínimas soportadas y declara, de forma explícita, que toda combinación no listada se considera no soportada.

## 2. Matriz de compatibilidad

| Componente | Plataforma soportada | Versión mínima | Notas |
|---|---|---|---|
| Sistema operativo del servidor | Linux x86-64 | Kernel 6.1 | El servidor relevado corre 6.12.95+deb13-amd64 |
| Runtime de ejecución | .NET | 10.0 | Sin retroceso a versiones anteriores |
| Empaquetado y ejecución | Docker Engine | 24.0 | Contenedor de producción runtime-only, distinto del contenedor de desarrollo |
| Arquitectura de CPU | linux/amd64 | — | No se soporta linux/arm64 |
| Navegador del panel (Chromium) | Chromium / Google Chrome | 120 | Mínimo estable para el transporte en tiempo real que usa el panel |
| Navegador del panel (Firefox) | Mozilla Firefox | 121 | Mínimo estable para el transporte en tiempo real que usa el panel |
| Acceso al SAI | NUT (Network UPS Tools) | 2.8.0 | La versión relevada en el equipo es 2.8.1 |
| Equipo SAI | Cualquiera soportado por el subdriver nutdrv_qx | — | Verificado sobre el dispositivo 0665:5161, dialecto Voltronic-QS-Hex 0.10 |

Toda combinación no listada se considera no soportada.

## 3. Restricciones de plataforma justificadas

- Exclusivamente Linux x86-64: el sistema administra un SAI conectado por USB a un servidor Linux concreto; el destino es ese servidor y no otro. Es una decisión de alcance del intake §10, no una limitación técnica coyuntural.
- Contenedor sobre el host: simplifica el despliegue y el respaldo a un único artefacto; el dispositivo USB se comparte al contenedor por ruta física de puerto mediante una regla del sistema, para que un reemplazo de equipo en el mismo puerto no rompa el vínculo.
- .NET 10 como piso, sin retroceso: fijado por el entorno de construcción; no hay compatibilidad hacia versiones anteriores del runtime.
- Navegadores basados en Chromium 120 y Firefox 121 como mínimos: son las primeras versiones que sostienen de forma estable el transporte en tiempo real con el que el panel empuja el estado del SAI sin sondeo desde el navegador.
- Acceso al equipo a través de NUT 2.8.0 o superior con el subdriver nutdrv_qx: el driver ya conoce las trampas del dialecto del equipo; construir el diálogo a mano reintroduciría riesgos ya resueltos.
- Sin exposición a internet: el panel y la interfaz de integración se sirven solo en la red local.

## 4. Alternativas para plataformas no soportadas

- Windows o macOS como host de producción: no soportado y sin alternativa prevista; la portabilidad no tendría consumidor.
- Arquitectura linux/arm64: no soportada; el único host de destino es x86-64.
- Navegador Safari: no soportado; se recomienda usar un navegador basado en Chromium o Firefox en su versión mínima o superior.
- Acceso al panel desde fuera de la red local: no soportado; el acceso remoto queda a cargo de mecanismos de red del propio administrador (por ejemplo, red privada), fuera del alcance del servicio.
- Equipos SAI no cubiertos por nutdrv_qx: no soportados en la primera entrega; la capa de extensiones de dialecto de protocolo queda diseñada pero no implementada (exclusión E-07 del intake) y solo se abordaría sobre un equipo de banco con verdad de referencia instrumental.

## 5. Estado de implementación por plataforma

| Plataforma o componente | Estado | Observación |
|---|---|---|
| Linux x86-64, kernel 6.1 o superior | Verificado en relevamiento | Host de producción 6.12.95+deb13-amd64 (2026-07-19) |
| .NET 10 | Objetivo de construcción | Piso fijado; sin build validado aún porque la construcción no arrancó |
| Docker Engine 24.0 | Objetivo de despliegue | Imagen de producción a construir en la fase de andamiaje |
| Chromium 120 / Firefox 121 | Objetivo de soporte | A verificar contra el panel cuando exista |
| NUT 2.8.0 / nutdrv_qx | Verificado en relevamiento | Versión 2.8.1 relevada; dialecto Voltronic-QS-Hex 0.10 confirmado |
| Dispositivo SAI 0665:5161 | Verificado en relevamiento | Sin iSerial; se ancla por ruta física de puerto |
| Ubicación de NUT (dentro del contenedor o en el host) | Decisión abierta | Pendiente P-03, se cierra como ADR en el arranque técnico |

## 6. Trazabilidad downstream

Este documento alimenta a 09-Devops, que materializa la matriz de sistema operativo, runtime y contenedores en la configuración del pipeline y del despliegue (un solo eje: ubuntu con .NET 10 y contenedor Linux amd64), y define cómo se comparte el dispositivo USB al contenedor por ruta física de puerto. Alimenta también a 05-Arquitectura-Tecnica, donde la decisión abierta sobre la ubicación de NUT (pendiente P-03) se cierra como ADR del arranque, junto con la del cifrado del acceso en la red local (pendiente P-04). El componente de navegadores condiciona las verificaciones de extremo a extremo de 08-Calidad-Y-Pruebas.
