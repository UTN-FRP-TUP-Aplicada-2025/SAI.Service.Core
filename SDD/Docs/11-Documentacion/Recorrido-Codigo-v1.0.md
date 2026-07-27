---
doc_id: DOC-SAI-recorrido-codigo-01
doc_type: recorrido-codigo
title: Recorrido del código — SAI.Service.Core
status: Vigente
rol_intervencion: [mantenedor]
owner: Administrador único de SAI.Service.Core
version: 1.0
last_review: 2026-07-27
momento: 3
traces:
  - ADR-15
  - ADR-04
  - ADR-27
  - CU-05
---

# Recorrido del código — SAI.Service.Core

**Proyecto:** SAI.Service.Core
**Rol de intervención:** Mantenedor
**Nivel:** Medio
**Tiempo estimado de lectura:** 12 min

## Resumen ejecutivo

Este documento es el puente entre la arquitectura (categoría 05) y el árbol de archivos real. Un mantenedor que llega sin contexto encuentra acá dónde vive cada componente, cómo se recorre el flujo principal y dónde poner un archivo nuevo. No repite el porqué de las decisiones —eso está en los ADR—; responde el «dónde» y el «en qué orden». Toda ruta citada existe en el repositorio.

## 1. Mapa arquitectura → repositorio

La solución sigue Clean Architecture en cinco assemblies (ADR-15); las dependencias apuntan siempre al dominio. Cada capa es una carpeta bajo `src/SAI.Service.Core/`.

| Capa / componente | ADR | Ruta | Responsabilidad |
| --- | --- | --- | --- |
| Domain | ADR-15 | `src/SAI.Service.Core/SAI.Service.Core.Domain/` | Modelo framework-free; no referencia nada |
| — Inventario (Host/Dispositivo/Bateria, TPH) | ADR-07 | `.../Domain/Inventario/` | Unidades físicas y su baja lógica |
| — Vínculos temporales (Montaje, Cobertura, Vigencia) | ADR-05 | `.../Domain/Vinculos/` | Intervalos semiabiertos, resolutor temporal |
| — Monitoreo (Muestra, Evento, Agregado, reglas) | ADR-08, ADR-12 | `.../Domain/Monitoreo/` | Series, derivación de eventos, salud |
| — Verificaciones (los 4 supuestos, evaluador) | ADR-10 | `.../Domain/Verificaciones/` | Bloqueo por verificación, modalidad efectiva |
| — Acciones (apagado) | RN-04 | `.../Domain/Acciones/` | `Accion` append-only, techo duro 540 s |
| — Intervenciones (recambio, sustitución, ingesta, fichas) | ADR-04 | `.../Domain/Intervenciones/` | Historia física append-only |
| — Políticas (versión de política) | CU-03 | `.../Domain/Politicas/` | Política de apagado versionada |
| — Valores / Historia | ADR-06, ADR-04 | `.../Domain/Valores/`, `.../Domain/Historia/` | `Valor<T>`/`Dinero` con procedencia; marcador append-only |
| Application (casos de uso + puertos) | ADR-15 | `.../SAI.Service.Core.Application/` | Orquesta el dominio; define interfaces |
| — Puertos del adaptador | ADR-27 | `.../Application/Abstractions/` | `IAdaptadorConexion`, `IDescubridorSai` |
| — Servicios de caso de uso | — | `.../Application/{Equipos,Monitoreo,Acciones,Intervenciones,Politicas,Informes,Ingesta}/` | Un subdirectorio por flujo |
| Infrastructure | ADR-18 | `.../SAI.Service.Core.Infrastructure/` | EF Core + SQLite, adaptadores, hosted services |
| — Persistencia (DbContext, repos, migraciones) | ADR-18, ADR-04 | `.../Infrastructure/Persistencia/` | `SaiDbContext`, `InterceptorAppendOnly`, `Configuraciones/`, `Migraciones/` |
| — Adaptadores de conexión | ADR-02, ADR-27 | `.../Infrastructure/Adaptadores/` | `AdaptadorConexionSimulado.cs`, `Nut/` |
| — Hosted services | — | `.../Infrastructure/Monitoreo/` | `ServicioSondeo`, `ServicioRearmePruebas` |
| — Cableado DI | — | `.../Infrastructure/DependencyInjection.cs` | Registro de repos, adaptadores, opciones |
| Api (endpoints REST) | ADR-17, ADR-28 | `.../SAI.Service.Core.Api/Endpoints/` | `EndpointsSalud.cs`, `EndpointsApiV1.cs` |
| Web (composition root + host) | ADR-16, ADR-29 | `.../SAI.Service.Core.Web/` | `Program.cs`, `Components/Pages/`, `Endpoints/`, `Autenticacion/` |

## 2. Árbol comentado

