# 02 — Especificación funcional · Sai-Service-Core

Índice navegable de la especificación funcional. Define el qué del sistema: casos de uso, reglas de negocio, modelo conceptual y reglas conceptuales del modelo. Estado general: Borrador (2026-07-20). Autor: Orquestador SDD (AG-02).

Punto de entrada: [Especificacion-Funcional-v1.0.md](Especificacion-Funcional-v1.0.md) — índice maestro con la matriz NB → CU → RN → US.

## Casos de uso

| CU | Propósito en una línea | Estado |
| --- | --- | --- |
| [CU-01](Casos-De-Uso/CU-01-Autenticacion-Del-Administrador-v1.0.md) | Acceso autenticado del administrador: alta inicial, ingreso, cambio de contraseña y cierre de sesión | Borrador |
| [CU-02](Casos-De-Uso/CU-02-Alta-De-Equipos-Y-Puesta-En-Marcha-v1.0.md) | Dar de alta equipo, batería y host descubriendo el dispositivo y sembrando las verificaciones | Borrador |
| [CU-03](Casos-De-Uso/CU-03-Configuracion-De-Politicas-De-Apagado-v1.0.md) | Configurar la política de apagado creando una versión nueva, sin editar la vigente | Borrador |
| [CU-04](Casos-De-Uso/CU-04-Monitoreo-En-Vivo-Del-Estado-Del-Sai-v1.0.md) | Ver el estado en vivo del equipo, persistir muestras con calidad y derivar eventos | Borrador |
| [CU-05](Casos-De-Uso/CU-05-Ejecucion-Del-Apagado-Ordenado-Ante-Corte-v1.0.md) | Ejecutar o bloquear el apagado ordenado ante un corte sostenido, con retorno garantizado | Borrador |
| [CU-06](Casos-De-Uso/CU-06-Historicos-Y-Graficas-De-Evolucion-v1.0.md) | Consultar y graficar la evolución del suministro con marcas de eventos | Borrador |
| [CU-07](Casos-De-Uso/CU-07-Prueba-De-Bateria-Y-Veredicto-De-Salud-v1.0.md) | Probar la batería y emitir un veredicto de salud propio con su confianza y reserva | Borrador |
| [CU-08](Casos-De-Uso/CU-08-Recambio-De-Bateria-Y-Ficha-De-Vida-Util-v1.0.md) | Registrar el recambio de batería, cerrar y abrir vigencias y proyectar la ficha de vida útil | Borrador |
| [CU-09](Casos-De-Uso/CU-09-Reparacion-Y-Sustitucion-Del-Sai-v1.0.md) | Registrar reparación o sustitución del equipo y la cobertura suplente del host | Borrador |
| [CU-10](Casos-De-Uso/CU-10-Ventana-De-Mantenimiento-Y-Verificacion-v1.0.md) | Verificar los cuatro supuestos en una ventana física para desbloquear el apagado automático | Borrador |
| [CU-11](Casos-De-Uso/CU-11-Ingesta-Automatizada-De-Intervenciones-v1.0.md) | Ingerir intervenciones de un sistema externo con idempotencia y confianza diferenciada | Borrador |
| [CU-12](Casos-De-Uso/CU-12-Informe-De-Periodo-Y-Comparacion-De-Marcas-v1.0.md) | Informe de período y comparación de modelos por costo por año de servicio normalizado | Borrador |

## Reglas de negocio

