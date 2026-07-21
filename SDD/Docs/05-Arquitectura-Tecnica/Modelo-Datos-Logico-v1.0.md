# Modelo lógico de datos — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Modelo-Datos-Logico-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)

Modelo físico del almacenamiento de Sai-Service-Core sobre **SQLite** accedido con **Entity Framework Core**, con migraciones versionadas en el repositorio y aplicadas al arranque (ADR-18). Deriva del modelo conceptual de 02 (`Modelo-Conceptual-v1.0.md`, 27 entidades en cuatro capas) y de la persistencia definida en el intake (§17 P.4 y P.10). Cada tabla de este documento traza a una entidad conceptual de origen; no hay tabla sin origen conceptual. La historia es append-only: sin `UPDATE` ni `DELETE` sobre sus tablas (ADR-04, RC-06).

Convenciones de tipos físicos SQLite usadas en todo el documento:

- **INTEGER**: claves sustitutas, contadores, banderas (`0`/`1`), segundos enteros.
- **TEXT**: cadenas, identificadores de negocio, enumeraciones persistidas como texto, mapas serializados (JSON), y **fechas e instantes en ISO-8601** (`YYYY-MM-DDTHH:MM:SSZ` o con desfase), por ausencia de tipo fecha nativo en SQLite.
- **REAL**: magnitudes de punto flotante (tensiones, coberturas, promedios).
- **NUMERIC**: importes monetarios y magnitudes donde se prefiere exactitud decimal a coma flotante.

Las claves sustitutas son `Id INTEGER PRIMARY KEY` (autoincremental gestionado por EF Core). El identificador de negocio legible (`fab-apc`, `ups-01`, `mue-20260719T011500`) se guarda además como `codigo TEXT` con restricción única, para preservar la trazabilidad con las instancias de los escenarios del intake.

---

## 1. Tablas o colecciones

Una subsección por tabla, agrupadas por las cuatro capas del modelo conceptual (ADR-07: separación de catálogo, inventario e historia). Se indica el propósito y la entidad conceptual de 02 de la que deriva.

### Capa de catálogo

#### 1.1 `Fabricante`
Marca o razón social que fabrica dispositivos o baterías. Deriva de **1.1 Fabricante**. Admite el fabricante `Sin identificar` cuando el descriptor del equipo no expone marca.

#### 1.2 `ModeloDispositivo`
Tipo de equipo de alimentación, independiente de la unidad concreta. Deriva de **1.2 ModeloDispositivo**. Referencia a `Fabricante`.

#### 1.3 `ModeloBateria`
Tipo de batería con tecnología, capacidad y vida de flotación esperada. Deriva de **1.3 ModeloBateria**. Referencia a `Fabricante`. La vida de flotación es inválida sin temperatura de referencia (RC-07 / RN-13).

#### 1.4 `TipoIntervencion`
Clase de servicio técnico posible y el tipo de unidad que afecta, con su bandera de planificable. Deriva de **1.4 TipoIntervencion**.

#### 1.5 `Proveedor`
Quien ejecuta intervenciones y puede recibir la disposición final de una batería. Deriva de **1.5 Proveedor**.

### Capa de inventario

#### 1.6 `UnidadFisica`
Tabla única que materializa el supertipo **1.6 UnidadFisica** y sus tres especializaciones **1.7 Host**, **1.8 Dispositivo** y **1.9 Bateria** mediante **herencia Table-Per-Hierarchy (TPH)** de EF Core: una sola tabla con una **columna discriminadora** `Tipo TEXT` (`Host`, `Dispositivo`, `Bateria`). Aporta el ciclo de vida común (`estado`, `fechaBaja`, `motivoBaja`) y la baja lógica (RC-08): sin borrado físico, la unidad permanece consultable tras la baja y no admite operaciones posteriores a ella. Las columnas propias de cada especialización son nulables porque solo aplican a su discriminador (por ejemplo `numeroSerie`, `fechaFabricacion` en `Bateria`; `criticidad` en `Host`; `modeloDispositivoId` en `Dispositivo`).

Motivación de TPH frente a Table-Per-Type (TPT) y Table-Per-Concrete (TPC): las tres especializaciones comparten el ciclo de vida y la baja lógica, las consultas de inventario recorren el conjunto completo de equipos, y el volumen de filas es bajo (decenas de unidades). TPH evita los `JOIN` de TPT y mantiene el ciclo de vida en un solo lugar. El costo es que las columnas específicas quedan nulables; se acepta y se refuerza con `CHECK` por discriminador (§4).

#### 1.7 Host — *especialización de `UnidadFisica`*
El servidor protegido (criticidad, en servicio desde). Deriva de **1.7 Host**. Se persiste como filas de `UnidadFisica` con `Tipo = 'Host'`.

#### 1.8 Dispositivo — *especialización de `UnidadFisica`*
Unidad concreta de equipo de alimentación, de un `ModeloDispositivo`. Deriva de **1.8 Dispositivo**. Filas con `Tipo = 'Dispositivo'`. `numeroSerie` es anulable a propósito (muchos equipos no lo exponen).

#### 1.9 Bateria — *especialización de `UnidadFisica`*
Unidad concreta de batería, de un `ModeloBateria`. Deriva de **1.9 Bateria**. Filas con `Tipo = 'Bateria'`. `fechaFabricacion` puede ser anterior a `fechaCompra`; la edad real se cuenta desde la fabricación.

