# CU-06 — Históricos y gráficas de evolución del suministro

**Proyecto:** Sai-Service-Core
**Documento:** CU-06-Historicos-Y-Graficas-De-Evolucion-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que el administrador consulte y grafique la evolución de las variables del equipo (tensiones, carga, microcortes) en un período, de forma individual o superpuesta, con las marcas de eventos encima, para evaluar la calidad del suministro durante la vida del host. Distingue siempre una serie de muestras a resolución completa de una serie de agregados, y esta última viaja con su cobertura y su advertencia.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador | Primario | Elige el período y las variables, y lee las gráficas con las marcas de eventos |
| Servicio de consulta histórica | Sistema | Recupera muestras, agregados y eventos del período y arma la serie, declarando su procedencia |

## 3. Precondiciones

- El administrador tiene una sesión activa (CU-01).
- Existe historia registrada: muestras a resolución completa dentro de la ventana de retención, o agregados horarios fuera de ella, y eventos.

## 4. Flujo principal

1. El administrador elige un período y una o varias variables a graficar.
2. El servicio determina, para el período pedido, si la serie sale de muestras a resolución completa (dentro de la ventana de retención) o de agregados horarios (fuera de ella).
3. El servicio recupera los eventos del período para superponerlos como marcas.
4. Si la serie proviene de agregados, el servicio incluye la cobertura de cada ventana y la advertencia de que un agregado no representa microcortes, y toma el conteo de microcortes de los eventos, nunca del promedio.
5. El servicio devuelve la serie con la procedencia de cada valor y el panel la muestra, con las variables superpuestas si el administrador lo pidió y las marcas de eventos encima.

## 5. Flujos alternativos

- FA-1 Serie sobre agregados. Disparador: el período pedido cae fuera de la ventana de retención de muestras. El servicio arma la serie con agregados horarios, conservando mínimo y máximo además del promedio para la tensión de entrada, y adjunta la advertencia y la cobertura. Retorna al paso 5.
- FA-2 Variables superpuestas. Disparador: el administrador pide superponer tensiones y carga. El servicio devuelve las series alineadas en el tiempo con sus respectivas escalas; el detalle visual queda para 03.

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| PERIODO_SIN_DATOS | El período pedido no tiene ninguna muestra, agregado ni evento | Informa que no hay datos para el período y no dibuja una serie vacía como si fuera real |
| AGREGADO_SIN_COBERTURA | Se intentaría servir una serie de agregados sin su cobertura o su advertencia | El sistema no sirve la serie sin esos campos; la cobertura y la advertencia son obligatorias (RN-10) |

## 7. Postcondiciones

- Éxito: el administrador ve la serie del período con las marcas de eventos; si la serie es de agregados, aparece con su cobertura y su advertencia.
- Fallo: ante un período sin datos, no se presenta una serie engañosa.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | Un período dentro de los últimos 30 días con muestras a resolución completa | El administrador grafica la tensión de entrada | El sistema muestra la serie de muestras con las marcas de eventos del período |
| CA-02 | Un período de más de 30 días atrás, servido por agregados horarios con cobertura 0,987 | El administrador grafica la tensión de entrada | La serie aparece con la cobertura 0,987 y la advertencia de que el agregado no representa microcortes (RN-10) |
| CA-03 | Un período con 31 microcortes registrados como eventos | El administrador consulta la cantidad de microcortes | El conteo proviene de los eventos, no del promedio de la serie agregada |
| CA-04 | Un período sin ninguna muestra, agregado ni evento | El administrador intenta graficar | El sistema responde PERIODO_SIN_DATOS y no dibuja una serie vacía |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-03 (Historia trazable con procedencia); toca NB-02 (calidad de suministro) |
| Reglas de negocio aplicables | RN-05, RN-10 |
| Historias de usuario a generar | US-06 en 06 |
| Componentes esperados | Servicio de consulta histórica, agregación y retención, panel de gráficas (referencia tentativa a 05) |
| Tests previstos | Serie de muestras vs serie de agregados, obligatoriedad de cobertura y advertencia, conteo de microcortes desde eventos (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancias de referencia: escenarios §20.E-2 (agregado resultante con cobertura 0,997) y §20.E-7 (consulta de período con calidad de suministro sobre agregados).
- El promedio horario oculta los microcortes; por eso la agregación conserva mínimo y máximo, y el conteo de microcortes sale siempre de los eventos.
- El diseño visual de las gráficas y la superposición corresponde a 03-UX-UI-DX.

## 11. Interacción multiusuario y concurrencia

Trivial: un único administrador consulta; las lecturas no compiten con otros usuarios.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-4 (§6), §20.E-2, §20.E-7 y NB-03 |
