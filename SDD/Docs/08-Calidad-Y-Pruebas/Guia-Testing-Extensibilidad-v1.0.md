# Guía de testing de extensibilidad — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Guia-Testing-Extensibilidad-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

---

## 1. Por qué aplica esta guía

Sai-Service-Core es `web-monolith` y, por regla (§2.2 de `08-Rules-Calidad-Y-Pruebas.md`), este tipo no lleva guía de extensibilidad **salvo que tenga un motor de extensión interno**. Sai-Service-Core lo tiene: el **puerto del adaptador de conexión con el equipo**, el único punto de extensión del sistema (`Extensibilidad-v1.0.md` §1, ADR-02, ADR-22). Esta guía documenta cómo probar ese puerto y sus implementaciones sin tocar el núcleo, cómo se agrega y testea una implementación nueva, y cómo el adaptador simulado habilita probar el camino de apagado sin hardware ni riesgo.

El puerto se declara en `SAI.Service.Core.Application`; sus implementaciones viven en `SAI.Service.Core.Infrastructure`. Esa separación es exactamente lo que hace testeable el camino de apagado irreversible sin cortar energía real (T-07, T-08).

---

## 2. El contrato a testear

El puerto expone **cuatro operaciones** (§17.P.2 del intake, ADR-02). La firma exacta es una decisión abierta de Sprint 0 (ADR-22, Propuesto); lo estable —y lo que la suite de contrato fija— es la semántica de cada operación:

| Operación | Semántica a verificar | Confirmación exigida |
| --- | --- | --- |
| Leer estado | Devuelve la lectura de variables mapeada a `Muestra` con procedencia por valor | La calidad (`completa`/`parcial`/`perdida`) refleja la respuesta real del canal |
| Probar conectividad | Verifica que el equipo responde por el canal | Resultado usado por CU-02 (alta) y por la vigilancia de 3 sondeos fallidos (N-09) |
| Ordenar apagado con retorno | Ordena cortar la salida de modo que produzca la transición que dispara el reencendido | **Retorno explícito del efecto observado**, no ausencia de error (ADR-11, RN-03): el contrato no admite «éxito por silencio» |
| Lanzar test de batería | Inicia el autotest con cadencia densa (1 Hz) y congela el `montajeBateriaId` | Dispara el muestreo denso de la `PruebaBateria` (N-08); el equipo puede dejar de atender consultas al conmutar (CL-13) |

Regla de diseño impuesta por el dominio, que la suite de contrato debe hacer cumplir: la operación de apagado **retorna** el efecto observado, porque un comando que no llega no produce error (CL-07).

---

## 3. Cómo testear el puerto sin tocar el núcleo

El principio es que el dominio (`SAI.Service.Core.Domain`) no referencia EF Core, Blazor ni NUT; toda la lógica de decisión de apagado se prueba puramente. El puerto se inyecta por configuración (`SAI_ADAPTADOR = "nut" | "simulado"`, `Extensibilidad-v1.0.md` §5), de modo que producción use NUT y las pruebas usen la implementación simulada sin recompilar el dominio.

Niveles de prueba del punto de extensión:

1. **Contract test del puerto (`TC-40`).** Una única suite de contrato, parametrizada por implementación, que ejerce las cuatro operaciones y verifica la semántica de la tabla §2 contra **cada** implementación de `IAdaptadorConexion`. La misma suite debe pasar contra NUT y contra el simulado. Es la garantía de que agregar una implementación nueva no cambia el contrato que el dominio espera. Tipo: contract. Tooling: xUnit + una clase base de contrato compartida (`AdaptadorConexionContractTests<TAdaptador>`).

2. **Integración del adaptador NUT contra el simulado.** El nivel de integración (§17.P.6) prueba el adaptador NUT contrastando su comportamiento con el del simulado sobre las mismas fixtures de `§20.E-2..E-5`, sin hardware. Tooling: xUnit + WebApplicationFactory.

3. **Camino de apagado end-to-end contra el simulado (`TC-26`, `TC-27`, `TC-39`).** Los recorridos de CU-05/CU-10 se ejecutan contra la implementación simulada, que emula el corte de `§20.E-4` sin cortar energía real.

Ninguno de estos tests referencia código de las implementaciones concretas más allá del puerto: se escriben contra `IAdaptadorConexion`, no contra `AdaptadorNut` ni `AdaptadorSimulado`.

---

## 4. El adaptador simulado como doble de prueba

El adaptador simulado (`AdaptadorSimulado`, F-24, implementado en la primera entrega) es el doble de prueba que hace posible toda la estrategia de testing del camino crítico (§17.P.6). Permite probar políticas y el camino de apagado sin hardware ni riesgo, y cubre en pruebas automatizadas la parte lógica de un flujo que, real, cortaría la energía del host.

Qué debe emular el simulado para servir como doble:

- **Reproducir un corte** con la serie de muestras de `§20.E-4` (transición OL→OB a las 04:15:00, `input.voltage 0,0 V`, descenso de `battery.voltage` de 12,91 a 12,46 V sostenido 370 s, retorno de red a las 04:21:10). Con esto `TC-26` verifica la degradación a `SoloAlerta` con supuestos sin verificar.
- **Retornar el efecto observado** de la operación de apagado de forma programable: `EFECTO_CONFIRMADO` o `EFECTO_NO_CONFIRMADO`, para que `TC-27` verifique que el sistema no reporta como ejecutado lo no observado (RN-03).
- **Emular la pérdida de muestras en la conmutación** (calidad `perdida`, valores `null`) como en `§20.E-5`, para `TC-17` y `TC-28`.
- **Disparar el muestreo denso a 1 Hz** al lanzar el test de batería y restaurar la cadencia normal al terminar (N-08).

