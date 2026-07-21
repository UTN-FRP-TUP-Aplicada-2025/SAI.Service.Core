# CU-08 — Registro de recambio de batería y ficha de vida útil

**Proyecto:** Sai-Service-Core
**Documento:** CU-08-Recambio-De-Bateria-Y-Ficha-De-Vida-Util-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que el administrador registre el recambio de una batería con su costo, sus hallazgos y su destino final, de modo que un solo acto cierre la vigencia del montaje viejo, abra la del nuevo, cambie el estado de las dos unidades por baja lógica y alta en servicio, y proyecte la ficha de vida útil de la batería retirada, con su costo por año de servicio normalizado.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador | Primario | Registra la intervención de recambio con sus costos, hallazgos y disposición final |
| Servicio de intervenciones | Sistema | Aplica los efectos de la intervención: cierra y abre montajes, cambia estados y proyecta la ficha |
| Proveedor o técnico externo | Secundario | Ejecuta el recambio y retira la batería agotada como receptor de la disposición final |

## 3. Precondiciones

- El administrador tiene una sesión activa (CU-01).
- Existe un montaje de batería vigente en el dispositivo y la batería nueva está registrada o se registra en el mismo acto.
- Los costos de la intervención cuadran: el total iguala la suma de repuestos y mano de obra.

## 4. Flujo principal

1. El administrador registra la intervención de recambio: instante, dispositivo, baterías afectadas, proveedor y quién la ejecutó.
2. El administrador carga los costos con moneda y fecha, sus hallazgos y las mediciones de antes y después.
3. El sistema valida que los costos cuadren y que todo importe lleve moneda y fecha.
4. El sistema aplica los efectos: cierra el montaje vigente en el instante del recambio, abre el montaje de la batería nueva desde ese mismo instante y sin hueco.
5. El sistema cambia el estado de la batería retirada a dado de baja, con su fecha y motivo de baja, conservando toda su historia; y el de la batería nueva a en servicio.
6. El sistema registra la disposición final de la batería retirada, con su destino y receptor, para trazabilidad ambiental.
7. El sistema proyecta la ficha de vida útil de la batería retirada: días en servicio, si cumplió la expectativa, eventos soportados, tendencia de salud y costo por año de servicio normalizado.

## 5. Flujos alternativos

- FA-1 Carga diferida con dos tiempos. Disparador: el administrador carga la intervención días después, con la factura en la mano. El sistema conserva el tiempo en que ocurrió y el tiempo en que se registró; la diferencia es normal en carga manual.
- FA-2 Corrección de la fecha del recambio. Disparador: el administrador corrige la fecha de un recambio ya cargado. El sistema reatribuye automáticamente el histórico afectado sin migrar datos, porque la historia guarda dispositivo e instante y la batería se resuelve por el montaje (ver CU relacionado de corrección retroactiva en el modelo).

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| COSTOS_NO_CUADRAN | El total no iguala la suma de repuestos y mano de obra | Rechaza la intervención con el motivo de cuadre de costos y no aplica ningún efecto (RN-08) |
| DINERO_SIN_MONEDA_O_FECHA | Un importe llega sin moneda o sin fecha | Rechaza la intervención con el motivo de dinero incompleto (RN-07) |
| COHERENCIA_TEMPORAL | La intervención se fecha después de la baja de una unidad afectada | Rechaza la intervención por coherencia temporal; la unidad dada de baja se puede consultar pero no operar (RN-12) |

## 7. Postcondiciones

- Éxito: el montaje viejo está cerrado y el nuevo abierto sin hueco; la batería retirada está dada de baja pero consultable; la nueva en servicio; existe la disposición final y la ficha de vida útil proyectada.
- Fallo: si los costos no cuadran, falta moneda o fecha, o hay incoherencia temporal, no se aplica ningún efecto y el estado previo se conserva.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | El montaje `mnt-001` vigente con la batería `bat-2024-a` | El administrador registra el recambio por `bat-2026-a` el 2026-09-05 a las 10:30 | El sistema cierra `mnt-001` a ese instante y abre `mnt-002` desde el mismo instante, sin hueco |
| CA-02 | El recambio registrado | El sistema aplica los efectos | La batería `bat-2024-a` queda dada de baja con motivo fin de vida útil y sigue consultable con sus 8 pruebas (RN-12) |
| CA-03 | Una intervención con repuestos por 52.000 y mano de obra por 15.000, total declarado 60.000, en pesos | El administrador intenta guardar | El sistema la rechaza con COSTOS_NO_CUADRAN, porque el total debería ser 67.000 (RN-08) |
| CA-04 | Un importe total sin fecha ni moneda | El administrador intenta guardar | El sistema la rechaza con DINERO_SIN_MONEDA_O_FECHA (RN-07) |
| CA-05 | El recambio aplicado sobre `bat-2024-a` con 654 días en servicio | El sistema proyecta la ficha de vida útil | La ficha indica que no cumplió la expectativa de 3 a 5 años y calcula el costo por año de servicio normalizado a dólares |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-04 (Ciclo de vida de los equipos); NB-06 (ficha de vida útil e insumo de comparación) |
| Reglas de negocio aplicables | RN-07, RN-08, RN-12; RN-05 |
| Historias de usuario a generar | US-08 en 06 |
| Componentes esperados | Servicio de intervenciones con efectos, vínculos temporales, resolutor temporal, proyección de ficha (referencia tentativa a 05) |
| Tests previstos | Cierre y apertura de montaje sin hueco, baja lógica consultable, rechazo por cuadre de costos y por dinero incompleto, coherencia temporal (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancia de referencia: escenario §20.E-6 (recambio de batería con baja lógica, cierre de vigencia y ficha de vida útil cerrada).
- La fecha de fabricación anterior a la de compra no es un error: la edad real de la batería se cuenta desde la fabricación.
- El detalle del formulario de intervención es materia de 03-UX-UI-DX.

## 11. Interacción multiusuario y concurrencia

Trivial: un único administrador registra intervenciones. La ingesta externa que también crea intervenciones se trata en CU-11 con su propia regla de idempotencia.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-6 (§6), §20.E-6, NB-04 y NB-06 |
| 1.1 | 2026-07-20 | Retroalimentación de la Fase B2: unificación de terminología "parque" → "equipos" |
