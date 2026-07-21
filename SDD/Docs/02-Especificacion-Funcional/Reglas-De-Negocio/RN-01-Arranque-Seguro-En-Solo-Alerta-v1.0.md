# RN-01 — Arranque seguro en solo aviso

**Proyecto:** Sai-Service-Core
**Documento:** RN-01-Arranque-Seguro-En-Solo-Alerta-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

El sistema arranca, y permanece, en modalidad efectiva de solo aviso mientras no existan verificaciones que habiliten expresamente una modalidad con acción sobre el equipo o el host.

## 2. Justificación

El servicio decide apagar un servidor sin copias de respaldo, de noche y sin testigos; es el riesgo principal del proyecto. El arranque seguro no es una recomendación de puesta en marcha, sino un estado impuesto que evita apagar cuando todavía no se puede probar que el host va a volver a encenderse.

## 3. Ámbito de aplicación

- En el arranque del servicio y en cada puesta en marcha del parque.
- En toda ronda de evaluación de políticas del planificador, como estado base del que solo se sale por verificación cumplida.

## 4. Consecuencia si se viola

Si el sistema pudiera arrancar en una modalidad con acción sin verificaciones cumplidas, se reintroduciría el riesgo principal: un apagado no garantizado. La regla exige que cualquier arranque en una modalidad distinta de solo aviso, sin supuestos verificados, sea imposible por diseño.

## 5. CU afectados

CU-02 (Alta del parque y puesta en marcha), CU-05 (Ejecución del apagado ordenado ante corte sostenido), CU-10 (Ventana de mantenimiento y verificación de supuestos). De forma indirecta CU-01 (el acceso no altera la modalidad).

## 6. Pruebas que la verifican

Prueba de que, sin verificaciones cumplidas, la modalidad efectiva en el arranque y en cada ronda es solo aviso; prueba de que la puesta en marcha siembra las verificaciones en estado sin verificar y fuerza solo aviso. Referencia tentativa a 08-Calidad-Y-Pruebas.

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §17 P.5 (arranque seguro) y PA-10 |
