# NB-08 — Ingesta automatizada de intervenciones desde un sistema externo

| Campo | Valor |
| --- | --- |
| Proyecto | Sai-Service-Core |
| Documento | NB-08-Ingesta-Automatizada-De-Intervenciones-v1.0.md |
| Versión | 1.3 |
| Estado | Borrador |
| Fecha | 2026-07-20 |
| Autor | Orquestador SDD (AG-01) |
| Trazabilidad upstream | SOLUTION-INTAKE §1, §4 (F-20), §7 (CL-20, CL-21, CL-22), §11; Vision-Producto-v1.0.md §2, §3; Alcance-Proyecto-v1.0.md §4 (C-12), §8 |
| Trazabilidad downstream | CU-11, CU-08 (02-Especificacion-Funcional) |

## 1. Descripción de la necesidad

El negocio necesita que un sistema externo de gestión de mantenimiento pueda empujar intervenciones —recambios, reparaciones, inspecciones y sus costos— sin intervención humana, para que esa información no dependa de que alguien la cargue a mano. Hoy toda intervención se registra manualmente; abrir una vía automatizada permite que la historia se alimente sola desde donde ya se gestiona el mantenimiento.

La necesidad tiene una condición dura: el reintento es el caso normal, no el excepcional. Una red que falla y un emisor que reintenta no deben duplicar el hecho ni corromper el histórico. Por eso la ingesta debe ser idempotente: el mismo hecho enviado dos veces se registra una sola vez, y un reenvío con un cuerpo distinto bajo la misma clave debe rechazarse en lugar de aceptarse en silencio, porque aceptarlo sería peor que duplicar: el emisor creería que su corrección se aplicó. Además, un dato ingresado por esta vía lleva menor confianza que el medido localmente, y una intervención con costos que no cuadran o fechada después de la baja de la unidad debe rechazarse, porque es el invariante que la ingesta externa rompe primero.

Importa porque sin estas garantías la vía automatizada, en vez de ayudar, sería la principal fuente de corrupción silenciosa del histórico de costos.

## 2. Ejemplo de uso desde la perspectiva del negocio

El sistema de gestión de mantenimiento de un tercero registra un recambio de batería con su costo y, sin que nadie intervenga, lo empuja al servicio con una clave que identifica ese hecho. La red se cae en medio del envío y el sistema reintenta: el servicio reconoce la clave y no duplica la intervención, devolviendo el mismo identificador. Más tarde alguien corrige el costo y reenvía con la misma clave pero un cuerpo distinto; el servicio lo rechaza y señala el conflicto, en vez de aceptar la corrección como si fuera un hecho nuevo. El dato ingresado queda registrado con menor confianza que el que mide el servicio localmente.

## 3. Impacto

- Alimentación automática del histórico: la información de mantenimiento entra sin depender de la carga manual.
- Integridad ante reintentos: la idempotencia evita duplicar hechos cuando la red falla.
- Protección del histórico de costos: el rechazo de costos que no cuadran evita que los agregados queden mal en silencio.
- Confianza diferenciada por origen: el dato externo se distingue del medido localmente por su menor confianza.
- Si queda sin resolver: la información externa sigue cargándose a mano, o una vía automatizada sin garantías corrompe el histórico ante el primer reintento.

## 4. Problema específico que resuelve

- Que un sistema externo empuje intervenciones sin intervención humana.
- Que el mismo hecho enviado dos veces con la misma clave se registre una sola vez.
- Que un reenvío con la misma clave y un cuerpo distinto se rechace, en vez de aceptarse en silencio.
- Que una intervención con costos que no cuadran o fechada después de la baja de la unidad se rechace.
- Que el dato ingresado por esta vía quede con menor confianza que el medido localmente.

## 5. Criterios de éxito

| Criterio | Métrica | Target | Plazo |
| --- | --- | --- | --- |
| Idempotencia ante reintento | Hechos duplicados ante reenvío con la misma clave y el mismo cuerpo | 0 | Continuo |
| Rechazo de conflicto de clave | Reenvíos con la misma clave y cuerpo distinto aceptados como hecho nuevo | 0 (se rechazan por conflicto) | Continuo |
| Rechazo de costos que no cuadran | Intervenciones con costos que no cuadran aceptadas | 0 | Continuo |
| Coherencia temporal | Intervenciones fechadas después de la baja de la unidad aceptadas | 0 | Continuo |
| Confianza del origen declarada | Datos ingeridos sin fuente ni confianza declarada | 0 (confianza media, menor que la del dato medido) | Continuo |

## 6. Stakeholders involucrados

| Rol | Nivel | Qué pide o aporta |
| --- | --- | --- |
| Administrador único (rol propietario) | Propietario | Aprueba abrir la vía de ingesta y exige que ningún reintento duplique ni corrompa el histórico |
| Administrador único (rol implementador) | Implementador | Construye la ingesta idempotente, la validación de costos y coherencia, y la confianza por origen |
| Sistema externo de gestión de mantenimiento (fd-gmao-externo) | Integrador / consumidor | Empuja intervenciones automatizadas con una clave de idempotencia y espera respuestas coherentes ante el reintento |

## 7. Trazabilidad a CU

| NB | CU prevista | Estado |
| --- | --- | --- |
| NB-08 | CU-11 Ingesta automatizada de intervenciones | aprobada |
| NB-08 | CU-08 Registro de recambio de batería y ficha de vida útil | aprobada |

## 8. Dependencias con otras NB

- Depende de NB-03 (Historia trazable con procedencia): las intervenciones ingeridas se registran como hechos append-only con su origen y confianza.
- Depende de NB-04 (Ciclo de vida de los equipos): una intervención se aplica sobre unidades del inventario y sus vínculos temporales, cerrando y abriendo vigencias.

## 9. Prioridad MoSCoW

Must Have. El SOLUTION-INTAKE §4 marca F-20 (API REST de ingesta idempotente) como Must Have de la primera entrega, dentro del conjunto F-01 a F-20 sin el cual el servicio no cumple sus propósitos; sus garantías de idempotencia e integridad son las que evitan que la vía externa corrompa el histórico de costos en silencio.

## 10. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §1, §4, §7 y §11, y de Vision-Producto-v1.0.md |
| 1.1 | 2026-07-20 | Corrección de prioridad MoSCoW a Must Have por alineación con SOLUTION-INTAKE §4 tras audit de Fase A |
| 1.2 | 2026-07-20 | Reconciliación de trazabilidad §7 con los CU vigentes de 02 tras audit de Fase B |
| 1.3 | 2026-07-20 | Retroalimentación de la Fase B2: unificación de terminología 'parque' → 'equipos' |
