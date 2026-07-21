# RN-09 — Idempotencia de la ingesta externa

**Proyecto:** Sai-Service-Core
**Documento:** RN-09-Idempotencia-De-La-Ingesta-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Reenviar una clave de idempotencia ya procesada con el mismo cuerpo devuelve el registro existente sin crear otro; con un cuerpo distinto, la operación se rechaza por conflicto y nunca se responde como si se hubiera aplicado.

## 2. Justificación

El reintento de red es el caso normal, no el excepcional. Sin idempotencia, la captura automatizada duplica registros y corrompe los costos agregados. Y responder como aplicado ante un cuerpo distinto sería peor que duplicar: el emisor creería que su corrección se aplicó cuando no fue así.

## 3. Ámbito de aplicación

- En toda ingesta de intervenciones desde un sistema externo por la interfaz de integración.

## 4. Consecuencia si se viola

Una clave repetida con el mismo cuerpo que crea un segundo registro, o una clave repetida con cuerpo distinto que responde como aplicada, viola la regla. La regla obliga a devolver el mismo registro en el primer caso y a rechazar por conflicto, con las huellas de ambos cuerpos, en el segundo.

## 5. CU afectados

CU-11 (Ingesta automatizada de intervenciones).

## 6. Pruebas que la verifican

Prueba de que la misma clave con el mismo cuerpo no crea un segundo registro y devuelve el existente; prueba de que la misma clave con cuerpo distinto se rechaza por conflicto con las dos huellas. Referencia tentativa a 08-Calidad-Y-Pruebas (invariante I-19).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-19, SOLUTION-INTAKE §7 (CL-21) y §20.E-8 |