### Capa de vínculos temporales

#### 1.10 `MontajeBateria`
Período en que una batería estuvo montada en un dispositivo, en una posición, con intervalo `desde`/`hasta`. Deriva de **1.10 MontajeBateria**. Sin solapamiento por dispositivo y posición; a lo sumo uno vigente (`hasta` nulo) por clave (RC-02, RC-03; ADR-05).

#### 1.11 `CoberturaHost`
Período en que un dispositivo cubrió a un host, con intervalo `desde`/`hasta`. Deriva de **1.11 CoberturaHost**. Sin solapamiento por host; a lo sumo una vigente (RC-02, RC-03; ADR-05).

### Capa de historia (append-only)

Todas las tablas de esta capa son **append-only** (ADR-04, RC-06): se insertan filas, nunca se actualizan ni se borran. La corrección de un dato se modela como un hecho nuevo, no como una mutación.

#### 1.12 `FuenteDatos`
Origen declarado de un conjunto de datos, con su confianza base. Deriva de **1.12 FuenteDatos**.

#### 1.13 `SesionSondeo`
Período de sondeo de un dispositivo bajo un mismo driver, versión y dialecto, con su **mapa de variable a origen** serializado. Deriva de **1.13 SesionSondeo**. Es el lugar donde se guarda la procedencia una sola vez (ver §2, optimización de §17 P.4). Referencia a `Dispositivo` (`UnidadFisica`) y a `FuenteDatos`.

#### 1.14 `Muestra`
Lectura del estado del equipo en un instante, con su calidad (completa, parcial, perdida). Deriva de **1.14 Muestra**. Guarda `dispositivoId` e `instante`, **no** la batería: la batería se resuelve por `MontajeBateria` (RC-07). Los valores llevan su procedencia por herencia del mapa de la `SesionSondeo`, no por columna propia (§2). Perdida implica valores sin dato (RC-01).

#### 1.15 `Agregado`
Resumen de una ventana de tiempo de una variable, con función, cantidad de muestras y cobertura. Deriva de **1.15 Agregado**. Entidad distinta de `Muestra`, no intercambiable ni heredada (RC-04, ADR-08). Para `input.voltage` conserva mínimo y máximo además del promedio (§17 P.4). Cobertura y advertencia obligatorias.

#### 1.16 `ReglaDerivacion`
Regla versionada que deriva eventos a partir de muestras. Deriva de **1.16 ReglaDerivacion**. Conserva versiones anteriores como filas propias (append-only): la versión es parte de la identidad.

#### 1.17 `Evento`
Hecho derivado de muestras por una regla, con tipo, duración e incertidumbre, y la regla y versión que lo produjeron. Deriva de **1.17 Evento**. Referencia obligatoria a `ReglaDerivacion` y a la versión aplicada (RC-09).

#### 1.18 `PruebaBateria`
Prueba de batería con muestreo denso, el montaje congelado, sus derivados y su veredicto de salud. Deriva de **1.18 PruebaBateria**. Congela `montajeBateriaId` (RC-07); no comparable si la carga difiere (RC-06 aptitud).

#### 1.19 `Politica`
Agrupador lógico de versiones de política de apagado. Deriva de **1.19 Politica**.

#### 1.20 `VersionPolitica`
Versión inmutable de una política, con modalidad, umbral de disparo, tiempo reservado de apagado y verificaciones requeridas. Deriva de **1.20 VersionPolitica**. `tiempoReservadoApagadoSeg ≤ 540` (RN-04, I-10). El vínculo N-N con `Verificacion` se materializa en la tabla puente `VersionPoliticaVerificacion` (§1.24).

#### 1.21 `Accion`
Decisión del planificador ante un evento de disparo, referida a una **versión de política**, con modalidad solicitada, modalidad efectiva y resultado. Deriva de **1.21 Accion**. FK a `VersionPolitica`, nunca a `Politica` (RC-05). FK opcional al `Evento` que la desencadena.

#### 1.22 `Verificacion`
Supuesto del que depende el apagado, con evidencia, método, vigencia y estado (sin verificar, verificado, vencido, refutado). Deriva de **1.22 Verificacion**.

#### 1.23 `Intervencion`
Servicio técnico registrado, con costos, hallazgos, mediciones, disposición final, clave de idempotencia y dos tiempos (`tiempoValido`, `tiempoRegistrado`). Deriva de **1.23 Intervencion**. Costos que cuadran (RN-08), clave idempotente única (RN-09, I-19). FK a `TipoIntervencion`, `Dispositivo`, `Proveedor` (opcional) y `FuenteDatos`. El vínculo N-N con `Bateria` se materializa en la tabla puente `IntervencionBateria` (§1.25).

#### 1.24 `VersionPoliticaVerificacion` — *tabla puente*
Materializa el vínculo N-N «una versión de política requiere muchas verificaciones» del modelo conceptual (relación VersionPolitica }o--o{ Verificacion). No es una entidad conceptual propia; existe por la normalización del N-N.

#### 1.25 `IntervencionBateria` — *tabla puente*
Materializa el vínculo N-N «una intervención afecta a una o varias baterías» (relación Bateria }o--o{ Intervencion). No es una entidad conceptual propia; existe por la normalización del N-N.

### Capa de proyección

#### 1.26 `FichaVidaUtil`
Proyección que cierra la historia de una batería: días en servicio, cumplimiento de la expectativa, eventos soportados, tendencia de salud y costo por año de servicio normalizado. Deriva de **1.24 FichaVidaUtil**. Relación 1 — 0..1 con `Bateria`.

