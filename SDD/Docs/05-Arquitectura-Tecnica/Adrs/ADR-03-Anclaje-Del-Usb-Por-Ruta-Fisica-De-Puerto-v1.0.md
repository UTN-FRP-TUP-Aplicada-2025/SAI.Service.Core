# ADR-03 — Anclaje del USB por ruta física de puerto

**Proyecto:** Sai-Service-Core
**Documento:** ADR-03-Anclaje-Del-Usb-Por-Ruta-Fisica-De-Puerto-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Despliegue

## 1. Contexto

El servicio corre en un contenedor y necesita un binding estable al nodo USB del SAI. El equipo relevado no expone `iSerial`, así que no existe `/dev/serial/by-id`, y el nodo `hidraw` es volátil: cambia entre reinicios y reconexiones. Sin un anclaje estable, el contenedor perdería el dispositivo o tomaría otro periférico. Además, la fuente documenta desapariciones del bus USB en este modelo (O-U11, riesgo R-06) y la sustitución del SAI por otro equipo (CL-27). Motiva la decisión la necesidad de un despliegue reproducible del monitoreo (CU-04) y del apagado (CU-05).

## 2. Decisión

Se ancla el dispositivo USB por su ruta física de puerto (`ID_PATH=pci-0000:00:14.0-usb-0:3`) mediante una regla `udev`, y se comparte ese nodo con el contenedor. No se ancla por serial (imposible en este equipo) ni se mapea `/dev/bus/usb` entero.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-03 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Anclaje por ruta física de puerto (`udev`) | Estable frente a reinicios; «el SAI que esté enchufado ahí»; un reemplazo de equipo no rompe el binding | Depende de no mover el cable de puerto; requiere regla `udev` en el host |
| Anclaje por serial (`/dev/serial/by-id`) | Estable e independiente del puerto | Imposible: el equipo no tiene `iSerial` |
| Mapear `/dev/bus/usb` entero al contenedor | Simple de configurar | Expone otros periféricos del host al contenedor; superficie de riesgo innecesaria |

## 5. Consecuencias positivas

1. Binding reproducible del dispositivo entre reinicios del host y del contenedor.
2. Efecto lateral favorable: anclar por puerto significa «el SAI que esté enchufado ahí», así que la sustitución del equipo (CL-27) no rompe el binding.
3. El contenedor recibe solo el nodo del SAI, sin exponer otros periféricos.

## 6. Consecuencias negativas y trade-offs

1. Si alguien cambia el cable de puerto físico, el anclaje deja de resolver y hay que actualizar la regla `udev`.
2. La regla `udev` vive en el host, fuera de la imagen del contenedor: es configuración de despliegue que debe documentarse en 09.
3. No elimina la competencia por el nodo USB (R-05), que se decide en ADR-19; la complementa.

## 7. Implementación

Regla `udev` en el host que fija el nodo del dispositivo por `ID_PATH`. El contenedor de producción recibe ese nodo (device mapping por ruta física de puerto). En la sustitución del SAI (CU-09/CL-27), el panel dispara el procedimiento de recaracterización y las verificaciones de firmware vuelven a `NuncaVerificado`, pero el binding físico se preserva. Sin ambiente de staging: no hay a qué SAI conectarlo (P.8).

## 8. Métricas de validación

- El contenedor recupera el dispositivo tras un reinicio del host sin intervención manual.
- Detección de pérdida de comunicación operativa: 3 sondeos fallidos ⇒ `DesconexionUsb` (ADR-11, F-09).
- La sustitución de equipo no exige reconfigurar el device mapping.

## 9. Referencias

- Intake §17 P.1, P.8, P.11 (PA-03); riesgos R-05, R-06.
- CU-04 Monitoreo en vivo; CU-05 Apagado; CU-09 Reparación y sustitución del SAI; CL-14, CL-27.
- ADR relacionadas: ADR-01, ADR-11, ADR-19.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-03. |
