# US-04 — Alta de catálogo e inventario con vínculos temporales y baja lógica

**Proyecto:** Sai-Service-Core
**Documento:** US-04-Alta-De-Catalogo-E-Inventario-Con-Vinculos-Temporales-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-04 Alta de equipos y políticas de apagado
**Prioridad MoSCoW:** Must
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador, quiero dar de alta el catálogo (fabricante, modelo) y el inventario (host, SAI, batería) abriendo los vínculos temporales de montaje y cobertura, para que cada métrica futura se pueda atribuir al período de la batería y del equipo que estaba montado.

## 2. Contexto
NB-04 modela el ciclo de vida con vínculos temporales de intervalo (ADR-05), separación de catálogo, inventario e historia (ADR-07) y baja lógica sin borrado físico. CU-02 abre el `MontajeBateria` y la `CoberturaHost` con `hasta = null` y crea la sesión de sondeo. Todo dato declarado lleva procedencia (RN-05): la potencia nominal desconocida queda `null` con procedencia `imputado`, nunca un número inventado.

## 3. Criterios de aceptación
- Given un dispositivo ya probado (US-03), When el administrador declara fabricante, modelo, potencia e inventario, Then el sistema crea el catálogo e inventario y abre los vínculos `MontajeBateria` y `CoberturaHost` con fin abierto.
- Given una potencia nominal desconocida, When se confirma el alta, Then el sistema guarda el valor como `null` con procedencia `imputado` y no bloquea el alta.
- Given una batería sin número de serie o con `fechaFabricacion` anterior a `fechaCompra`, When se confirma el alta, Then el sistema lo acepta (son casos normales) y cuenta la edad desde la fecha de fabricación.
- Given una entidad dada de baja lógicamente, When se intenta registrar una operación fechada después de la baja, Then el sistema la rechaza por coherencia temporal (RN-12) y conserva el historial consultable.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-04 |
| CU cubiertos | CU-02 |
| BT derivadas | BT-07, BT-11, BT-12 |
| Tests previstos | acceptance/AT-04-alta-catalogo-inventario |

## 5. Prioridad y estimación
Must: el inventario con vínculos temporales es la base que permite atribuir métricas y reconstruir cobertura. 8 SP por el modelo de cuatro capas y el resolutor temporal; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-02
- [x] NB de origen (NB-04) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-07, BT-11 y BT-12, planificadas antes

## 7. Notas y supuestos
El borrado físico no existe en este dominio (CL-20): siempre baja lógica con estado, fecha y motivo. Múltiples SAI simultáneos quedan fuera de alcance (F-29, Won't v1) aunque el modelo los contemple.