### Actor persistido

#### 1.27 `AspNetUsers` (Identity)
Cuenta única de administrador. Deriva de **1.27 Usuario administrador**. Se materializa con el esquema de ASP.NET Core Identity (ADR-16): un solo usuario, un solo rol `administrador`, sin gestión de usuarios ni roles (exclusión E-05). Se documenta como tabla de origen para trazabilidad, pero su DDL lo genera la migración de Identity, no un mapeo propio del dominio.

### Objetos de valor (no son tablas)

Las entidades conceptuales **1.25 Valor con Origen** y **1.26 Dinero** son objetos de valor: no generan tablas propias, se mapean como **owned types** de EF Core y se materializan como columnas dentro de la tabla dueña. Su patrón físico se documenta en §2.

---

## 2. Atributos con tipo de dato físico

Nombre, tipo físico SQLite, nulabilidad y default por tabla. `NN` = NOT NULL; `NULL` = nulable. FK notada como referencia.

### Patrón `Valor<T>` con `Origen` (ADR-06, RC-01)

Todo valor de historia se envuelve conceptualmente en un `Valor<T>` con su procedencia. Físicamente se resuelve de dos maneras según el volumen:

- **Historia de muestreo denso** (`Muestra`, `Agregado`): la procedencia (`o`) y la lista de derivación (`de`) **no se guardan por fila**. Se declaran **una sola vez por `SesionSondeo`** en la columna `mapaVariableOrigen TEXT` (JSON: variable → `{o, de}`), evitando duplicar constantes ~17.280 veces por día (§17 P.4). La `Muestra` guarda solo el valor y su calidad; la procedencia se resuelve por el mapa de su sesión y se expande recién en la API.
- **Historia de baja cardinalidad** (`Intervencion`, `PruebaBateria`, `VersionPolitica`, catálogo con procedencia como `ModeloDispositivo.potenciaNominal`): el `Valor<T>` se mapea como **owned type** en columnas inline con el sufijo del atributo: `<attr>Valor` (REAL/NUMERIC/INTEGER), `<attr>Origen TEXT` y, si aplica, `<attr>Derivadas TEXT` (JSON). El `Origen` persiste como texto con el conjunto permitido `medido | derivado | estimadoPorDriver | declarado | imputado | noCalculable`. Un valor `imputado` o `noCalculable` deja el `Valor` en `NULL` con el `Origen` declarado; nunca un número inventado.

### Patrón `Dinero` (RN-07)

`Dinero` se mapea como owned type con columnas inline: `<attr>Monto NUMERIC NN`, `<attr>Moneda TEXT NN`, `<attr>Fecha TEXT NN` (ISO-8601) y, opcionalmente, `<attr>EquivNormalizadoMonto NUMERIC NULL`, `<attr>EquivNormalizadoMoneda TEXT NULL`, `<attr>FuenteCotizacion TEXT NULL`. Moneda y fecha son obligatorias siempre que haya monto (RN-07; ver `CHECK` en §4).

### 2.1 `Fabricante`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| nombre | TEXT | NN | — |
| identificado | INTEGER | NN | 1 |

### 2.2 `ModeloDispositivo`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| fabricanteId | INTEGER | NN (FK → Fabricante) | — |
| nombre | TEXT | NN | — |
| lineaTopologia | TEXT | NULL | — |
| tensionNominalV | REAL | NULL | — |
| potenciaVaNominalValor | REAL | NULL | — |
| potenciaVaNominalOrigen | TEXT | NN | 'declarado' |

### 2.3 `ModeloBateria`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| fabricanteId | INTEGER | NN (FK → Fabricante) | — |
| nombre | TEXT | NN | — |
| tecnologia | TEXT | NULL | — |
| capacidadAh | REAL | NULL | — |
| tensionNominalV | REAL | NULL | — |
| vidaFlotacionAniosMin | REAL | NULL | — |
| vidaFlotacionAniosMax | REAL | NULL | — |
| temperaturaReferenciaC | REAL | NULL | — |

`vidaFlotacion*` es inválida sin `temperaturaReferenciaC` (RC-07 / RN-13; `CHECK` en §4).

### 2.4 `TipoIntervencion`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| nombre | TEXT | NN | — |
| afectaA | TEXT | NN | — |
| planificable | INTEGER | NN | 0 |

### 2.5 `Proveedor`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| nombre | TEXT | NN | — |
| contacto | TEXT | NULL | — |
| receptorDisposicionFinal | INTEGER | NN | 0 |

### 2.6 `UnidadFisica` (TPH — Host / Dispositivo / Bateria)
| Columna | Tipo | Nulabilidad | Default | Aplica a |
| --- | --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental | todas |
| Tipo | TEXT | NN (discriminador) | — | todas (`Host`/`Dispositivo`/`Bateria`) |
| codigo | TEXT | NN | — | todas |
| estado | TEXT | NN | 'EnStock' | todas (`EnStock`/`EnServicio`/`EnReparacion`/`DadoDeBaja`) |
| fechaBaja | TEXT | NULL | — | todas (ISO-8601) |
| motivoBaja | TEXT | NULL | — | todas |
| criticidad | TEXT | NULL | — | Host |
| enServicioDesde | TEXT | NULL | — | Host |
| modeloDispositivoId | INTEGER | NULL (FK → ModeloDispositivo) | — | Dispositivo |
| numeroSerie | TEXT | NULL | — | Dispositivo (anulable a propósito) |
| modeloBateriaId | INTEGER | NULL (FK → ModeloBateria) | — | Bateria |
| fechaFabricacion | TEXT | NULL | — | Bateria (ISO-8601) |
| fechaCompra | TEXT | NULL | — | Bateria (ISO-8601) |

