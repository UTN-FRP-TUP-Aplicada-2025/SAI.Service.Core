# RN-03 — Validación por efecto observado

**Proyecto:** Sai-Service-Core
**Documento:** RN-03-Validacion-Por-Efecto-Observado-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Ninguna acción sobre el equipo o el host se considera ejecutada por la ausencia de error; se considera ejecutada solo cuando su efecto se observa y se confirma.

## 2. Justificación

Durante el relevamiento, un comando que nunca llegó al equipo no produjo ningún mensaje de error. Un servicio que asuma que la ausencia de excepción equivale a ejecución va a mentir sobre lo que hizo, justo en el camino con consecuencias irreversibles.

## 3. Ámbito de aplicación

- En toda orden que el sistema emite hacia el equipo (corte con retorno, autotest) o hacia el host (apagado ordenado).
- En la actualización del resultado de una acción y de las verificaciones que se apoyan en efecto observado.

## 4. Consecuencia si se viola

Si una acción se diera por ejecutada sin observar su efecto, el sistema podría reportar como apagado un host que sigue encendido, o como cortado un equipo que no cortó. La regla obliga a registrar el efecto no confirmado y a no avanzar como si la acción hubiera surtido efecto.

## 5. CU afectados

CU-05 (Ejecución del apagado ordenado ante corte sostenido), CU-10 (Ventana de mantenimiento y verificación de supuestos), CU-07 (Prueba de batería, disparo del autotest).

## 6. Pruebas que la verifican

Prueba con el adaptador simulado en la que una orden no produce efecto y no genera error: la acción no debe quedar como ejecutada, sino como efecto no confirmado. Referencia tentativa a 08-Calidad-Y-Pruebas.

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §7 (CL-07), PA-11 y §17 P.5 |
