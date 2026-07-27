---
doc_id: DOC-SAI-extension-01
doc_type: extension
title: Guía de extensión — SAI.Service.Core
status: Vigente
rol_intervencion: [mantenedor]
owner: Administrador único de SAI.Service.Core
version: 1.0
last_review: 2026-07-27
momento: 3
traces:
  - ADR-02
  - ADR-27
  - VER-01
---

# Guía de extensión — SAI.Service.Core

**Proyecto:** SAI.Service.Core
**Rol de intervención:** Mantenedor
**Nivel:** Avanzado
**Tiempo estimado de lectura:** 6 min

## Resumen ejecutivo

El único punto de extensión publicado del sistema es el **adaptador de conexión al SAI**: la frontera que aísla todo el acceso al equipo detrás de dos interfaces (`IAdaptadorConexion` e `IDescubridorSai`, ADR-27). Agregar un backend nuevo —otro protocolo, otro simulador, un equipo de otra marca— es implementar esas interfaces y registrarlas; nada del resto del sistema cambia. Es lo que permite que el desarrollo corra con un simulado y la producción con NUT sin tocar los casos de uso.

## EXT-01 — Adaptador de conexión al SAI

**Qué permite extender.** Cómo el servicio lee el estado del SAI, prueba la conectividad, ordena el apagado con retorno y lanza el test de batería, y cómo descubre dispositivos. Todo el dominio y los casos de uso dependen de las interfaces, no de una implementación concreta.

**Contrato.** Dos puertos en `Application/Abstractions/`:

```csharp
// IAdaptadorConexion.cs — operación (contrato cerrado, ADR-27)
Task<EstadoSai> LeerEstadoAsync(CancellationToken ct);
Task<ResultadoConectividad> ProbarConectividadAsync(CancellationToken ct);
Task<ResultadoAccion> OrdenarApagadoConRetornoAsync(TimeSpan retardo, CancellationToken ct);
Task<ResultadoAccion> LanzarTestBateriaAsync(CancellationToken ct);

// IDescubridorSai.cs — descubrimiento
Task<IReadOnlyList<DispositivoDescubierto>> DescubrirAsync(CancellationToken ct);
```

Las implementaciones vigentes están en `Infrastructure/Adaptadores/`: `AdaptadorConexionSimulado.cs` (dev, sin hardware) y `Nut/AdaptadorConexionNut.cs` (producción, habla con `upsd`). Ambas implementan las dos interfaces y se resuelven a la misma instancia.

**Ejemplo de registro.** La selección vive en `Infrastructure/DependencyInjection.cs`, gobernada por `Sai:Adaptador`:

```csharp
var usarNut = string.Equals(configuration["Sai:Adaptador"], "Nut", StringComparison.OrdinalIgnoreCase);
if (usarNut)
{
    services.AddSingleton<IClienteNut>(new ClienteNut(opcionesNut));
    services.AddSingleton<AdaptadorConexionNut>();
    services.AddSingleton<IAdaptadorConexion>(sp => sp.GetRequiredService<AdaptadorConexionNut>());
    services.AddSingleton<IDescubridorSai>(sp => sp.GetRequiredService<AdaptadorConexionNut>());
}
else { /* AdaptadorConexionSimulado, ídem para ambos puertos */ }
```

Para agregar un backend `MiAdaptador`: implementá las dos interfaces en `Infrastructure/Adaptadores/`, agregá una rama al selector por un valor nuevo de `Sai:Adaptador` (p. ej. `"MiBackend"`) y registrá la misma instancia para los dos puertos. Ningún caso de uso ni entidad de dominio se toca.

**Límites de la extensión.** El adaptador **confirma por efecto observado** (ADR-11): `OrdenarApagadoConRetornoAsync` devuelve `Aceptada` solo si el equipo admitió la orden, nunca por ausencia de excepción; una excepción de transporte se traduce a «no alcanzable», no a «apagado exitoso». El adaptador **no decide** la modalidad ni el bloqueo por verificación: eso es del dominio (`EvaluadorModalidad`). Y **nunca** emite un `shutdown.stop`: el ciclo forzado no se cancela (ADR-09). Un adaptador que viole cualquiera de estas tres reglas rompe la seguridad operativa del sistema.

**Sample que lo demuestra.** El [Ejemplo-01 de 10-Examples](../10-Examples/Ejemplo-01-Datos-Seed-v1.0.md) corre el sistema completo sobre el adaptador simulado (contrato `VER-01`), que es la prueba viva de que la frontera funciona: el panel muestra estado en vivo sin ningún equipo conectado.
