# CU-11 — Ingesta automatizada de intervenciones

**Proyecto:** Sai-Service-Core
**Documento:** CU-11-Ingesta-Automatizada-De-Intervenciones-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que un sistema externo de gestión de mantenimiento empuje intervenciones por la interfaz de integración, sin intervención humana, con una clave de idempotencia, de modo que un reintento de red no duplique el hecho ni corrompa el histórico. Los datos ingresados quedan registrados con menor confianza que los medidos por el propio servicio.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Sistema externo de gestión de mantenimiento | Primario | Empuja una intervención con su clave de idempotencia y su fuente de datos |
| Servicio de ingesta | Sistema | Valida el cuerpo, aplica la idempotencia, registra la intervención y asigna la confianza |

## 3. Precondiciones

- La interfaz de integración de ingesta está disponible en la LAN.
- El sistema externo envía la clave de idempotencia y la identificación de su fuente de datos.
- La fuente de datos externa está registrada, con su confianza base.

## 4. Flujo principal

1. El sistema externo envía la intervención con su clave de idempotencia y su fuente de datos.
2. El servicio valida el cuerpo: que los costos cuadren, que todo importe lleve moneda y fecha, y que no haya incoherencia temporal con entidades dadas de baja.
3. El servicio comprueba la clave de idempotencia. Si es nueva, registra la intervención y devuelve el identificador creado, con confianza media por ser de origen externo sin verificación cruzada.
4. El servicio deja registrada la intervención con la marca de su fuente y su confianza, y con los dos tiempos: cuándo ocurrió y cuándo se registró.

## 5. Flujos alternativos

- FA-1 Reintento con la misma clave y el mismo cuerpo. Disparador: el emisor no recibió la respuesta y reintenta con la misma clave y el mismo cuerpo. El servicio no crea otro registro y devuelve el mismo identificador, indicando que no se creó de nuevo. Es el caso normal, no el excepcional.

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| CONFLICTO_IDEMPOTENCIA | La misma clave llega con un cuerpo distinto | Rechaza con conflicto de idempotencia, devuelve las huellas del cuerpo original y del recibido y una acción sugerida; nunca responde como si se hubiera aplicado (RN-09) |
| COSTOS_NO_CUADRAN | El total no iguala la suma de repuestos y mano de obra | Rechaza por validación indicando el campo y el invariante de cuadre de costos (RN-08) |
| DINERO_SIN_MONEDA_O_FECHA | Un importe llega sin moneda o sin fecha | Rechaza por validación indicando el campo y el invariante de dinero completo (RN-07) |
| COHERENCIA_TEMPORAL | La intervención referencia una entidad para operar sobre ella después de su baja | Rechaza por coherencia temporal; referenciar la entidad para consultar su historial sí es válido (RN-12) |

## 7. Postcondiciones

- Éxito con clave nueva: existe una intervención registrada con confianza media, su fuente y sus dos tiempos.
- Reintento idempotente: no se crea un registro nuevo; se devuelve el existente.
- Rechazo: no se registra nada; el emisor recibe el motivo preciso para corregir.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | Una intervención de inspección con clave `gmao-ext-ot-88213` y costo total de 12.000 pesos con fecha | El sistema externo la envía por primera vez | El servicio la registra, devuelve el identificador creado y le asigna confianza media por origen externo |
| CA-02 | La misma clave `gmao-ext-ot-88213` con el mismo cuerpo | El sistema externo reintenta el envío | El servicio no crea otro registro y devuelve el mismo identificador indicando que no se creó de nuevo (RN-09) |
| CA-03 | La misma clave `gmao-ext-ot-88213` con el monto cambiado de 12.000 a 19.500 | El sistema externo la envía | El servicio rechaza con conflicto de idempotencia y devuelve las huellas del cuerpo original y del recibido (RN-09) |
| CA-04 | Un cuerpo con repuestos por 52.000, mano de obra por 15.000 y total 60.000 | El sistema externo lo envía | El servicio rechaza por validación con el invariante de cuadre de costos (RN-08) |
| CA-05 | Una intervención fechada el 2026-11-01 sobre la batería `bat-2024-a`, dada de baja el 2026-09-05 | El sistema externo la envía | El servicio rechaza por coherencia temporal (RN-12) |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-08 (Ingesta automatizada de intervenciones) |
| Reglas de negocio aplicables | RN-07, RN-08, RN-09, RN-12 |
| Historias de usuario a generar | US-12 en 06 |
| Componentes esperados | Servicio de ingesta idempotente, fuentes de datos con confianza, validación de invariantes (referencia tentativa a 05); ejemplo en 11-Examples |
| Tests previstos | Los cuatro caminos de respuesta de la ingesta, idempotencia por clave, degradación de confianza, dos tiempos (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancia de referencia: escenario §20.E-8 (ingesta desde un servicio externo con reintento y casos de error).
- El endpoint de rectificación que sugiere la respuesta de conflicto está mencionado pero su contrato no está definido: es el pendiente P-05, que se cierra en esta categoría. Hasta definirlo, la corrección de un hecho ya ingresado se marca como pendiente y no se especifica aquí su flujo.
- El detalle del contrato de la interfaz (rutas, cabeceras, códigos) es superficie de integración documentada por 05-Arquitectura-Tecnica; aquí se define el qué y las respuestas de negocio.

## 11. Interacción multiusuario y concurrencia

El actor es un sistema, no un usuario humano. Puede reintentar; la idempotencia por clave es precisamente lo que hace segura la concurrencia de reintentos.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-10 (§6), §20.E-8, §17 P.3 y NB-08 |