### 2.7 `MontajeBateria`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| bateriaId | INTEGER | NN (FK → UnidadFisica) | — |
| dispositivoId | INTEGER | NN (FK → UnidadFisica) | — |
| posicion | TEXT | NN | — |
| desde | TEXT | NN | — (ISO-8601) |
| hasta | TEXT | NULL | — (nulo = vigente) |

### 2.8 `CoberturaHost`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| dispositivoId | INTEGER | NN (FK → UnidadFisica) | — |
| hostId | INTEGER | NN (FK → UnidadFisica) | — |
| desde | TEXT | NN | — (ISO-8601) |
| hasta | TEXT | NULL | — (nulo = vigente) |

### 2.9 `FuenteDatos`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| descripcion | TEXT | NULL | — |
| confianzaBase | TEXT | NN | — (`alta`/`media`/`baja`) |

### 2.10 `SesionSondeo`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| dispositivoId | INTEGER | NN (FK → UnidadFisica) | — |
| fuenteDatosId | INTEGER | NN (FK → FuenteDatos) | — |
| driver | TEXT | NN | — |
| driverVersion | TEXT | NULL | — |
| dialecto | TEXT | NULL | — |
| intervaloSeg | INTEGER | NN | 5 |
| desde | TEXT | NN | — (ISO-8601) |
| hasta | TEXT | NULL | — |
| mapaVariableOrigen | TEXT | NN | — (JSON: variable → {o, de}) |

### 2.11 `Muestra`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| dispositivoId | INTEGER | NN (FK → UnidadFisica) | — |
| sesionSondeoId | INTEGER | NN (FK → SesionSondeo) | — |
| instante | TEXT | NN | — (ISO-8601) |
| calidad | TEXT | NN | — (`completa`/`parcial`/`perdida`) |
| valores | TEXT | NULL | — (JSON: variable → valor; sin `o` ni `de`, resueltos por el mapa de la sesión) |

No guarda `bateriaId`: la batería se resuelve por `MontajeBateria` (RC-07). La procedencia no se guarda por fila (§17 P.4).

### 2.12 `Agregado`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| dispositivoId | INTEGER | NN (FK → UnidadFisica) | — |
| variable | TEXT | NN | — |
| ventanaInicio | TEXT | NN | — (ISO-8601) |
| ventanaDuracion | TEXT | NN | 'PT1H' (ISO-8601 duration) |
| funcion | TEXT | NN | — |
| nMuestras | INTEGER | NN | — |
| cobertura | REAL | NN | — |
| advertencia | TEXT | NULL | — |
| promedio | REAL | NULL | — |
| minimo | REAL | NULL | — |
| maximo | REAL | NULL | — |

`minimo`/`maximo` obligatorios para `input.voltage` (§17 P.4). No hereda de `Muestra` (RC-04).

### 2.13 `ReglaDerivacion`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| version | INTEGER | NN | 1 |
| descripcion | TEXT | NULL | — |
| parametros | TEXT | NULL | — (JSON, ej. umbral de microcorte) |
| vigenteDesde | TEXT | NN | — (ISO-8601) |

### 2.14 `Evento`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| dispositivoId | INTEGER | NN (FK → UnidadFisica) | — |
| tipo | TEXT | NN | — |
| instante | TEXT | NN | — (ISO-8601) |
| duracionSeg | REAL | NULL | — |
| incertidumbreDuracionSeg | REAL | NULL | — |
| reglaDerivacionId | INTEGER | NN (FK → ReglaDerivacion) | — |
| reglaVersion | INTEGER | NN | — |

`reglaDerivacionId` + `reglaVersion` obligatorios (RC-09).

### 2.15 `PruebaBateria`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| dispositivoId | INTEGER | NN (FK → UnidadFisica) | — |
| montajeBateriaId | INTEGER | NN (FK → MontajeBateria) | — (congelado, RC-07) |
| instante | TEXT | NN | — (ISO-8601) |
| caidaTensionValor | REAL | NULL | — |
| caidaTensionOrigen | TEXT | NN | 'derivado' |
| cargaPorcentaje | INTEGER | NULL | — |
| comparable | INTEGER | NN | 0 |
| veredicto | TEXT | NULL | — |
| confianzaVeredicto | TEXT | NULL | — |

### 2.16 `Politica`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| nombre | TEXT | NN | — |

### 2.17 `VersionPolitica`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| politicaId | INTEGER | NN (FK → Politica) | — |
| version | INTEGER | NN | 1 |
| modalidad | TEXT | NN | — |
| umbralDisparoSegundos | INTEGER | NULL | 300 |
| tiempoReservadoApagadoSeg | INTEGER | NULL | — (≤ 540, I-10) |
| creadaEn | TEXT | NN | — (ISO-8601) |

### 2.18 `VersionPoliticaVerificacion` (puente N-N)
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| versionPoliticaId | INTEGER | NN (FK → VersionPolitica) | — |
| verificacionId | INTEGER | NN (FK → Verificacion) | — |

