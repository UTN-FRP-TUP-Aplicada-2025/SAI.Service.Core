# Extensibilidad — Puerto del adaptador de conexión

**Proyecto:** Sai-Service-Core
**Documento:** Extensibilidad-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)

---

## 1. Punto de extensión

El único punto de extensión del sistema es el **puerto del adaptador de conexión con el equipo**: la abstracción que aísla al dominio del *cómo* se dialoga con el SAI. El puerto se declara en `SAI.Service.Core.Application`; sus implementaciones viven en `SAI.Service.Core.Infrastructure`. Esta separación es lo que hace **testeable el camino de apagado sin hardware ni riesgo** (la implementación simulada) y lo que permite cambiar de mecanismo de acceso al equipo sin tocar el dominio (T-07).

Gobierna **ADR-02** (adaptador de conexión con tres implementaciones). La **forma exacta del contrato** —las firmas de las cuatro operaciones— es una decisión abierta de Sprint 0, documentada como **ADR-22 en estado Propuesto**: la fuente declara las cuatro operaciones pero deja la firma sin cerrar.

## 2. Contrato mínimo del puerto

El puerto expone **cuatro operaciones** (§17 P.2, ADR-02). La firma es indicativa hasta que ADR-22 la fije; lo estable son las operaciones y su semántica:

| Operación | Semántica | Entrada | Salida | Confirmación |
| --- | --- | --- | --- | --- |
| **Leer estado** | Obtener la lectura del estado del equipo en un instante | — | Lectura cruda de variables (mapeada a `Muestra` con procedencia por valor) | La calidad (`completa`/`parcial`/`perdida`) refleja la respuesta real |
| **Probar conectividad** | Verificar que el equipo responde por el canal | — | Resultado de conectividad (usado por CU-02 en el alta y por la vigilancia de 3 sondeos fallidos) | — |
| **Ordenar apagado con retorno** | Ordenar al equipo cortar la salida de modo que produzca la transición que dispara el reencendido | Tiempo reservado, modalidad | **Retorno explícito** del resultado observado, no ausencia de error | Validación **por efecto observado** (ADR-11), nunca por ausencia de excepción |
| **Lanzar test de batería** | Iniciar el autotest con cadencia densa (1 Hz) y congelar el `montajeBateriaId` | — | Disparo del muestreo denso de la `PruebaBateria` | El equipo puede dejar de atender consultas mientras conmuta (CL-13) |

Nota de diseño impuesta por el dominio: la operación de apagado **retorna** el efecto observado porque un comando que no llega no produce error (CL-07); el contrato no admite «éxito por silencio».

## 3. Las tres implementaciones

| Implementación | Estado en la primera entrega | Qué hace | ADR / trazabilidad |
| --- | --- | --- | --- |
| **NUT** (`nutdrv_qx` + `upsd`) | Implementada | El adaptador es cliente de `upsd` (TCP local) o invoca `upsc`; NUT habla Megatec/Qx `Voltronic-QS-Hex 0.10` sobre USB con el equipo. Es la ruta de producción | ADR-01 (adopción de NUT), ADR-03 (anclaje USB) |
| **Directo + add-on de dialecto** | **Diseñada, no implementada** | Acceso directo al equipo sin NUT, para SAI que `nutdrv_qx` no cubra; apoyada en la capa de add-ons de dialecto | F-27 (Could Have); E-07 |
| **Simulada** | Implementada | Emula las cuatro operaciones sin hardware, para probar políticas y el camino de apagado sin riesgo y cubrir el flujo en pruebas automatizadas | F-24 (Should Have); habilita los Pipelines B y C del `Flujo-Ejecucion` en tests |

La implementación simulada es la que hace posible la estrategia de testing (§17 P.6): el nivel de integración prueba el adaptador NUT **contra el simulado**, y los end-to-end recorren el camino de apagado de CU-05/CU-10 sin cortar energía real.

## 4. Punto de extensión de add-ons de dialecto (diseñado, no implementado)

Debajo de la implementación **directa** hay una **capa de add-ons de dialecto de protocolo**: la variante concreta del protocolo que habla un equipo (subdriver). *"El firmware manda"*: dos equipos de la misma marca y modelo pueden hablar dialectos distintos. Esta capa queda **diseñada pero no implementada** en la primera entrega (E-07, F-26): su interfaz *"no tiene sentido especificarla antes de tener el servicio"*. Se incorporaría solo cuando aparezca un equipo que NUT no soporte, y únicamente sobre un SAI de banco con verdad de referencia instrumental. Al sustituir el SAI por otro modelo, el dialecto debe relevarse de nuevo y todas las verificaciones de firmware vuelven a `NuncaVerificado` (CL-27).

## 5. Ejemplo de registro

Registro del puerto en la composición de dependencias (indicativo; la firma se cierra en ADR-22). Se selecciona la implementación por configuración de entorno, de modo que producción use NUT y las pruebas usen la simulada sin recompilar el dominio:

```csharp
// SAI.Service.Core.Web — composición de la raíz de dependencias
// Selección por variable de entorno SAI_ADAPTADOR = "nut" | "simulado"
services.AddSingleton<IAdaptadorConexion>(sp =>
    config["SAI_ADAPTADOR"] switch
    {
        "simulado" => new AdaptadorSimulado(),                 // F-24: pruebas sin hardware
        _          => new AdaptadorNut(sp.GetRequiredService<OpcionesNut>()), // producción (ADR-01, ADR-03)
    });
// La implementación "directo + add-on de dialecto" (F-27) se registraría aquí
// cuando exista un equipo que nutdrv_qx no cubra (E-07, diseñada no implementada).
```

Un ejemplo ejecutable de extensión —el escenario que ejercita el adaptador— vive en **10-Examples** y se apoya en el escenario §20.E-07 del intake. El `samples/ingesta-gmao/` de §16.1 ejercita la otra superficie (la API), no el adaptador.

## 6. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| CU cubiertos | CU-04 (leer estado, conectividad), CU-05 (ordenar apagado con retorno), CU-07/CU-10 (lanzar test de batería) |
| RN aplicables | RN-03 (validación por efecto observado) |
| Capacidades | F-02 (NUT), F-24 (simulada), F-26 (add-ons diseñados), F-27 (directo) |
| ADR que lo gobiernan | ADR-02 (tres implementaciones); ADR-22 [Propuesto] (forma del contrato); relacionados ADR-01, ADR-03, ADR-11 |
| Ejemplo de extensión | 10-Examples (escenario §20.E-07) |
| Tests previstos en 08 | Camino de apagado contra el adaptador simulado; conmutación de implementación por configuración; efecto observado en la operación de apagado |

## Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Punto de extensión inicial: puerto del adaptador con contrato mínimo de cuatro operaciones, tres implementaciones (NUT, directo+add-on, simulada), capa de add-ons de dialecto diseñada, ejemplo de registro y trazabilidad a ADR-02/ADR-22. |
