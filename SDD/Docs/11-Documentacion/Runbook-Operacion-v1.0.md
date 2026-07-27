---
doc_id: DOC-SAI-runbook-01
doc_type: runbook
title: Runbook de operación — SAI.Service.Core
status: Vigente
rol_intervencion: [operador]
owner: Administrador único de SAI.Service.Core
version: 1.0
last_review: 2026-07-27
momento: 3
traces:
  - CU-05
  - CU-10
  - ADR-25
  - ADR-29
---

# Runbook de operación — SAI.Service.Core

**Proyecto:** SAI.Service.Core
**Rol de intervención:** Operador
**Nivel:** Medio
**Tiempo estimado de lectura:** 10 min

## Resumen ejecutivo

Procedimientos para operar el servicio en ejecución: arrancar, parar, verificar salud, leer logs y resolver los incidentes conocidos. El servicio es un solo proceso; su salud se resume en tres señales: `GET /health` responde, el sondeo escribe muestras, y el adaptador NUT alcanza al SAI. Los incidentes más caros de este dominio no son del servicio sino de su entorno NUT, y están catalogados abajo como `OPS-XX`.

## 1. Procedimientos base

| Acción | Comando | Salida esperada |
| --- | --- | --- |
| Arrancar (dev) | `./scripts/run.sh SAI.Service.Core.Web` | Log de arranque; migraciones aplicadas; escucha en `:8080` |
| Arrancar (prod publicado) | `dotnet ./.publish/SAI.Service.Core.Web.dll` | Ídem, con `ASPNETCORE_ENVIRONMENT=Production` |
| Parar | `Ctrl-C` en primer plano, o `SIGTERM` al proceso | Cierre ordenado; el hosted service de sondeo se detiene |
| Reiniciar | Parar y arrancar de nuevo | El `ServicioRearmePruebas` rearma en `StartAsync` las pruebas pendientes |
| Verificar salud | `curl -sf http://localhost:8080/health` | exit `0`, `{"estado":"ok",...}` |

El reinicio no es inocuo respecto de las verificaciones: por ADR-25, un reinicio real del host es la señal que el `ServicioRearmePruebas` interpreta para rearmar las pruebas de apagado pendientes. Reiniciar el proceso sin que el host haya ciclado también dispara ese rearme.

## 2. Logs

Los logs van a la salida estándar del proceso (consola en dev; capturados por el gestor de servicio en prod). Nivel por defecto `Information`; `Microsoft.AspNetCore` en `Warning` (`appsettings.json`).

| Clase de problema | Patrón a buscar | Comando de filtrado |
| --- | --- | --- |
| Apagado ordenado y su resultado | `Accion` / `EfectoNoConfirmado` / `Bloqueada` | `journalctl -u sai -f \| grep -iE "accion\|apagado"` |
| Fallo de conexión al SAI | `no alcanzable` / excepción de transporte NUT | `... \| grep -iE "nut\|alcanzable"` |
| Migraciones al arranque | `Applying migration` | `... \| grep -i migration` |

El mensaje de error hacia el operador se redacta en lenguaje llano; el detalle técnico de NUT (`shutdown.return`, claves de config) va solo al log, no a la UI.

## 3. Métricas y umbrales

| Métrica | Qué mide | Atención | Alarma | Acción |
| --- | --- | --- | --- | --- |
| Antigüedad del último sondeo | Frescura del estado en vivo | > 15 s | > 3 sondeos perdidos | Ver OPS-01 (SAI no alcanzable) |
| Supuestos verificados | Habilitación del apagado | < 4 | 0 y política ≠ solo aviso | Correr ventana de mantenimiento (CU-10) |
| Días sin protección | Cobertura del host | > 0 | cobertura vigente ausente | Revisar sustitución del SAI (CU-09) |
| Estado del apagado | Última `Accion` | `EfectoNoConfirmado` | repetido | Ver OPS-02 |

## 4. Incidentes conocidos

### OPS-01 — El panel muestra el SAI «no alcanzable»

- **Síntoma:** el estado en vivo no refresca; el indicador marca no alcanzable; el sondeo registra muestras de calidad perdida.
- **Diagnóstico:** `upsc sai@localhost ups.status` — si falla la conexión, el problema es NUT, no el servicio. Confirmar que `upsd` corre (`systemctl status nut-server`) y que el nombre del UPS coincide con `Sai__Nut__Ups`.
- **Resolución:** reiniciar `upsd`/driver de NUT; verificar la ruta física del USB (ADR-03). El servicio se recupera solo en la siguiente ronda de sondeo cuando NUT vuelve.

### OPS-02 — El apagado queda en «efecto no confirmado»

- **Síntoma:** una `Accion` con estado `EfectoNoConfirmado` en el historial del panel de apagado.
- **Diagnóstico:** el adaptador ordenó el apagado pero el equipo no admitió la orden. Casi siempre es falta de credenciales de escritura: revisar `Sai__Nut__Usuario`/`Sai__Nut__Password` y que ese usuario sea `upsmon master` con permiso `shutdown.return` en `upsd.users`.
- **Resolución:** completar las credenciales NUT de escritura y reintentar desde el panel. Con las credenciales vacías, el servicio queda en solo lectura por diseño (no apaga a ciegas).

### OPS-03 — Tras reiniciar el contenedor se pierden las sesiones y fallan los formularios

- **Síntoma:** todos los usuarios quedan deslogueados tras un reinicio; los formularios devuelven HTTP 400 antiforgery.
- **Diagnóstico:** el keyring de DataProtection era efímero (no persistido). Verificar que `DataProtection__RutaLlaves` apunta a un volumen montado y escribible.
- **Resolución:** montar el volumen del keyring (ADR-29) y reiniciar. Con el keyring persistido, sesión y antiforgery sobreviven a reinicios y redeploys.

### OPS-04 — upsmon no dispara el apagado del host ante un corte

- **Síntoma:** ante un corte prolongado el host no se apaga aunque el servicio registró el disparo.
- **Diagnóstico:** el disparo de apagado del host lo ejecuta la cadena NUT (`upsmon`/`upssched`), no este servicio. Revisar que `upsmon.conf` tenga la línea `MONITOR sai@localhost 1 <usuario> <clave> master` y que el usuario sea `upsmon master` en `upsd.users` (un `slave` da `ERR ACCESS-DENIED`).
- **Resolución:** corregir `upsmon.conf`/`upsd.users`, recargar NUT. Detalle en el `Installer-Guide` del entorno real. Evidencia de un ciclo limpio: `last -x` muestra «shutdown» (no «crash») para el apagado atribuible al corte gestionado.

## 5. Escalamiento

Al ser un servicio de administrador único, el escalamiento es hacia la documentación, no hacia un equipo. Si un incidente no está en esta lista ni se resuelve con el diagnóstico correspondiente: registrar la eventualidad en [Bitacora-Eventualidades](Bitacora-Eventualidades-v1.0.md) con su síntoma, causa e intentos descartados, y —si es un patrón nuevo del entorno NUT— sumarla como `OPS-XX` nuevo en este runbook. Dejar de intentar el apagado real y volver el servicio a solo aviso es siempre una posición segura: el sistema arranca así por diseño.