```text
src/SAI.Service.Core/
├── SAI.Service.Core.Domain/        # núcleo; sin dependencias
├── SAI.Service.Core.Application/   # casos de uso (subdir por flujo) + Abstractions/ (puertos)
├── SAI.Service.Core.Infrastructure/
│   ├── Persistencia/               # SaiDbContext, repos, Configuraciones/, Migraciones/
│   ├── Adaptadores/                # Simulado + Nut/ (cliente y adaptador NUT)
│   ├── Monitoreo/                  # ServicioSondeo, ServicioRearmePruebas (hosted)
│   └── DependencyInjection.cs      # AddInfrastructure(): todo el cableado
├── SAI.Service.Core.Api/Endpoints/ # /api/v1: salud, token, ingesta
└── SAI.Service.Core.Web/
    ├── Program.cs                  # composition root: DI, pipeline, seed al arranque
    ├── Components/Pages/           # paneles Blazor (un .razor por superficie)
    ├── Endpoints/                  # EndpointsAcceso (login, token ROPC)
    └── Autenticacion/              # proveedor de estado, generador de tokens
tests/                              # Domain.Tests, Application.Tests, Integration.Tests
```

## 3. Recorrido del flujo principal (disparo del apagado)

Sea un corte sostenido con el sistema verificado. El punto de entrada es el sondeo de fondo:

1. `Infrastructure/Monitoreo/ServicioSondeo.cs` — a cada intervalo abre un scope de DI, lee el estado por `IAdaptadorConexion` y persiste una `Muestra` por el repositorio de monitoreo.
2. `Application/Monitoreo/ServicioMonitoreo.cs` — tras cada muestra, evalúa la ventana reciente con `Domain/Monitoreo/DerivadorEventos.cs` contra las reglas versionadas; al superarse el umbral de tiempo en batería con la tensión en rango, deriva un evento `DisparoApagado` (ADR-12: nunca por el flag de batería baja).
3. `Application/Acciones/ServicioApagadoOrdenado.cs` — recibe el disparo. Lee la política vigente por `IRepositorioPoliticas`, deriva la **modalidad efectiva** con `Domain/Verificaciones/EvaluadorModalidad.cs` (degrada a solo aviso si los cuatro supuestos no están verificados y vigentes) y, si habilita una acción, ordena el apagado por `IAdaptadorConexion.OrdenarApagadoConRetornoAsync`.
4. `Infrastructure/Adaptadores/Nut/AdaptadorConexionNut.cs` (o `AdaptadorConexionSimulado.cs` en dev) — ejecuta la orden contra NUT y devuelve un resultado.
5. De vuelta en el servicio, la `Domain/Acciones/Accion.cs` se registra **por efecto observado** y se persiste append-only. El panel `Web/Components/Pages/PanelDeApagado.razor` la muestra en el historial.

## 4. Dónde vive cada cosa

| Pregunta | Respuesta (ruta) |
| --- | --- |
| ¿Dónde se define el esquema de datos? | `Infrastructure/Persistencia/Configuraciones/Modelo*.cs` + `SaiDbContext.cs` |
| ¿Dónde se aplica el append-only? | `Infrastructure/Persistencia/InterceptorAppendOnly.cs` (marcador `Domain/Historia/IEntidadHistoria`) |
| ¿Dónde se cablea la DI y se eligen los adaptadores? | `Infrastructure/DependencyInjection.cs` (`Sai:Adaptador`) |
| ¿Dónde se valida la entrada de la API de ingesta? | `Application/Ingesta/ServicioIngesta.cs`; el endpoint en `Api/Endpoints/EndpointsApiV1.cs` |
| ¿Dónde está el bloqueo por verificación? | `Domain/Verificaciones/EvaluadorModalidad.cs` |
| ¿Dónde se siembra el estado inicial? | `Web/Program.cs` (bloque de seed tras `MigrateAsync`) |
| ¿Dónde se agregan páginas del panel? | `Web/Components/Pages/*.razor` |

## 5. Convenciones estructurales

Un flujo nuevo se agrega como un subdirectorio en `Application/<Flujo>/` con su servicio de caso de uso y sus puertos (interfaces), la implementación de los puertos en `Infrastructure/` (repositorio EF + configuración de modelo + migración), y la superficie en `Api/Endpoints/` o `Web/Components/Pages/`. El dominio nuevo va en `Domain/<Area>/` y no referencia ninguna capa externa. Una entidad de historia implementa `IEntidadHistoria` para quedar protegida por el interceptor append-only. La regla es que la dirección de las dependencias nunca se invierte: si un archivo de `Domain` necesita algo de `Infrastructure`, el diseño está mal y se resuelve con un puerto en `Application/Abstractions/`.

## 6. Preguntas guía

- ¿La funcionalidad nueva es un caso de uso? Entonces tiene un servicio en `Application/` y, casi siempre, un puerto nuevo.
- ¿Persiste historia? Entonces su entidad implementa `IEntidadHistoria` y tiene su `Modelo*.Configurar` + migración.
- ¿Toca el SAI? Entonces pasa por `IAdaptadorConexion`, nunca por NUT directamente. Ver [Guia-Extension](Guia-Extension-v1.0.md).
