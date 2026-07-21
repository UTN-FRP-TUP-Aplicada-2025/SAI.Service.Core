# ADR-01 — Adopción de NUT como mecanismo de acceso al SAI

**Proyecto:** Sai-Service-Core
**Documento:** ADR-01-Adopcion-De-Nut-Como-Acceso-Al-Sai-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Comunicación

## 1. Contexto

El servicio necesita dialogar con el SAI para leer su estado en vivo, ordenar el apagado con retorno y disparar la prueba de batería. El equipo relevado (`0665:5161`, INNO TECH) habla un dialecto Megatec/Qx que no era evidente: el driver descartó `voltronic-qs` antes de acertar `voltronic-qs-hex`, y el espacio de comandos del protocolo incluye letras sueltas que cortan la energía. Construir la trama a mano reintroduce la clase de trampas de firmware que el driver ya conoce y modela. El monitoreo básico y el apagado ordenado ya están resueltos por NUT (`upsmon` + `upssched`) sobre el host, y la fuente es taxativa: no hace falta reconstruir eso. Motivan la decisión CU-04 (monitoreo en vivo) y CU-05 (ejecución del apagado), que dependen de un canal confiable hacia el equipo, y la capacidad F-02.

## 2. Decisión

Se accede al SAI exclusivamente a través de NUT (`nutdrv_qx` + `upsd`), consumido por el adaptador de conexión como cliente de `upsd` (TCP local) o invocando `upsc`. No se escribe un traductor de protocolo propio para el equipo actual (exclusión E-04). NUT provee las variables clave-valor que el adaptador mapea al dominio.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-01 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| NUT (`nutdrv_qx`) | El driver conoce el dialecto y las trampas de firmware; ya verificado sobre el equipo; comunidad madura | Dependencia de runtime externa al proceso; obliga a resolver la competencia por el nodo USB (ver ADR-19) |
| Trama Megatec construida a mano | Sin dependencia externa; control total del transporte | Reintroduce el riesgo de comandos que cortan energía sobre un dialecto no evidente; reescribe lo ya resuelto |
| Software del fabricante (ViewPower) | Producto listo | RCE sin parche; no expone panel ni API administrable; descartado por seguridad |

## 5. Consecuencias positivas

1. El camino de apagado se apoya en un driver que ya modela el comando con retorno y el dialecto correcto.
2. El dominio queda aislado del transporte a través del adaptador de conexión (ADR-02), de modo que NUT es reemplazable sin tocar la lógica.
3. Se evita reintroducir riesgos de firmware que la fuente documenta como resueltos y verificados.

## 6. Consecuencias negativas y trade-offs

1. Se acepta una dependencia de runtime sin la cual el servicio no cumple su función (T-07): NUT debe estar accesible dentro del contenedor o en el host.
2. Queda abierta la ubicación de NUT (contenedor vs host) y la competencia por el nodo USB, que se tratan en ADR-19.
3. Un cambio de versión de driver obliga a abrir una `SesionSondeo` nueva para preservar la procedencia de las muestras viejas.

## 7. Implementación

El adaptador NUT vive en `Infrastructure` e implementa el puerto de `Application` (ADR-02). Se conecta como cliente de `upsd` o invoca `upsc`. Cada `SesionSondeo` registra `driver`, `driverVersion`, `dialecto` y el mapa variable→origen. Versión mínima de NUT: 2.8.0 (relevada 2.8.1). El anclaje del dispositivo USB se resuelve en ADR-03.

## 8. Métricas de validación

- Prueba de conectividad exitosa en el alta de equipos (CU-02), con lectura de las variables esperadas del dialecto `Voltronic-QS-Hex 0.10`.
- Toda acción sobre el equipo validada por efecto observado (ADR-11), no por ausencia de error.
- Cero comandos enviados por fuera del contrato del adaptador.

## 9. Referencias

- Intake §17 P.1, P.3, P.11 (PA-01); §5.2 de la fuente.
- CU-04 Monitoreo en vivo del estado del SAI; CU-05 Ejecución del apagado ordenado ante corte.
- F-02, F-33; exclusión E-04.
- Riesgos R-05, R-08 (ubicación de NUT). ADR relacionadas: ADR-02, ADR-03, ADR-11, ADR-19.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-01. |