PK compuesta (`versionPoliticaId`, `verificacionId`).

### 2.19 `Accion`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| eventoId | INTEGER | NULL (FK → Evento) | — |
| versionPoliticaId | INTEGER | NN (FK → VersionPolitica) | — (RC-05) |
| instante | TEXT | NN | — (ISO-8601) |
| modalidadSolicitada | TEXT | NN | — |
| modalidadEfectiva | TEXT | NN | — |
| resultado | TEXT | NN | — |

### 2.20 `Verificacion`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| supuesto | TEXT | NN | — |
| metodo | TEXT | NULL | — |
| estado | TEXT | NN | 'sinVerificar' (`sinVerificar`/`verificado`/`vencido`/`refutado`) |
| evidencia | TEXT | NULL | — |
| vigenciaHasta | TEXT | NULL | — (ISO-8601) |
| actualizadoEn | TEXT | NN | — (ISO-8601) |

### 2.21 `Intervencion`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| claveIdempotencia | TEXT | NN | — (única, RN-09) |
| tipoIntervencionId | INTEGER | NN (FK → TipoIntervencion) | — |
| dispositivoId | INTEGER | NN (FK → UnidadFisica) | — |
| proveedorId | INTEGER | NULL (FK → Proveedor) | — |
| fuenteDatosId | INTEGER | NN (FK → FuenteDatos) | — |
| tiempoValido | TEXT | NN | — (cuándo ocurrió, ISO-8601) |
| tiempoRegistrado | TEXT | NN | — (cuándo se registró, ISO-8601) |
| hallazgos | TEXT | NULL | — |
| disposicionFinal | TEXT | NULL | — |
| costoRepuestosMonto | NUMERIC | NULL | — |
| costoRepuestosMoneda | TEXT | NULL | — |
| costoRepuestosFecha | TEXT | NULL | — |
| costoManoObraMonto | NUMERIC | NULL | — |
| costoManoObraMoneda | TEXT | NULL | — |
| costoManoObraFecha | TEXT | NULL | — |
| costoTotalMonto | NUMERIC | NN | — |
| costoTotalMoneda | TEXT | NN | — |
| costoTotalFecha | TEXT | NN | — |

Los importes usan el patrón `Dinero` (RN-07). Los costos cuadran: total = repuestos + mano de obra (RN-08; `CHECK` en §4).

### 2.22 `IntervencionBateria` (puente N-N)
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| intervencionId | INTEGER | NN (FK → Intervencion) | — |
| bateriaId | INTEGER | NN (FK → UnidadFisica) | — |

PK compuesta (`intervencionId`, `bateriaId`).

### 2.23 `FichaVidaUtil`
| Columna | Tipo | Nulabilidad | Default |
| --- | --- | --- | --- |
| Id | INTEGER | NN (PK) | autoincremental |
| codigo | TEXT | NN | — |
| bateriaId | INTEGER | NN (FK → UnidadFisica, única) | — |
| diasEnServicio | INTEGER | NN | — |
| cumplioExpectativa | INTEGER | NN | — |
| eventosSoportados | INTEGER | NULL | — |
| tendenciaSalud | TEXT | NULL | — |
| costoPorAnioServicioMonto | NUMERIC | NULL | — |
| costoPorAnioServicioMoneda | TEXT | NULL | — |
| costoPorAnioServicioFecha | TEXT | NULL | — |
| cerradaEn | TEXT | NN | — (ISO-8601) |

### 2.24 `AspNetUsers` y tablas de Identity
Generadas por ASP.NET Core Identity (ADR-16): `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` y auxiliares. Tipos y columnas los define el proveedor de Identity; no se remapean aquí. Un solo usuario, rol `administrador`.

---

## 3. Índices

