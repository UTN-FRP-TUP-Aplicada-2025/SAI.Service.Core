# ADR-05 — Vigencia como entidad con intervalo, no como atributo

**Proyecto:** Sai-Service-Core
**Documento:** ADR-05-Vigencia-Como-Entidad-Con-Intervalo-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Persistencia

## 1. Contexto

El sistema debe registrar qué batería estaba montada en qué SAI y qué SAI protegía al host en cada tramo de tiempo, incluyendo casos como «una batería se retira, se prueba en banco y se reinstala», «se mueve a otro SAI» o «un SAI se va a reparación y otro cubre el host» (CL-17). Si la vigencia fuera un atributo `desde`/`hasta` en la batería (modelo descartado C-1), el tercer caso *"ni siquiera es representable"*. Motivan la decisión las reglas conceptuales RC-02 (vigencia como entidad con intervalo), RC-03 (sucesión sin hueco) y RC-07 (resolución temporal de la batería), y los casos de uso CU-08 (recambio) y CU-09 (reparación/sustitución).

## 2. Decisión

La vigencia se modela como entidad con intervalo, no como atributo de la unidad física. Se definen los vínculos temporales `MontajeBateria` (batería↔dispositivo, con `posicion`) y `CoberturaHost` (dispositivo↔host), ambos con intervalo `desde`/`hasta` (con `hasta = null` mientras están abiertos). El `ResolutorTemporal` resuelve qué batería/dispositivo aplica en cada instante.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-05 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Vínculo temporal como entidad con intervalo | Representa recambios, movimientos entre SAI y cobertura suplente; reatribución automática del histórico | Un `ResolutorTemporal` que resolver correctamente; más entidades |
| Fechas `desde`/`hasta` como atributos de la batería (C-1) | Modelo más simple | El caso «un SAI cubre a otro» no es representable; rompe CL-17 |
| Snapshot del vínculo por muestra | Consulta directa sin resolver | Duplica constantes millones de veces; corregir una fecha exigiría migrar datos |

## 5. Consecuencias positivas

1. Los tres casos de CL-17 quedan representables como períodos, no como un único registro mutable.
2. Corregir la fecha de un recambio reatribuye automáticamente el histórico afectado sin migrar datos (CL-18), porque la historia guarda dispositivo e instante (ADR-07) y la batería se resuelve por `MontajeBateria`.
3. Habilita el cómputo de días sin protección y de disponibilidad de respaldo (CU-09, M-03).

## 6. Consecuencias negativas y trade-offs

1. Toda consulta de historia que necesite la batería pasa por el `ResolutorTemporal`: es una indirección obligatoria, no opcional.
2. La sucesión de vínculos debe respetar RC-03 (sin hueco no justificado): requiere validación de dominio.
3. Añade entidades y lógica de resolución que deben cubrirse con pruebas unitarias (P.6).

## 7. Implementación

Entidades `MontajeBateria` y `CoberturaHost` en `Domain` con intervalo `desde`/`hasta`. El `ResolutorTemporal` en `Domain`/`Application` resuelve la batería o el dispositivo vigente para un instante dado. Las escrituras de historia (ADR-04) guardan dispositivo e instante, nunca la batería directamente. `MontajeBateria` lleva `posicion` para soportar múltiples SAI en el futuro (E-02), aunque la implementación de v1 es de un SAI activo.

## 8. Métricas de validación

- Pruebas unitarias del `ResolutorTemporal` para los tres casos de CL-17.
- CL-18: corrección de fecha reatribuye sin migración.
- CU-09: días sin protección calculados correctamente en un escenario de sustitución.

## 9. Referencias

- Intake §17 P.4, P.11 (PA-05); CL-17, CL-18.
- RC-02 Vigencia como entidad con intervalo; RC-03 Sucesión de vínculos sin hueco; RC-07 Resolución temporal de la batería.
- CU-08 Recambio de batería y ficha de vida útil; CU-09 Reparación y sustitución del SAI. NB-04.
- ADR relacionadas: ADR-04, ADR-07.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-05. |
