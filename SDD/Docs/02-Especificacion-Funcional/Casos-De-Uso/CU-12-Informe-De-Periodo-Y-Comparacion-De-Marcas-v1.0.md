# CU-12 — Informe de período y comparación de marcas

**Proyecto:** Sai-Service-Core
**Documento:** CU-12-Informe-De-Periodo-Y-Comparacion-De-Marcas-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que el administrador obtenga un informe de un período y una comparación de modelos de batería por costo por año de servicio normalizado, para decidir qué marca comprar la próxima vez. El informe responde, sin consultas especiales, qué dispositivos estuvieron activos, con qué cobertura, qué baterías intervinieron, qué intervenciones y costos hubo, qué eventos y qué calidad de suministro, todo intersecando intervalos.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador | Primario | Elige el período o el conjunto de modelos y lee el informe o la comparación |
| Servicio de informes | Sistema | Interseca los intervalos, agrega costos y eventos y arma el informe y la comparación |

## 3. Precondiciones

- El administrador tiene una sesión activa (CU-01).
- Existe historia en el período: coberturas, montajes, intervenciones, eventos, agregados y fichas de vida útil.

## 4. Flujo principal

1. El administrador elige un host y un período, o un conjunto de modelos de batería a comparar.
2. Para el informe de período, el servicio interseca los intervalos y devuelve: dispositivos activos con su cobertura, días con y sin protección, baterías intervinientes con sus intervalos recortados al período, intervenciones y costos por tipo, eventos por tipo y calidad de suministro.
3. El servicio incluye las baterías dadas de baja que intervinieron en el período; la baja lógica no las excluye de los informes históricos.
4. Cuando la calidad de suministro sale de agregados, el servicio adjunta la advertencia y la cobertura.
5. Para la comparación de marcas, el servicio agrupa las fichas de vida útil cerradas por modelo de batería y calcula el costo por año de servicio normalizado, junto con si cumplió la expectativa y el desvío.
6. El servicio devuelve todos los importes con moneda y fecha, y el equivalente normalizado marcado como derivado, con su fuente de cotización.

## 5. Flujos alternativos

- FA-1 Comparación con un solo modelo con ficha cerrada. Disparador: solo hay una ficha de vida útil cerrada. El servicio muestra ese modelo y advierte que la comparación necesita al menos dos modelos con ficha cerrada para ser concluyente.
- FA-2 Informe con tramo sin protección. Disparador: en el período hubo un tramo sin cobertura. El servicio reporta los días sin protección calculados del hueco entre coberturas.

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| PERIODO_SIN_DATOS | El período no tiene actividad registrada | Informa que no hay datos y no arma un informe vacío como si fuera real |
| AGREGADO_SIN_COBERTURA | La calidad de suministro se serviría sin cobertura ni advertencia | El sistema no la sirve sin esos campos (RN-10) |

## 7. Postcondiciones

- Éxito: existe un informe del período consistente, con las bajas incluidas y los importes con moneda y fecha; o una comparación de modelos por costo por año de servicio normalizado.
- Fallo: ante un período sin datos, no se presenta un informe engañoso.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | El período 2026 con un recambio de batería el 2026-09-05 | El administrador pide el informe del host `i7infra` | El informe muestra las baterías `bat-2024-a` y `bat-2026-a` con intervalos recortados que suman 365 días sin solapamiento |
| CA-02 | La batería `bat-2024-a` dada de baja que intervino en el período | El administrador consulta el informe | La batería aparece en el informe pese a estar dada de baja (RN-12) |
| CA-03 | Una calidad de suministro construida sobre agregados horarios con cobertura 0,987 | El administrador lee la sección de calidad de suministro | Aparece la cobertura 0,987 y la advertencia de que los promedios no representan microcortes (RN-10) |
| CA-04 | Una ficha de vida útil cerrada con costo por año de servicio de 37.430 pesos | El administrador compara modelos | El servicio muestra el costo por año de servicio normalizado a unos 29,50 dólares, marcado como derivado con su fuente de cotización (RN-07) |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-04 (Ciclo de vida del parque); NB-06 (comparación de marcas por desempeño) |
| Reglas de negocio aplicables | RN-05, RN-07, RN-10, RN-12 |
| Historias de usuario a generar | US-11 en 06 |
| Componentes esperados | Servicio de informes, resolutor temporal, agregación, proyección de fichas (referencia tentativa a 05) |
| Tests previstos | Intersección de intervalos, inclusión de bajas en informes, obligatoriedad de cobertura, comparación por costo por año de servicio normalizado (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancia de referencia: escenario §20.E-7 (consulta de período) y la ficha cerrada de §20.E-6.
- Comparar importes en pesos de años distintos no significa nada por la inflación; por eso todo importe lleva moneda y fecha y el equivalente normalizado viaja marcado como derivado.
- El sistema no presenta sus veredictos de salud como conformes a ninguna norma de pago no adquirida; toda cifra de comparación conserva su confianza y su reserva.
- El diseño de tablas y visualizaciones del informe es materia de 03-UX-UI-DX.

## 11. Interacción multiusuario y concurrencia

Trivial: un único administrador consulta informes.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-9 (§6), §20.E-6, §20.E-7, NB-04 y NB-06 |