| Identificador | Tabla | Columnas | Tipo | Motivación |
| --- | --- | --- | --- | --- |
| UX_Fabricante_Codigo | Fabricante | (codigo) | único | Identificador de negocio estable |
| UX_ModeloDispositivo_Codigo | ModeloDispositivo | (codigo) | único | Identificador de negocio |
| UX_ModeloBateria_Codigo | ModeloBateria | (codigo) | único | Identificador de negocio |
| UX_TipoIntervencion_Codigo | TipoIntervencion | (codigo) | único | Identificador de negocio |
| UX_Proveedor_Codigo | Proveedor | (codigo) | único | Identificador de negocio |
| UX_UnidadFisica_Codigo | UnidadFisica | (codigo) | único | Identificador de negocio de host/dispositivo/batería |
| IX_UnidadFisica_Tipo_Estado | UnidadFisica | (Tipo, estado) | compuesto | Listado de inventario por especialización y estado (baja lógica, RC-08) |
| IX_MontajeBateria_Dispositivo_Posicion_Vigente | MontajeBateria | (dispositivoId, posicion) WHERE hasta IS NULL | único parcial | A lo sumo un montaje vigente por dispositivo y posición (RC-02) |
| IX_MontajeBateria_Dispositivo_Desde | MontajeBateria | (dispositivoId, desde) | compuesto | Resolución temporal de la batería por intervalo (RC-07) |
| IX_MontajeBateria_Bateria_Desde | MontajeBateria | (bateriaId, desde) | compuesto | Historia de montajes de una batería (ficha de vida útil) |
| IX_CoberturaHost_Host_Vigente | CoberturaHost | (hostId) WHERE hasta IS NULL | único parcial | A lo sumo una cobertura vigente por host (RC-02) |
| IX_CoberturaHost_Dispositivo_Desde | CoberturaHost | (dispositivoId, desde) | compuesto | Resolución de qué host cubre un dispositivo en un instante |
| IX_SesionSondeo_Dispositivo_Desde | SesionSondeo | (dispositivoId, desde) | compuesto | Ubicar la sesión vigente de un dispositivo para resolver procedencia |
| IX_Muestra_Dispositivo_Instante | Muestra | (dispositivoId, instante) | compuesto | Resolución temporal: consulta de históricos por dispositivo y rango (CU-06); soporte de la agregación por ventana |
| IX_Muestra_Sesion | Muestra | (sesionSondeoId) | simple | Expandir procedencia por sesión; purga de muestras > 30 días |
| IX_Agregado_Dispositivo_Variable_Ventana | Agregado | (dispositivoId, variable, ventanaInicio) | único compuesto | Un agregado por dispositivo, variable y ventana; consulta de informes (CU-12) |
| IX_Evento_Dispositivo_Instante | Evento | (dispositivoId, instante) | compuesto | Línea de tiempo de eventos (CU-06) |
| IX_Evento_Regla_Version | Evento | (reglaDerivacionId, reglaVersion) | compuesto | Trazar eventos a la regla y versión que los produjo (RC-09) |
| IX_PruebaBateria_Montaje | PruebaBateria | (montajeBateriaId) | simple | Pruebas de un montaje congelado (RC-07) |
| UX_ReglaDerivacion_Codigo_Version | ReglaDerivacion | (codigo, version) | único compuesto | Versión como parte de la identidad de la regla |
| UX_VersionPolitica_Politica_Version | VersionPolitica | (politicaId, version) | único compuesto | Una versión por política; inmutabilidad |
| IX_Accion_VersionPolitica | Accion | (versionPoliticaId) | simple | Acciones regidas por una versión de política (RC-05) |
| IX_Accion_Evento | Accion | (eventoId) | simple | Acción desencadenada por un evento |
| UX_Intervencion_ClaveIdempotencia | Intervencion | (claveIdempotencia) | único | Idempotencia de ingesta (RN-09, I-19): reenviar una clave ya vista devuelve el registro existente |
| IX_Intervencion_Dispositivo_TiempoValido | Intervencion | (dispositivoId, tiempoValido) | compuesto | Historial de servicio de un dispositivo (CU-08) |
| UX_FichaVidaUtil_Bateria | FichaVidaUtil | (bateriaId) | único | A lo sumo una ficha por batería (1 — 0..1) |

---

## 4. Restricciones

### 4.1 Claves primarias
Cada tabla de dominio: `Id INTEGER PRIMARY KEY` (autoincremental). Tablas puente: PK compuesta — `VersionPoliticaVerificacion (versionPoliticaId, verificacionId)`, `IntervencionBateria (intervencionId, bateriaId)`. Identity: PK las define el proveedor.

### 4.2 Claves foráneas
| Tabla | Columna | Referencia | Nulable | Regla |
| --- | --- | --- | --- | --- |
| ModeloDispositivo | fabricanteId | Fabricante(Id) | No | — |
| ModeloBateria | fabricanteId | Fabricante(Id) | No | — |
| UnidadFisica | modeloDispositivoId | ModeloDispositivo(Id) | Sí | solo `Tipo='Dispositivo'` |
| UnidadFisica | modeloBateriaId | ModeloBateria(Id) | Sí | solo `Tipo='Bateria'` |
| MontajeBateria | bateriaId | UnidadFisica(Id) | No | — |
| MontajeBateria | dispositivoId | UnidadFisica(Id) | No | — |
| CoberturaHost | dispositivoId | UnidadFisica(Id) | No | — |
| CoberturaHost | hostId | UnidadFisica(Id) | No | — |
| SesionSondeo | dispositivoId | UnidadFisica(Id) | No | — |
| SesionSondeo | fuenteDatosId | FuenteDatos(Id) | No | — |
| Muestra | dispositivoId | UnidadFisica(Id) | No | — |
| Muestra | sesionSondeoId | SesionSondeo(Id) | No | — |
| Agregado | dispositivoId | UnidadFisica(Id) | No | — |
| Evento | dispositivoId | UnidadFisica(Id) | No | — |
| Evento | reglaDerivacionId | ReglaDerivacion(Id) | No | RC-09 |
| PruebaBateria | dispositivoId | UnidadFisica(Id) | No | — |
| PruebaBateria | montajeBateriaId | MontajeBateria(Id) | No | RC-07 (congelado) |
| VersionPolitica | politicaId | Politica(Id) | No | — |
| Accion | versionPoliticaId | VersionPolitica(Id) | No | **RC-05: nunca a Politica** |
| Accion | eventoId | Evento(Id) | Sí | — |
| Intervencion | tipoIntervencionId | TipoIntervencion(Id) | No | — |
| Intervencion | dispositivoId | UnidadFisica(Id) | No | — |
| Intervencion | proveedorId | Proveedor(Id) | Sí | 0..1 |
| Intervencion | fuenteDatosId | FuenteDatos(Id) | No | — |
| FichaVidaUtil | bateriaId | UnidadFisica(Id) | No | única |
| VersionPoliticaVerificacion | versionPoliticaId / verificacionId | VersionPolitica / Verificacion | No | N-N |
| IntervencionBateria | intervencionId / bateriaId | Intervencion / UnidadFisica | No | N-N |

