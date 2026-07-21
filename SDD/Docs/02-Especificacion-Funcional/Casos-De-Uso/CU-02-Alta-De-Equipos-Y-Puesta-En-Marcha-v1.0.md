# CU-02 — Alta de equipos y puesta en marcha

**Proyecto:** Sai-Service-Core
**Documento:** CU-02-Alta-De-Equipos-Y-Puesta-En-Marcha-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que el administrador, la primera vez que abre el panel, dé de alta el equipo de alimentación, la batería que tiene montada y el host que protege, descubriendo el dispositivo por su conexión física, sin editar archivos de configuración a mano. Deja el sistema listo para empezar a registrar historia, con los vínculos temporales abiertos y las verificaciones sembradas en estado sin verificar, lo que fuerza el modo de solo aviso.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador | Primario | Descubre el dispositivo, declara catálogo e inventario y confirma el alta |
| Adaptador de conexión con el equipo | Sistema | Identifica el dispositivo, informa sus descriptores y prueba la conectividad |
| Host protegido | Secundario | Es la unidad que queda cubierta por el equipo dado de alta |

## 3. Precondiciones

- El administrador tiene una sesión activa (CU-01).
- No existe todavía un dispositivo activo dado de alta, o se está incorporando uno nuevo al conjunto de equipos vacío.
- El equipo de alimentación está conectado físicamente al host.

## 4. Flujo principal

1. El administrador inicia el alta de equipos desde el panel.
2. El adaptador de conexión lista los candidatos conectados con sus descriptores y devuelve el identificador del dispositivo, por ejemplo `vendorId 0665`, `productId 5161`, descriptor de fabricante `INNO TECH`, sin número de serie.
3. El administrador declara a mano el fabricante, el modelo y la topología del equipo, que quedan con procedencia declarado; si la potencia nominal se desconoce, queda sin valor con procedencia imputado, nunca un número inventado.
4. El administrador da de alta el catálogo necesario: fabricante, modelo de dispositivo y modelo de batería, este último con su vida de flotación esperada acompañada de la temperatura de referencia.
5. El administrador da de alta el inventario: el host, el dispositivo y la batería, cada uno con su estado inicial.
6. El sistema abre el vínculo de montaje de la batería en el dispositivo y el vínculo de cobertura del host por el dispositivo, ambos con fin abierto (vigentes).
7. El adaptador de conexión prueba la conectividad y el sistema crea la sesión de sondeo con su mapa de variable a origen, registrando driver, versión y dialecto.
8. El sistema siembra las cuatro verificaciones de supuestos en estado sin verificar, lo que fuerza la modalidad de solo aviso, y muestra en el panel el aviso permanente de operativo con cero de cuatro supuestos verificados, con acceso a la ventana de mantenimiento (CU-10).

## 5. Flujos alternativos

- FA-1 Potencia nominal desconocida. Disparador: en el paso 3 el administrador no conoce la potencia nominal. El sistema la registra sin valor con procedencia imputado y continúa; no se bloquea el alta.
- FA-2 Batería sin número de serie ni fecha de fabricación. Disparador: en el paso 5 la batería no expone número de serie ni fecha de fabricación. El sistema acepta ambos campos vacíos; la edad real queda no calculable con su motivo y la edad se contará desde la fecha de compra. No es un error.
- FA-3 Unidad de repuesto en stock. Disparador: el administrador da de alta un segundo dispositivo de repuesto. El sistema lo registra en estado en stock, sin conexión y sin cobertura, para que la sucesión de coberturas del host sea representable más adelante (CU-09).

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| DISPOSITIVO_NO_DESCUBIERTO | El adaptador no encuentra ningún candidato conectado | Informa la ausencia de candidatos y no permite continuar el alta del dispositivo hasta resolver la conexión |
| VIDA_FLOTACION_SIN_TEMPERATURA | Se declara la vida de flotación esperada del modelo de batería sin temperatura de referencia | Rechaza el alta del modelo con el motivo del invariante de vida de flotación (RN-13) |
| PRUEBA_CONEXION_FALLIDA | La prueba de conectividad del paso 7 no obtiene respuesta del equipo | Registra el fallo, deja el dispositivo dado de alta pero marca la conectividad como no confirmada y alerta en el panel |

## 7. Postcondiciones

- Éxito: existen el catálogo, el inventario y los vínculos temporales abiertos; hay una sesión de sondeo activa; las cuatro verificaciones están en estado sin verificar y la modalidad efectiva es solo aviso.
- Fallo: si el alta no se confirma, no se crean entidades a medias; los equipos quedan como estaban.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | Un dispositivo descubierto con descriptor `0665:5161 INNO TECH` sin número de serie | El administrador confirma el alta del dispositivo sin número de serie | El sistema registra el dispositivo con número de serie vacío, sin rechazarlo por ese campo |
| CA-02 | Un modelo de batería `12V 9Ah AGM` cuya vida de flotación se declara como 3 a 5 años sin temperatura de referencia | El administrador intenta guardar el modelo | El sistema rechaza el alta con VIDA_FLOTACION_SIN_TEMPERATURA (RN-13) |
| CA-03 | El host `i7infra` y el dispositivo `ups-01` recién dados de alta | El administrador confirma la puesta en marcha | El sistema abre el montaje `mnt-001` y la cobertura `cob-001` con fin abierto y crea la sesión de sondeo |
| CA-04 | El alta de equipos recién completada | El sistema termina la puesta en marcha | Las cuatro verificaciones quedan en estado sin verificar y el panel muestra operativo con 0 de 4 supuestos verificados y modalidad solo aviso |
| CA-05 | Una potencia nominal desconocida al declarar el modelo | El administrador continúa el alta | El sistema registra la potencia sin valor con procedencia imputado y no bloquea la puesta en marcha |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-04 (Ciclo de vida de los equipos); toca NB-02 (sondeo), NB-03 (procedencia) y NB-05 (siembra de verificaciones) |
| Reglas de negocio aplicables | RN-01, RN-05, RN-13; RN-06 de forma indirecta |
| Historias de usuario a generar | US-01, US-05 (base) en 06 |
| Componentes esperados | Adaptador de conexión, gestión de catálogo, inventario y vínculos temporales, resolutor temporal (referencia tentativa a 05) |
| Tests previstos | Dispositivo sin número de serie válido, rechazo de vida de flotación sin temperatura, apertura de vínculos sin hueco, siembra de verificaciones y forzado de solo aviso (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancia de referencia: escenario §20.E-1 del intake (catálogo, inventario y vínculos), del que se toman los identificadores y valores concretos.
- El modelo del equipo no es legible del dispositivo, por eso entra como dato declarado por el operador; el sistema debe registrar esa condición y no presentarlo como medido.
- El detalle de la interfaz de descubrimiento y de los formularios corresponde a 03-UX-UI-DX.
- No se nombra aquí el mecanismo concreto de conexión con el equipo; se lo trata como adaptador de conexión, según las reglas de la categoría.

## 11. Interacción multiusuario y concurrencia

Trivial: un único administrador ejecuta el alta. No hay concurrencia de operadores.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-1 (§6), §20.E-1 y NB-04 |
| 1.1 | 2026-07-20 | Retroalimentación de la Fase B2: unificación de terminología "parque" → "equipos" |
