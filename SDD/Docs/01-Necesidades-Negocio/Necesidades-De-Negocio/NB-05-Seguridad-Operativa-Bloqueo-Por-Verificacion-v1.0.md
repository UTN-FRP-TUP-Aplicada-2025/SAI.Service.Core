# NB-05 — Seguridad operativa: arranque seguro, bloqueo por verificación y validación por efecto observado

| Campo | Valor |
| --- | --- |
| Proyecto | Sai-Service-Core |
| Documento | NB-05-Seguridad-Operativa-Bloqueo-Por-Verificacion-v1.0.md |
| Versión | 1.1 |
| Estado | Borrador |
| Fecha | 2026-07-20 |
| Autor | Orquestador SDD (AG-01) |
| Trazabilidad upstream | SOLUTION-INTAKE §1, §3, §4 (F-11, F-15, F-25), §7 (CL-02, CL-03, CL-07), §8 (M-01), §11 (R-01, R-02, R-03, R-12); Vision-Producto-v1.0.md §3, §8; Alcance-Proyecto-v1.0.md §4 (C-06), §8 |
| Trazabilidad downstream | CU-01, CU-05, CU-10, CU-04 (02-Especificacion-Funcional) |

## 1. Descripción de la necesidad

El negocio necesita una garantía dura de que el servicio no va a apagar un servidor sin respaldo mientras no pueda probar que el servidor va a volver a encenderse. Esa garantía es el riesgo principal del proyecto: el servicio toma una decisión con consecuencias irreversibles, de noche y sin testigos; si falla, deja al servidor fuera de servicio o corrompido. La necesidad, entonces, es que el sistema arranque desconfiando de sus propios supuestos y solo se habilite cuando cada supuesto del que depende está verificado por evidencia.

Esto se traduce en tres reglas de negocio. La primera es el arranque seguro: el sistema arranca forzado en modo de solo aviso, no como recomendación sino como estado impuesto. La segunda es el bloqueo por verificación: si algún supuesto requerido está sin verificar, vencido o refutado, el apagado queda bloqueado y la modalidad efectiva degrada a solo aviso. La tercera es la validación por efecto observado: ninguna acción sobre el equipo se da por ejecutada porque no haya habido error, porque durante el relevamiento un comando que nunca llegó al equipo no produjo ningún mensaje de error.

Importa porque estas reglas son lo único que separa una automatización confiable de una que puede fallar en silencio. Sin ellas, todo el valor del apagado automático se apoya en optimismo.

El control de acceso del administrador único (capacidad F-15 del intake, Must Have, materializada por CU-01) se trata como habilitador transversal de seguridad anclado a esta necesidad, y por decisión de alcance no tiene una NB propia: hay un único administrador y ninguna gestión de usuarios ni de roles que modelar (exclusión E-05 del intake), de modo que la autenticación mínima queda registrada aquí de forma explícita en vez de implícita, sin abrir una necesidad de negocio separada.

## 2. Ejemplo de uso desde la perspectiva del negocio

El servicio se instala y arranca por sí solo en modo de solo aviso, mostrando en la pantalla principal que 0 de 4 supuestos están verificados. Aunque haya un corte, no apaga nada: solo avisa, porque no puede probar que el servidor volvería. Tras la ventana de mantenimiento en la que se verifican los supuestos uno por uno, el apagado con retorno queda habilitado. Meses después, la verificación del tiempo de apagado vence porque la carga del servidor cambió; el sistema vuelve a bloquear el apagado y degrada a solo aviso hasta que se repita la prueba, sin esperar a que ocurra un incidente para descubrirlo.

## 3. Impacto

- Confianza para delegar la decisión: es lo que permite que el administrador deje que el sistema decida apagar un servidor sin respaldo.
- Prevención del peor resultado: evita apagar cuando no se puede garantizar el reencendido.
- Honestidad de la ejecución: valida por efecto observado, de modo que el sistema no reporta como hecho algo que no ocurrió.
- Mantenimiento vivo de la garantía: las vigencias fuerzan a re-verificar cuando cambian las condiciones, sin intervención.
- Si queda sin resolver: el apagado automático se apoya en supuestos no probados y reintroduce el riesgo principal del proyecto.

## 4. Problema específico que resuelve

- Que el sistema arranque siempre en modo de solo aviso, como estado impuesto y no como recomendación.
- Que el apagado quede bloqueado mientras algún supuesto requerido esté sin verificar, vencido o refutado.
- Que una verificación refutada bloquee de forma dura, distinta de una simplemente vencida que solo pide repetir la prueba.
- Que ninguna acción sobre el equipo se dé por ejecutada por ausencia de error, sino por efecto observado.
- Que las verificaciones tengan vigencia y se re-evalúen al vencer o al cambiar las condiciones.

## 5. Criterios de éxito

| Criterio | Métrica | Target | Plazo |
| --- | --- | --- | --- |
| Arranque seguro por defecto | Arranques del servicio en un modo distinto de solo aviso sin supuestos verificados | 0 (100 % arranca en solo aviso) | Desde el primer arranque |
| Bloqueo por verificación efectivo | Apagados ejecutados con algún supuesto requerido sin verificar, vencido o refutado | 0 | Continuo |
| Validación por efecto observado | Acciones dadas por ejecutadas sin confirmar el efecto | 0 | Continuo |
| Vigencia de las verificaciones críticas | Caducidad del presupuesto de apagado y del reencendido automático | 180 días y 365 días respectivamente | Continuo |
| Salida del modo degradado | Supuestos verificados sobre supuestos requeridos | 4 de 4 (target propuesto, requiere ratificación — P-01) | 1 mes desde la puesta en marcha (M-01) |

## 6. Stakeholders involucrados

| Rol | Nivel | Qué pide o aporta |
| --- | --- | --- |
| Administrador único (rol propietario) | Propietario | Exige la garantía de no apagar sin poder probar el reencendido y ejecuta la ventana de mantenimiento con presencia física |
| Administrador único (rol implementador) | Implementador | Construye el arranque seguro, la regla de bloqueo, las vigencias y la validación por efecto observado |
| Host protegido i7infra | Beneficiario (sistema) | Es lo protegido: la regla de bloqueo evita apagarlo cuando no se puede garantizar que vuelva |

## 7. Trazabilidad a CU

| NB | CU prevista | Estado |
| --- | --- | --- |
| NB-05 | CU-01 Autenticación y gestión de la sesión del administrador | aprobada |
| NB-05 | CU-05 Ejecución del apagado ordenado ante corte sostenido | aprobada |
| NB-05 | CU-10 Ventana de mantenimiento y verificación de supuestos | aprobada |
| NB-05 | CU-04 Monitoreo en vivo del estado del SAI | aprobada |

## 8. Dependencias con otras NB

- Depende de NB-02 (Monitoreo en vivo y alertas): la validación por efecto observado y la renovación de verificaciones por evidencia se apoyan en los eventos del monitoreo.
- Depende de NB-07 (Configuración de políticas de apagado): la regla de bloqueo actúa sobre los supuestos que una versión de política declara requeridos.

## 9. Prioridad MoSCoW

Must Have. Es la garantía de seguridad que mitiga el riesgo principal del proyecto y sin la cual el apagado automático no puede habilitarse (SOLUTION-INTAKE §11, R-12; §4, F-11).

## 10. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §1, §3, §4, §7, §8 y §11, y de Vision-Producto-v1.0.md |
| 1.1 | 2026-07-20 | Reconciliación de trazabilidad §7 con los CU vigentes de 02 tras audit de Fase B; se agrega en §1 la nota de autenticación como habilitador transversal (F-15, CU-01) sin NB propia |