| RN | Propósito en una línea | Estado |
| --- | --- | --- |
| [RN-01](Reglas-De-Negocio/RN-01-Arranque-Seguro-En-Solo-Alerta-v1.0.md) | El sistema arranca y permanece en solo aviso hasta que las verificaciones habiliten otra modalidad | Borrador |
| [RN-02](Reglas-De-Negocio/RN-02-Bloqueo-Por-Verificacion-v1.0.md) | Un supuesto requerido sin cumplir bloquea el apagado y degrada la modalidad a solo aviso | Borrador |
| [RN-03](Reglas-De-Negocio/RN-03-Validacion-Por-Efecto-Observado-v1.0.md) | Ninguna acción se da por ejecutada por ausencia de error, solo por efecto observado | Borrador |
| [RN-04](Reglas-De-Negocio/RN-04-Techo-Duro-Del-Tiempo-De-Apagado-v1.0.md) | El tiempo reservado para el apagado no supera los 540 segundos | Borrador |
| [RN-05](Reglas-De-Negocio/RN-05-Procedencia-Obligatoria-De-Todo-Valor-v1.0.md) | Todo valor almacenado declara su origen; el derivado declara sus variables de origen | Borrador |
| [RN-06](Reglas-De-Negocio/RN-06-Aptitud-De-Datos-Para-Tendencia-De-Salud-v1.0.md) | Datos derivados o pruebas no comparables no entran en la tendencia de salud | Borrador |
| [RN-07](Reglas-De-Negocio/RN-07-Todo-Importe-Con-Moneda-Y-Fecha-v1.0.md) | Todo importe lleva moneda y fecha; el equivalente normalizado viaja como derivado | Borrador |
| [RN-08](Reglas-De-Negocio/RN-08-Cuadre-De-Costos-De-Intervencion-v1.0.md) | El total de una intervención iguala la suma de repuestos y mano de obra | Borrador |
| [RN-09](Reglas-De-Negocio/RN-09-Idempotencia-De-La-Ingesta-v1.0.md) | Un reintento con la misma clave no duplica; con cuerpo distinto se rechaza por conflicto | Borrador |
| [RN-10](Reglas-De-Negocio/RN-10-Agregado-Con-Cobertura-Y-Advertencia-v1.0.md) | Todo agregado declara cobertura y advertencia y no se sirve como una muestra | Borrador |
| [RN-11](Reglas-De-Negocio/RN-11-Accion-Referida-A-Version-De-Politica-v1.0.md) | Toda acción referencia una versión de política, nunca la política | Borrador |
| [RN-12](Reglas-De-Negocio/RN-12-Baja-Logica-Y-Coherencia-Temporal-v1.0.md) | La unidad no se borra; se da de baja lógica y no admite operaciones posteriores a la baja | Borrador |
| [RN-13](Reglas-De-Negocio/RN-13-Vida-De-Flotacion-Con-Temperatura-De-Referencia-v1.0.md) | La vida de flotación esperada sin temperatura de referencia es inválida | Borrador |

## Modelo conceptual

- [Modelo-Conceptual-v1.0.md](Modelo-Datos/Modelo-Conceptual-v1.0.md) — 27 entidades y objetos de valor en cuatro capas, con diagrama entidad-relación. Estado: Borrador.

## Reglas conceptuales del modelo

| RC | Propósito en una línea | Estado |
| --- | --- | --- |
| [RC-01](Modelo-Datos/reglas-conceptuales-de-modelo/RC-01-Procedencia-Por-Valor-v1.0.md) | Todo valor de historia se envuelve en un Valor con Origen | Borrador |
| [RC-02](Modelo-Datos/reglas-conceptuales-de-modelo/RC-02-Vigencia-Como-Entidad-Con-Intervalo-v1.0.md) | Los vínculos temporales no se solapan y admiten a lo sumo uno vigente | Borrador |
| [RC-03](Modelo-Datos/reglas-conceptuales-de-modelo/RC-03-Sucesion-De-Vinculos-Sin-Hueco-v1.0.md) | Cerrar y abrir un vínculo en continuidad no deja hueco no intencional | Borrador |
| [RC-04](Modelo-Datos/reglas-conceptuales-de-modelo/RC-04-Agregado-No-Hereda-De-Muestra-v1.0.md) | Agregado y Muestra son entidades distintas y no intercambiables | Borrador |
| [RC-05](Modelo-Datos/reglas-conceptuales-de-modelo/RC-05-Accion-Referida-A-Version-De-Politica-v1.0.md) | Una acción referencia una versión de política, nunca la política | Borrador |
| [RC-06](Modelo-Datos/reglas-conceptuales-de-modelo/RC-06-Historia-Append-Only-v1.0.md) | Las entidades de historia se agregan pero no se actualizan ni se borran | Borrador |
| [RC-07](Modelo-Datos/reglas-conceptuales-de-modelo/RC-07-Resolucion-Temporal-De-La-Bateria-v1.0.md) | La métrica guarda dispositivo e instante; la batería se resuelve por el montaje | Borrador |
| [RC-08](Modelo-Datos/reglas-conceptuales-de-modelo/RC-08-Baja-Logica-Y-Coherencia-Temporal-v1.0.md) | La unidad física no se borra y no admite operaciones posteriores a su baja | Borrador |
| [RC-09](Modelo-Datos/reglas-conceptuales-de-modelo/RC-09-Evento-Referido-A-Regla-Versionada-v1.0.md) | Todo evento referencia su regla de derivación y la versión aplicada | Borrador |

## Cobertura de necesidades de negocio

Las 8 necesidades NB-01 a NB-08 del catálogo 01 quedan cubiertas por al menos un caso de uso, y cada caso de uso traza a al menos una necesidad. El detalle está en la matriz NB → CU → RN → US del índice maestro.