El simulado no reemplaza la verificación física: el flujo F-3 (ciclo completo de apagado y reencendido) no es automatizable (T-08). El simulado cubre la lógica; el comportamiento real del firmware se registra como evidencia de una `Verificacion` en la ventana de mantenimiento (CU-10), no como test. Este límite se declara explícitamente en la Matriz-Sensado-Deriva y en los gaps de la matriz de cobertura.

---

## 5. Cómo se agrega y testea una implementación nueva

La primera entrega trae NUT y el simulado implementados; la implementación **directa + add-on de dialecto** está diseñada pero no implementada (F-27, E-07). El procedimiento para incorporarla —o cualquier otra implementación del puerto— sin tocar el núcleo:

1. **Escribir la implementación en `SAI.Service.Core.Infrastructure`** contra la interfaz `IAdaptadorConexion` declarada en `Application`. El dominio no se toca.
2. **Registrarla en la composición de dependencias** (`SAI.Service.Core.Web`) bajo una nueva clave de `SAI_ADAPTADOR`, seleccionable por variable de entorno. Producción sigue usando NUT; la nueva implementación se activa por configuración sin recompilar el dominio.
3. **Ejecutar la suite de contrato (`TC-40`) contra la nueva implementación.** Basta con instanciar `AdaptadorConexionContractTests<TAdaptador>` con la nueva clase: si la implementación respeta la semántica de §2, la suite pasa sin modificaciones. Este es el criterio de aceptación de una implementación nueva.
4. **Agregar tests específicos del transporte** de la nueva implementación (por ejemplo, el manejo del dialecto concreto), que no viven en la suite de contrato compartida sino en su propio archivo.

### Add-ons de dialecto de protocolo (diseñados, no implementados)

Debajo de la implementación directa hay una **capa de add-ons de dialecto** (subdrivers): la variante concreta del protocolo que habla un equipo. Queda diseñada pero no implementada en la primera entrega (E-07, F-26): su interfaz «no tiene sentido especificarla antes de tener el servicio». Se incorporaría solo cuando aparezca un equipo que NUT no soporte, y únicamente sobre un SAI de banco con verdad de referencia instrumental.

Consideración de testing propia del dominio: al sustituir el SAI por otro modelo, **el dialecto debe relevarse de nuevo y todas las verificaciones de firmware vuelven a `NuncaVerificado`** (CL-27). El caso está cubierto por `TC-35` (sustitución del SAI reinicia las verificaciones por cambio de modelo). El testing de un add-on de dialecto nuevo, cuando exista, seguirá el mismo procedimiento de los cuatro pasos, con su propia suite de contrato de dialecto.

---

## 6. Contract tests del puerto — checklist

La suite de contrato (`TC-40`) verifica, para cada implementación:

- [ ] **Leer estado** devuelve una `Muestra` con procedencia por valor y la calidad correcta (`completa` con respuesta plena; `parcial` si falta una variable como `ups.load`; `perdida` con valores `null` si el equipo no responde).
- [ ] **Probar conectividad** devuelve un resultado de conectividad utilizable por el alta (CU-02) y por el contador de 3 sondeos fallidos (N-09).
- [ ] **Ordenar apagado con retorno** devuelve el efecto observado explícito; nunca reporta éxito por ausencia de excepción; ante orden no confirmada devuelve `EFECTO_NO_CONFIRMADO` y el sistema mantiene el estado seguro.
- [ ] **Lanzar test de batería** dispara el muestreo denso a 1 Hz, congela el `montajeBateriaId` (I-15) y restaura la cadencia normal al terminar.
- [ ] La misma suite pasa sin cambios contra `AdaptadorNut` y contra `AdaptadorSimulado`.

---

## 7. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Punto de extensión | Puerto del adaptador de conexión (`IAdaptadorConexion`), `Extensibilidad-v1.0.md` |
| ADR que lo gobiernan | ADR-02 (tres implementaciones); ADR-22 [Propuesto] (forma del contrato); relacionados ADR-01, ADR-03, ADR-11 |
| CU cubiertos | CU-04 (leer estado, conectividad), CU-05 (apagado con retorno), CU-07/CU-10 (test de batería) |
| RN aplicables | RN-03 (validación por efecto observado) |
| TC asociados | TC-40 (contrato del puerto); TC-26, TC-27, TC-39 (camino de apagado contra el simulado); TC-17, TC-28 (muestra perdida, test de batería); TC-35 (sustitución reinicia verificaciones) |
| Ejemplo de extensión | 11-Examples (escenario §20.E-07); `samples/ingesta-gmao/` ejercita la API, no el adaptador |
| Límite declarado | F-3 (ciclo físico de apagado/reencendido) no automatizable; se cubre con el simulado para la lógica y con evidencia de la ventana de mantenimiento (T-08) |

---

## 8. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Guía inicial de testing del motor de extensión interno (puerto del adaptador de conexión): contrato de cuatro operaciones, tres niveles de prueba, el adaptador simulado como doble de prueba del camino de apagado, procedimiento de cuatro pasos para incorporar y testear una implementación nueva (add-on de dialecto diseñado no implementado) y checklist de contract tests. Referencia ADR-02, ADR-22 y el ejemplo previsto en 11. |