Sin `ON DELETE CASCADE` sobre las tablas de historia: son append-only (ADR-04, RC-06), no se borran.

### 4.3 Restricciones únicas
- `codigo` único en cada tabla de catálogo, inventario, vínculos e historia (índices `UX_*` de §3).
- `Intervencion.claveIdempotencia` **única** (RN-09, I-19).
- `MontajeBateria`: único parcial `(dispositivoId, posicion) WHERE hasta IS NULL` — a lo sumo un montaje vigente (RC-02).
- `CoberturaHost`: único parcial `(hostId) WHERE hasta IS NULL` — a lo sumo una cobertura vigente (RC-02).
- `Agregado (dispositivoId, variable, ventanaInicio)` único (RC-04, evita duplicar la ventana).
- `ReglaDerivacion (codigo, version)`, `VersionPolitica (politicaId, version)`, `FichaVidaUtil (bateriaId)` únicos.

### 4.4 CHECK y valores permitidos
| Restricción | Tabla | Condición |
| --- | --- | --- |
| CK_UnidadFisica_Estado | UnidadFisica | `estado IN ('EnStock','EnServicio','EnReparacion','DadoDeBaja')` |
| CK_UnidadFisica_Tipo | UnidadFisica | `Tipo IN ('Host','Dispositivo','Bateria')` |
| CK_UnidadFisica_Baja | UnidadFisica | `(estado='DadoDeBaja') = (fechaBaja IS NOT NULL)` (baja lógica coherente, RC-08) |
| CK_UnidadFisica_DispModelo | UnidadFisica | `Tipo <> 'Dispositivo' OR modeloDispositivoId IS NOT NULL` |
| CK_UnidadFisica_BatModelo | UnidadFisica | `Tipo <> 'Bateria' OR modeloBateriaId IS NOT NULL` |
| CK_ModeloBateria_VidaTemp | ModeloBateria | `vidaFlotacionAniosMin IS NULL OR temperaturaReferenciaC IS NOT NULL` (RC-07 / RN-13) |
| CK_Muestra_Calidad | Muestra | `calidad IN ('completa','parcial','perdida')` |
| CK_Muestra_Perdida | Muestra | `calidad <> 'perdida' OR valores IS NULL` (perdida no lleva datos, RC-01) |
| CK_Agregado_Cobertura | Agregado | `cobertura >= 0 AND cobertura <= 1` |
| CK_Verificacion_Estado | Verificacion | `estado IN ('sinVerificar','verificado','vencido','refutado')` |
| CK_VersionPolitica_Tiempo | VersionPolitica | `tiempoReservadoApagadoSeg IS NULL OR tiempoReservadoApagadoSeg <= 540` (I-10, RN-04) |
| CK_Intervencion_CostosCuadran | Intervencion | `costoTotalMonto = COALESCE(costoRepuestosMonto,0) + COALESCE(costoManoObraMonto,0)` (RN-08) |
| CK_Intervencion_DineroTotal | Intervencion | `costoTotalMoneda IS NOT NULL AND costoTotalFecha IS NOT NULL` (RN-07) |
| CK_Intervencion_DineroRepuestos | Intervencion | `costoRepuestosMonto IS NULL OR (costoRepuestosMoneda IS NOT NULL AND costoRepuestosFecha IS NOT NULL)` (RN-07) |
| CK_Intervencion_DineroManoObra | Intervencion | `costoManoObraMonto IS NULL OR (costoManoObraMoneda IS NOT NULL AND costoManoObraFecha IS NOT NULL)` (RN-07) |
| CK_Vigencia_Intervalo | MontajeBateria, CoberturaHost | `hasta IS NULL OR hasta >= desde` (intervalo bien formado, ADR-05) |
| CK_Origen_* | ModeloDispositivo, PruebaBateria y demás columnas `*Origen` | `<attr>Origen IN ('medido','derivado','estimadoPorDriver','declarado','imputado','noCalculable')` (ADR-06, RC-01) |

Nota: SQLite no ofrece restricciones de exclusión por rango; el no-solapamiento de intervalos (RC-02) se garantiza con el único parcial sobre el vínculo vigente más la validación de sucesión sin hueco (RC-03) en la capa de aplicación (`Domain`), que es el único proceso escritor.

---

## 5. Migración inicial

El esquema se versiona con **migraciones de EF Core** guardadas en el repositorio y aplicadas al arranque del servicio; no hay generación automática de esquema en producción (ADR-18, §17 P.4).

- **Identificador:** `20260720000001_InitialCreate` (migración inicial del dominio).
- **Migración complementaria de Identity:** `20260720000002_AddIdentitySchema` crea `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` y auxiliares (ADR-16).
- **Resumen del cambio de `InitialCreate`:** crea las tablas de catálogo (`Fabricante`, `ModeloDispositivo`, `ModeloBateria`, `TipoIntervencion`, `Proveedor`), la tabla TPH `UnidadFisica` con su discriminador `Tipo`, los vínculos temporales (`MontajeBateria`, `CoberturaHost`), las tablas de historia append-only (`FuenteDatos`, `SesionSondeo`, `Muestra`, `Agregado`, `ReglaDerivacion`, `Evento`, `PruebaBateria`, `Politica`, `VersionPolitica`, `Accion`, `Verificacion`, `Intervencion`), las puentes N-N (`VersionPoliticaVerificacion`, `IntervencionBateria`) y la proyección `FichaVidaUtil`. Crea los índices de §3, las FK de §4.2 y las restricciones `UNIQUE`/`CHECK` de §4.3 y §4.4.
- **Política de migraciones:** aditivas siempre que sea posible; una migración destructiva exige respaldo previo del archivo SQLite (append-only preserva los hechos, ADR-04). El proceso interno de agregación y retención (muestras `P30D` → agregados `PT1H`, agregados `P10Y`, eventos indefinidos) no es una migración de esquema sino lógica de aplicación programada.
- **Pendiente asociado:** el tamaño máximo del archivo SQLite tras la agregación queda **PENDIENTE** (riesgo R-07); se valida antes de producción y no se fija en este documento.

