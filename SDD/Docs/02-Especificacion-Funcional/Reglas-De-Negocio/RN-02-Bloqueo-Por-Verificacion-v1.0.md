# RN-02 — Bloqueo por verificación y degradación de modalidad

**Proyecto:** Sai-Service-Core
**Documento:** RN-02-Bloqueo-Por-Verificacion-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Si alguna verificación requerida por la versión de política vigente no está en estado verificado, es decir está sin verificar, vencida o refutada, la acción resulta bloqueada por verificación y la modalidad efectiva degrada a solo aviso.

## 2. Justificación

Es la garantía dura que separa una automatización confiable de una que puede fallar en silencio: el apagado no se habilita mientras un supuesto del que depende no esté probado por evidencia. Una verificación refutada bloquea de forma dura; una simplemente vencida solo pide repetir la prueba.

## 3. Ámbito de aplicación

- En cada evaluación de una acción de apagado por el planificador.
- Cada vez que una verificación cambia de estado o vence, lo que puede volver a bloquear una modalidad antes habilitada.

## 4. Consecuencia si se viola

Si el sistema ejecutara una acción con un supuesto requerido sin cumplir, podría dejar el host apagado indefinidamente. La regla obliga a que, ante cualquier supuesto no cumplido, la acción quede registrada como bloqueada por verificación con su motivo, sin apagar el host.

## 5. CU afectados

CU-05 (Ejecución del apagado ordenado ante corte sostenido), CU-03 (Configuración de políticas de apagado versionadas), CU-10 (Ventana de mantenimiento y verificación de supuestos), CU-04 (visualización del estado degradado en el panel).

## 6. Pruebas que la verifican

Prueba de que con al menos un supuesto requerido sin verificar la modalidad efectiva es solo aviso y el resultado es bloqueada por verificación; prueba de que un refutado bloquea de forma permanente y un vencido solo pide repetir. Referencia tentativa a 08-Calidad-Y-Pruebas (invariante I-11).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-11 y de SOLUTION-INTAKE §17 P.5 |