Nota de tooling: EF Core y SQLite se nombran explícitamente porque son el stack real del proyecto (ADR-18), no productos del dominio fuente; la regla §4.4/§4.7 de no nombrar stacks del dominio fuente se refiere al dominio SAI, no al motor de persistencia de la solución.

---

## 6. Estrategia multi-tenant

**No aplica.** El sistema opera con un único administrador, un único host y un único SAI activo (§17 P.4, exclusión E-05). No hay partición por tenant, ni columna discriminadora de tenant, ni esquema o base por tenant, ni mecanismo de aislamiento entre inquilinos. La única columna discriminadora del modelo (`UnidadFisica.Tipo`) es de herencia TPH, no de tenancy. Si en el futuro se admitieran múltiples SAI simultáneos (exclusión E-02), el modelo de vínculos ya lo soporta (`MontajeBateria.posicion`, `CoberturaHost` por dispositivo) sin introducir multi-tenancy.

---

## 7. Trazabilidad

Cada tabla física traza a su entidad conceptual de origen (02, `Modelo-Conceptual-v1.0.md`) y a los CU que la consumen. No hay tabla sin origen conceptual.

| Tabla física | Entidad conceptual (02) | CU que la consumen |
| --- | --- | --- |
| Fabricante | 1.1 Fabricante | CU-02, CU-12 |
| ModeloDispositivo | 1.2 ModeloDispositivo | CU-02, CU-12 |
| ModeloBateria | 1.3 ModeloBateria | CU-02, CU-12 |
| TipoIntervencion | 1.4 TipoIntervencion | CU-08, CU-09, CU-11 |
| Proveedor | 1.5 Proveedor | CU-08, CU-09, CU-11 |
| UnidadFisica (Tipo='Host') | 1.6 UnidadFisica + 1.7 Host | CU-02, CU-09, CU-12 |
| UnidadFisica (Tipo='Dispositivo') | 1.6 UnidadFisica + 1.8 Dispositivo | CU-02, CU-08, CU-12 |
| UnidadFisica (Tipo='Bateria') | 1.6 UnidadFisica + 1.9 Bateria | CU-02, CU-08, CU-12 |
| MontajeBateria | 1.10 MontajeBateria | CU-02, CU-07, CU-08 |
| CoberturaHost | 1.11 CoberturaHost | CU-02, CU-09, CU-12 |
| FuenteDatos | 1.12 FuenteDatos | CU-04, CU-11 |
| SesionSondeo | 1.13 SesionSondeo | CU-02, CU-04 |
| Muestra | 1.14 Muestra | CU-04, CU-06 |
| Agregado | 1.15 Agregado | CU-06, CU-12 |
| ReglaDerivacion | 1.16 ReglaDerivacion | CU-04, CU-06 |
| Evento | 1.17 Evento | CU-04, CU-06 |
| PruebaBateria | 1.18 PruebaBateria | CU-07, CU-12 |
| Politica | 1.19 Politica | CU-03, CU-05 |
| VersionPolitica | 1.20 VersionPolitica | CU-03, CU-05 |
| Accion | 1.21 Accion | CU-05 |
| Verificacion | 1.22 Verificacion | CU-02, CU-05, CU-10 |
| Intervencion | 1.23 Intervencion | CU-08, CU-09, CU-11 |
| FichaVidaUtil | 1.26 FichaVidaUtil (1.24 en 02) | CU-08, CU-12 |
| VersionPoliticaVerificacion | Normalización N-N de VersionPolitica ⟷ Verificacion (1.20 ⟷ 1.22) | CU-05 |
| IntervencionBateria | Normalización N-N de Intervencion ⟷ Bateria (1.23 ⟷ 1.9) | CU-08 |
| AspNetUsers (Identity) | 1.27 Usuario administrador | CU-01 |
| *(owned columns `*Valor`/`*Origen`)* | 1.25 Valor con Origen | transversal a la historia |
| *(owned columns `*Monto`/`*Moneda`/`*Fecha`)* | 1.26 Dinero | CU-08, CU-11, CU-12 |

Las 27 entidades conceptuales de 02 quedan cubiertas: 22 como tablas (incluida la única `UnidadFisica` que absorbe el supertipo y sus tres especializaciones vía TPH, y `AspNetUsers` para el administrador), 2 como objetos de valor materializados en columnas (`Valor con Origen`, `Dinero`), más 2 tablas puente que normalizan los vínculos N-N del modelo conceptual.

## 8. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Modelo lógico inicial sobre SQLite + EF Core, derivado del modelo conceptual de 02 y de §17 P.4/P.10 del intake. Herencia TPH para UnidadFisica; procedencia por SesionSondeo; historia append-only; multi-tenant no aplica. |
