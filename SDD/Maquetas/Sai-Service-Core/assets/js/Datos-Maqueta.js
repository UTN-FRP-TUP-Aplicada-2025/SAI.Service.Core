/* ==========================================================================
   Datos-Maqueta.js — Sai-Service-Core
   FUENTE UNICA de los datos de ejemplo de la maqueta. Ningun HTML hardcodea
   datos: los renderiza Maqueta.js desde este objeto global.

   Procedencia de los datos: SOLUTION-INTAKE-Sai-Service-Core-v1.0.md, Parte D
   §20, escenarios E-1..E-8 (verbatim). Donde una superficie necesita mostrar
   un campo que la documentacion NO ejemplifica, el valor se marca con
   sinDato:true y una etiqueta "sin dato de ejemplo" (ver §Ambiguedades del
   README y la devolucion al orquestador). No se inventan datos.
   ========================================================================== */
(function (global) {
  "use strict";

  var D = {};

  /* ----------------------------------------------------------------------
     Identidad de version (Design-Rules-Identidad-De-Version, contrato §4)
     versionLegible NO tiene ejemplo en la doc -> marcado sin dato de ejemplo.
     ---------------------------------------------------------------------- */
  D.version = {
    proyecto: "Sai-Service-Core",
    modeloUx: "Catalogo base + Blazor-MudBlazor + 4 extensiones",
    fechaIteracion: "2026-07-20",
    contrato: {
      versionLegible: "0.1.0-alpha.1",        // sin dato de ejemplo en la doc
      identificadorDeConstruccion: "build-local-sin-dato", // sin dato de ejemplo
      esPreliminar: true,                      // artefacto -alpha.N
      origenIndeterminado: true                // ejecucion local
    },
    sinDato: true
  };

  /* ----------------------------------------------------------------------
     Acceso monousuario: catalogo de codigos de resultado
     (Design-Rules-Acceso-Monousuario §4.2/§5). Texto unico por codigo;
     rechazo indiferenciado; sin exponer parametros de politica.
     ---------------------------------------------------------------------- */
  D.codigosResultado = {
    "ACC-RECHAZO":      { tipo: "danger",  texto: "No pudimos iniciar sesion con esos datos. Revisa el usuario y la contraseña e intenta de nuevo." },
    "ACC-RESTRINGIDO":  { tipo: "danger",  texto: "El acceso quedo restringido temporalmente. Espera un momento antes de volver a intentar." },
    "ACC-FORM-VENCIDO": { tipo: "danger",  texto: "El formulario expiro. Volve a cargar la pagina e intenta otra vez." },
    "ACC-IDENTIDAD-CREADA":   { tipo: "success", texto: "Identidad creada. Ya podes ingresar con el usuario y la contraseña que definiste." },
    "ACC-SECRETO-ACTUALIZADO":{ tipo: "success", texto: "Contraseña actualizada. La sesion anterior se cerro; ingresa con la contraseña nueva." },
    "ACC-SESION-VENCIDA":     { tipo: "warning", texto: "La sesion vencio. Volve a ingresar para continuar." }
  };

  /* ----------------------------------------------------------------------
     Usuario administrador (E-1 configuracionOperativa.usuarios)
     ---------------------------------------------------------------------- */
  D.usuario = { id: "usr-admin", nombre: "administrador" };

  /* ----------------------------------------------------------------------
     Alta-De-Equipos (§20.E-1): catalogo, inventario, candidato USB
     ---------------------------------------------------------------------- */
  D.altaEquipos = {
    candidatosUsb: [
      { vendorId: "0665", productId: "5161", descriptorFabricante: "INNO TECH",
        iSerial: null, marca: null, modelo: null,
        nota: "iSerial vacio; el descriptor USB no expone marca ni modelo" }
    ],
    sai: {
      marca: { valor: "Sin identificar", origen: "declarado" },
      modelo: { valor: "SAI linea interactiva 220V (modelo no identificado)", origen: "declarado" },
      topologia: { valor: "line-interactive", origen: "medido" },
      potenciaVaNominal: { valor: null, origen: "imputado",
        nota: "El equipo no la expone; ups.load es % de una nominal desconocida" },
      dialecto: "megatec/qx"
    },
    bateria: {
      numeroSerie: { valor: null, origen: "declarado", nota: "Anulable a proposito" },
      modelo: "12V 9Ah AGM VRLA",
      fechaCompra: "2024-11-15",
      fechaFabricacion: { valor: null, origen: "noCalculable",
        nota: "Sin fecha de fabricacion; la edad real no es calculable" }
    },
    host: { nombre: "i7infra", criticidad: "alta", fechaAlta: "2024-11-20" },
    verificacionesSembradas: 4 // las 4 arrancan en NuncaVerificado -> fuerza SoloAlerta
  };

  /* ----------------------------------------------------------------------
     Panel-Estado-En-Vivo (§20.E-2 muestra, §20.E-3/E-4 eventos, E-1 supuestos)
     ---------------------------------------------------------------------- */
  D.panelEstado = {
    sai: {
      estado: { texto: "En linea", origen: "medido", tipo: "success" }, // ups.status OL
      inputVoltage:  { valor: "232,9", unidad: "V", origen: "medido" },
      outputVoltage: { valor: "232,9", unidad: "V", origen: "medido" },
      upsLoad:       { valor: "12", unidad: "%", origen: "medido" }
    },
    bateria: {
      batteryVoltage: { valor: "13,41", unidad: "V", origen: "medido" },
      batteryCharge:  { valor: "100", unidad: "%", origen: "derivado",
        nota: "Interpolacion del driver sobre umbrales estimados. No es un valor medido (RN-05, CA-03)." },
      estado: { texto: "Flotacion", tipo: "success" }
    },
    conectividad: {
      sai: { texto: "OK", tipo: "success" },
      ultimoSondeo: "2 s",
      calidad: "completa",
      intervalo: "5 s"
    },
    supuestos: {
      verificados: 0,
      total: 4,
      modalidadEfectiva: { texto: "Solo aviso", tipo: "warning" }
    },
    eventosRecientes: [
      { hora: "18:30", tipo: "Microcorte", detalle: "5 s (±10 s)", regla: "rd-transicion-ups-status", version: 2 },
      { hora: "18:30", tipo: "RetornoRed", detalle: "—", regla: "rd-transicion-ups-status", version: 2 }
    ],
    // Estado politica-degradada / bloqueo (E-4)
    bloqueo: {
      motivo: "3 de 4 supuestos sin verificar. El apagado automatico esta degradado a solo aviso.",
      modalidadSolicitada: "Host luego equipo con retorno",
      modalidadEfectiva: "Solo aviso"
    },
    // Estado tension fuera de rango (E-4 transicion OL->OB, input 0,0 V)
    eventoTension: { hora: "04:15", tipo: "TensionFueraDeRango", detalle: "input.voltage 0,0 V sostenido", regla: "rd-tension-fuera-rango", version: 1 },
    // Orientacion posterior (estado vacio, sin equipos)
    orientacion: [
      { titulo: "Dar de alta los equipos", descripcion: "Descubri el SAI, la bateria y el host protegido.", destino: "Alta-De-Equipos.html" },
      { titulo: "Configurar la politica", descripcion: "Defini como reacciona el servicio ante un corte.", destino: "Configuracion-De-Politicas.html" },
      { titulo: "Ventana de mantenimiento", descripcion: "Verifica los supuestos para desbloquear el apagado.", destino: "Panel-De-Verificaciones.html" }
    ],
    // ultimo estado conocido con antiguedad (error sin conexion SAI)
    antiguedadUltimoEstado: "hace 45 s"
  };

  /* ----------------------------------------------------------------------
     Configuracion-De-Politicas: descriptores (fuente unica; la pantalla los
     lee, no hardcodea) + valores vigentes (E-1 vp-001) + preview (E-4 vp-003)
     ---------------------------------------------------------------------- */
  D.descriptores = [
    { parametro: "modalidad", etiqueta: "Modalidad de apagado", tipo: "seleccion", unidad: null,
      defecto: "SoloAlerta",
      enum: ["SoloAlerta", "SoloHost", "HostLuegoUpsConRetorno", "CicloForzado"],
      leyenda: "Que hace el servicio cuando el corte supera el umbral.",
      ejemplos: [ "CicloForzado → el corte no se cancela aunque vuelva la red" ],
      entidad: "VersionPolitica.modalidad", avanzado: false },
    { parametro: "umbralDisparoSegundos", etiqueta: "Umbral de disparo", tipo: "numerico", unidad: "s",
      defecto: 300, min: 30, max: 3600,
      leyenda: "Segundos de corte sostenido antes de disparar la accion.",
      ejemplos: [ "300 → dispara si el corte se sostiene 5 min" ],
      entidad: "VersionPolitica.umbralDisparo", avanzado: false },
    { parametro: "tiempoReservadoApagadoSeg", etiqueta: "Tiempo reservado para el apagado", tipo: "numerico", unidad: "s",
      defecto: null, min: 12, max: 540,
      leyenda: "Segundos reservados para completar el apagado. El maximo es el techo duro del equipo (I-10, RN-04).",
      ejemplos: [ "540 → usa todo el presupuesto del equipo" ],
      entidad: "VersionPolitica.tiempoReservadoApagado", avanzado: false, sinDatoDefecto: true },
    { parametro: "verificacionesRequeridas", etiqueta: "Verificaciones requeridas", tipo: "chips", unidad: null,
      defecto: [],
      opciones: ["ver-presupuesto-apagado", "ver-flag-ob", "ver-shutdown-return", "ver-bios-autoencendido"],
      leyenda: "Que supuestos exige la politica. Condicionan el bloqueo (RN-02).",
      ejemplos: [], entidad: "VersionPolitica.verificacionesRequeridas", avanzado: false },
    { parametro: "intervaloSondeoSegundos", etiqueta: "Intervalo de sondeo", tipo: "numerico", unidad: "s",
      defecto: 5, min: 1, max: 60,
      leyenda: "Cada cuanto se consulta el estado del equipo.",
      ejemplos: [ "5 → una lectura cada 5 s" ],
      entidad: "SesionSondeo.intervaloSegundos", avanzado: true }
  ];
  D.politica = {
    // Version vigente (E-1 vp-001)
    vigente: { id: "vp-001", version: 1, modalidad: "SoloAlerta", umbralDisparoSegundos: 300,
               tiempoReservadoApagadoSeg: null, verificacionesRequeridas: [] },
    // Propuesta de configuracion en preview (E-4 vp-003)
    propuesta: { modalidad: "HostLuegoUpsConRetorno", umbralDisparoSegundos: 300,
                 tiempoReservadoApagadoSeg: 240, verificacionesRequeridas: ["ver-presupuesto-apagado", "ver-flag-ob", "ver-shutdown-return", "ver-bios-autoencendido"] },
    presets: [
      { id: "solo-aviso", nombre: "Solo aviso" },
      { id: "apagado-retorno", nombre: "Apagado con retorno" },
      { id: "ciclo-forzado", nombre: "Ciclo forzado" }
    ],
    enPalabras: "Cuando el corte supere 300 s, el sistema apagara el host y cortara la salida del SAI con retorno; el corte no se cancela aunque vuelva la red.",
    alcance: "politica vigente vp-001 → nueva version inmutable"
  };

  /* ----------------------------------------------------------------------
     Prueba-De-Bateria (§20.E-5 prb-20260901 + §20.E-6 serie de tendencia)
     ---------------------------------------------------------------------- */
  D.pruebaBateria = {
    precondicion: { texto: "Tiempo minimo en flotacion cumplido", ok: true },
    enCurso: { progreso: 62, muestras: 42, perdidas: 3, cadencia: "1 Hz" },
    veredicto: {
      tendencia: "Se comporta peor que la linea base",
      confianza: { texto: "Baja", tipo: "warning" },
      comparable: { texto: "Si", tipo: "success" },
      caidaLineaBase: "-0,47 V",
      caidaActual: "-0,58 V",
      reserva: "Sin correccion por temperatura (sin sensor). No conforme a norma (IEEE 1188 no adquirida).",
      calculadoPor: "sai-service (el equipo no reporta veredicto de test)"
    },
    historial: [
      { fecha: "2025-03-01", caida: "-0,31 V", carga: "12 %", comparable: "Si", confianza: "baja" },
      { fecha: "2025-09-01", caida: "-0,35 V", carga: "13 %", comparable: "Si", confianza: "baja" },
      { fecha: "2026-03-01", caida: "-0,41 V", carga: "12 %", comparable: "Si", confianza: "baja" },
      { fecha: "2026-07-19", caida: "-0,47 V", carga: "13 %", comparable: "Si", confianza: "baja" },
      { fecha: "2026-09-01", caida: "-0,47 V", carga: "13 %", comparable: "Si", confianza: "baja" }
    ],
    // muestras perdidas en conmutacion (error)
    muestrasPerdidas: { total: 2, nota: "El equipo deja de atender consultas mientras conmuta a bateria; caen justo en el instante mas informativo." },
    // no comparable
    noComparable: { fecha: "2026-05-04", caida: "-0,52 V", carga: "31 %", motivo: "Carga concurrente fuera de tolerancia (deltaCargaConcurrente)" }
  };

  /* ----------------------------------------------------------------------
     Historicos-Y-Graficas (§20.E-2 agregado input.voltage, §20.E-7 calidad)
     ---------------------------------------------------------------------- */
  D.historicos = {
    periodo: { desde: "2026-07-19", hasta: "2026-07-19" },
    variables: ["input.voltage", "output.voltage", "ups.load", "microcortes"],
    // serie de muestras (P30D)
    muestras: {
      resolucion: "Muestras P30D",
      nMuestras: 718,
      eventos: [ { instante: "18:30", tipo: "Microcorte", regla: "rd-transicion-ups-status v2" } ],
      resumen: { promedio: "232,4 V", minimo: "229,8 V", maximo: "235,1 V", p95: "234,6 V" }
    },
    // serie de agregados (E-7 calidadSuministro)
    agregados: {
      resolucion: "Agregados PT1H",
      cobertura: 0.987,
      promedio: "228,4 V", minimo: "0,0 V", maximo: "241,2 V", p95: "236,1 V",
      muestrasAgregadas: 6307200,
      advertencia: "Serie construida sobre agregados horarios: los promedios NO representan microcortes. El conteo de microcortes viene de Evento, no de esta serie (RN-10)."
    },
    coberturaInsuficiente: { cobertura: 0.42, nota: "Cobertura por debajo de lo utilizable en el periodo elegido; no se oculta el hueco." },
    microcortesConteo: 31 // desde Evento (E-7)
  };

  /* ----------------------------------------------------------------------
     Panel-De-Verificaciones (§20.E-1 verificaciones + §20.E-4 ver-flag-ob)
     ---------------------------------------------------------------------- */
  D.verificaciones = {
    // estado vacio: 4 nunca verificados
    supuestos: [
      { id: "ver-presupuesto-apagado", supuesto: "El apagado completo del host cabe en 540 s", estado: "NuncaVerificado", metodo: "Cronometrado", vigenciaDias: 180 },
      { id: "ver-flag-ob", supuesto: "ups.status senala OB en un corte real", estado: "NuncaVerificado", metodo: "CorteControlado", vigenciaDias: 365 },
      { id: "ver-shutdown-return", supuesto: "El firmware ejecuta shutdown.return sin quedar apagado", estado: "NuncaVerificado", metodo: "PruebaFisica", vigenciaDias: null },
      { id: "ver-bios-autoencendido", supuesto: "BIOS reenciende el host tras restaurar la energia", estado: "NuncaVerificado", metodo: "PruebaFisica", vigenciaDias: 365 }
    ],
    // con datos: mezcla de estados (incluye ver-flag-ob verificado por E-4)
    supuestosMixtos: [
      { id: "ver-presupuesto-apagado", supuesto: "El apagado completo del host cabe en 540 s", estado: "Verificado", metodo: "Cronometrado", vigenciaDias: 180, venceEl: "2027-01-10" },
      { id: "ver-flag-ob", supuesto: "ups.status senala OB en un corte real", estado: "Verificado", metodo: "EvidenciaAcumulada", vigenciaDias: 365, venceEl: "2027-08-11", nota: "Verificado por evidencia en el corte real evt-20260811T041500" },
      { id: "ver-shutdown-return", supuesto: "El firmware ejecuta shutdown.return sin quedar apagado", estado: "Vencido", metodo: "PruebaFisica", vigenciaDias: null },
      { id: "ver-bios-autoencendido", supuesto: "BIOS reenciende el host tras restaurar la energia", estado: "NuncaVerificado", metodo: "PruebaFisica", vigenciaDias: 365 }
    ],
    supuestosDesbloqueado: [
      { id: "ver-presupuesto-apagado", supuesto: "El apagado completo del host cabe en 540 s", estado: "Verificado", metodo: "Cronometrado", vigenciaDias: 180, venceEl: "2027-01-10" },
      { id: "ver-flag-ob", supuesto: "ups.status senala OB en un corte real", estado: "Verificado", metodo: "EvidenciaAcumulada", vigenciaDias: 365, venceEl: "2027-08-11" },
      { id: "ver-shutdown-return", supuesto: "El firmware ejecuta shutdown.return sin quedar apagado", estado: "Verificado", metodo: "PruebaFisica", vigenciaDias: null },
      { id: "ver-bios-autoencendido", supuesto: "BIOS reenciende el host tras restaurar la energia", estado: "Verificado", metodo: "PruebaFisica", vigenciaDias: 365, venceEl: "2027-08-11" }
    ],
    // refutado (bloqueo permanente)
    supuestosRefutado: [
      { id: "ver-bios-autoencendido", supuesto: "BIOS reenciende el host tras restaurar la energia", estado: "Refutado", metodo: "PruebaFisica", vigenciaDias: 365, nota: "El host no arranco solo tras restaurar la energia. Bloqueo permanente hasta resolverlo." }
    ],
    stepper: [
      { n: 1, titulo: "Cronometrar el apagado del host bajo carga", supuesto: "ver-presupuesto-apagado" },
      { n: 2, titulo: "Observar ups.status = OB al cortar la red", supuesto: "ver-flag-ob" },
      { n: 3, titulo: "Ejecutar el shutdown.return controlado", supuesto: "ver-shutdown-return" },
      { n: 4, titulo: "Observar si el host arranca solo", supuesto: "ver-bios-autoencendido" }
    ]
  };

  /* ----------------------------------------------------------------------
     Registro-De-Intervenciones (§20.E-6 recambio + §20.E-8 fuente externa)
     ---------------------------------------------------------------------- */
  D.intervenciones = {
    formulario: {
      tipo: "Recambio de bateria", instante: "2026-09-05 10:30",
      dispositivo: "ups-01", bateriaSaliente: "bat-2024-a", bateriaEntrante: "bat-2026-a",
      proveedor: "Taller Electronica Sur (ficticio)", ejecutadaPor: "tecnico externo",
      tiempoRegistrado: "2026-09-08 21:14"
    },
    costos: {
      repuestos: { desc: "Bateria 12V 9Ah AGM", monto: "52.000", moneda: "ARS", fecha: "2026-09-05", usd: "41,00" },
      manoDeObra: { monto: "15.000", moneda: "ARS", fecha: "2026-09-05", usd: "11,80" },
      total: { monto: "67.000", moneda: "ARS", fecha: "2026-09-05", usd: "52,80" },
      cuadra: true
    },
    // estado error: costos no cuadran (E-8 caso 422)
    costosNoCuadran: { repuestos: "52.000", manoDeObra: "15.000", total: "60.000", esperado: "67.000",
      motivo: "total (60.000 ARS) ≠ Σ repuestos + mano de obra (67.000 ARS). Invariante Costos.cuadra()." },
    importeSinMoneda: { campo: "costos.total", motivo: "Dinero requiere 'moneda' y 'fecha'. Todo importe se registra con ambas (RN-07, I-18)." },
    coherenciaTemporal: { motivo: "bat-2024-a fue dada de baja el 2026-09-05; no puede recibir una intervencion fechada el 2026-11-01. Consultar su historial si es valido (I-5)." },
    hallazgos: [
      { codigo: "deformacion-carcasa", severidad: "alta", afecta: "bat-2024-a", detalle: "Abombamiento lateral visible." },
      { codigo: "sulfatacion-bornes", severidad: "baja", afecta: "ups-01", detalle: "Sulfatacion leve en borne positivo, removida." }
    ],
    mediciones: { antes: "12,71 V", despues: "13,44 V" },
    disposicion: { destino: "reciclado", receptor: "Taller Electronica Sur" },
    ficha: {
      bateria: "bat-2024-a",
      diasEnServicio: 654,
      aniosEnServicio: "1,79",
      cumplioExpectativa: { texto: "No", tipo: "danger", detalle: "-1,21 anios respecto del minimo de 3 declarado" },
      costoPorAnio: "37.430", moneda: "ARS",
      costoPorAnioUsd: "29,50", fuenteCotizacion: "BNA-divisa-venta"
    },
    historial: [
      { fecha: "2026-09-05", tipo: "Recambio de bateria", costo: "67.000 ARS", fuente: "local", confianza: "alta" },
      { fecha: "2026-10-02", tipo: "Inspeccion preventiva", costo: "12.000 ARS", fuente: "externa", confianza: "media",
        nota: "GMAO Corporativo v4; origen ApiExterna sin verificacion cruzada" }
    ]
  };

  /* ----------------------------------------------------------------------
     Sustitucion-Del-SAI (§20.E-1 parcial). R-11: el flujo NO tiene escenario
     de datos completo. La sucesion de coberturas de sustitucion esta
     RECONSTRUIDA para la maqueta (no proviene de la doc) y se marca como tal.
     ---------------------------------------------------------------------- */
  D.sustitucion = {
    coberturaVigente: { equipo: "ups-01", host: "i7infra", estado: { texto: "Activa", tipo: "success" } },
    equiposDisponibles: [ { id: "ups-02", estado: "En stock", nota: "Repuesto sin conectar (E-1)" } ],
    // reconstruida para la maqueta (R-11) — no hay E-9 aun
    sucesion: {
      reconstruida: true,
      tramos: [
        { equipo: "ups-01", desde: "2024-11-20", hasta: "2026-09-05", estado: { texto: "Cerrada", tipo: "neutral" } },
        { hueco: true, dias: 2, texto: "Hueco: 2 dias sin proteccion" },
        { equipo: "ups-02", desde: "2026-09-07", hasta: null, estado: { texto: "Vigente", tipo: "success" } }
      ]
    },
    hostSinCobertura: { desde: "2026-09-05", dias: 2 },
    suplenteActivo: { equipo: "ups-02", host: "i7infra" },
    enReparacion: { equipo: "ups-01" },
    avisoCaracterizacion: "El suplente es de otro modelo: las verificaciones de firmware vuelven a 'sin verificar'. Corresponde el procedimiento de caracterizacion."
  };

  /* ----------------------------------------------------------------------
     Informe-De-Periodo (§20.E-7 consulta + §20.E-6 comparacion de marcas)
     ---------------------------------------------------------------------- */
  D.informe = {
    parametros: { host: "i7infra", desde: "2026-01-01", hasta: "2026-12-31" },
    cobertura: {
      dispositivosActivos: "ups-01 (365 dias, 100%)",
      diasConProteccion: 365,
      diasSinProteccion: 0,
      bateriasIntervinientes: [
        { id: "bat-2024-a", dias: 247, estado: "DadoDeBaja" },
        { id: "bat-2026-a", dias: 118, estado: "EnServicio" }
      ]
    },
    intervencionesCostos: {
      porTipo: [ { tipo: "Recambio de bateria", cantidad: 1, costo: "67.000 ARS" } ],
      totalUsd: "52,80", fuenteCotizacion: "BNA-divisa-venta"
    },
    eventos: { Microcorte: 31, CorteSuministro: 2, DesconexionUsb: 1, TensionFueraDeRango: 6 },
    calidad: {
      fuente: "Agregado",
      cobertura: 0.987,
      promedio: "228,4 V", horasFueraDeRango: "14,2 h", disponibilidadRed: "0,9994",
      advertencia: "Calidad construida sobre agregados horarios. El conteo de microcortes sale de Evento, no del promedio (RN-10)."
    },
    // comparacion de marcas: con 1 sola ficha cerrada -> confianza baja
    comparacion: {
      modelos: [ { modelo: "mod-bat-12v9ah-agm", costoAnioUsd: "29,50", cumplio: "No", desvio: "-1,21 anios" } ],
      aviso: "1 ficha cerrada; se necesitan >= 2 modelos con ficha cerrada (o >= 4 pruebas comparables) para una comparacion concluyente. Confianza baja."
    },
    periodoSinDatos: { motivo: "El periodo elegido no tiene actividad registrada. No se arma un informe vacio como si fuera real." },
    agregadoSinCobertura: { motivo: "La calidad de suministro se serviria sin cobertura ni advertencia. La seccion no se sirve sin esos campos." }
  };

  /* ----------------------------------------------------------------------
     Contrato de campos: nombre, tipo, ejemplo, entidad de origen (modelo
     conceptual §2). Se exhibe en el index para validar el modelo de datos.
     ---------------------------------------------------------------------- */
  D.contratoCampos = [
    { nombre: "input.voltage", tipo: "tension (V), medido", ejemplo: "232,9 V", entidad: "Muestra" },
    { nombre: "battery.voltage", tipo: "tension (V), medido", ejemplo: "13,41 V", entidad: "Muestra" },
    { nombre: "battery.charge", tipo: "porcentaje, derivado", ejemplo: "100 % [derivado]", entidad: "Muestra (Valor con Origen)" },
    { nombre: "ups.load", tipo: "porcentaje, medido", ejemplo: "12 %", entidad: "Muestra" },
    { nombre: "ups.status", tipo: "enum OL/OB, medido", ejemplo: "OL", entidad: "Muestra" },
    { nombre: "cobertura", tipo: "fraccion [0..1]", ejemplo: "0,997", entidad: "Agregado" },
    { nombre: "duracionSegundos", tipo: "entero + incertidumbre", ejemplo: "5 s (±10 s)", entidad: "Evento" },
    { nombre: "reglaVersion", tipo: "entero", ejemplo: "2", entidad: "Evento / ReglaDerivacion" },
    { nombre: "modalidad", tipo: "enum", ejemplo: "SoloAlerta", entidad: "VersionPolitica" },
    { nombre: "umbralDisparoSegundos", tipo: "segundos", ejemplo: "300 s", entidad: "VersionPolitica" },
    { nombre: "tiempoReservadoApagadoSeg", tipo: "segundos (<=540)", ejemplo: "240 s", entidad: "VersionPolitica" },
    { nombre: "modalidadSolicitada / efectiva", tipo: "enum", ejemplo: "HostLuegoUpsConRetorno / SoloAlerta", entidad: "Accion" },
    { nombre: "estado (verificacion)", tipo: "enum", ejemplo: "NuncaVerificado / Verificado / Vencido / Refutado", entidad: "Verificacion" },
    { nombre: "vigenciaDias", tipo: "entero o null", ejemplo: "180 / 365 / sin caducidad", entidad: "Verificacion" },
    { nombre: "caidaV (prueba)", tipo: "tension (V), derivado", ejemplo: "-0,47 V", entidad: "PruebaBateria" },
    { nombre: "veredicto / confianza", tipo: "enum / enum", ejemplo: "SinDegradacionDetectable / baja", entidad: "PruebaBateria" },
    { nombre: "Dinero (monto, moneda, fecha)", tipo: "importe fechado", ejemplo: "67.000 ARS @ 2026-09-05", entidad: "Dinero" },
    { nombre: "equivalenteUsd", tipo: "importe derivado", ejemplo: "52,80 USD [BNA]", entidad: "Dinero" },
    { nombre: "diasEnServicio", tipo: "entero", ejemplo: "654", entidad: "FichaVidaUtil" },
    { nombre: "costoPorAnioDeServicio", tipo: "importe derivado", ejemplo: "37.430 ARS → 29,50 USD", entidad: "FichaVidaUtil" },
    { nombre: "desde / hasta (montaje)", tipo: "intervalo temporal", ejemplo: "2024-11-20 → 2026-09-05", entidad: "MontajeBateria" },
    { nombre: "desde / hasta (cobertura)", tipo: "intervalo temporal", ejemplo: "2024-11-20 → abierto", entidad: "CoberturaHost" },
    { nombre: "estado (unidad) / motivoBaja", tipo: "enum / texto", ejemplo: "DadoDeBaja / FinDeVidaUtil", entidad: "UnidadFisica" },
    { nombre: "confianza (fuente)", tipo: "enum", ejemplo: "alta (local) / media (externa)", entidad: "FuenteDatos" }
  ];

  /* ----------------------------------------------------------------------
     Superficies: nombre canonico, CU, archivo, chrome, descripcion, icono,
     y la lista de estados que cada una demuestra (para la barra de validacion)
     ---------------------------------------------------------------------- */
  D.superficies = [
    { id: "Alta-Inicial-Administrador", archivo: "Alta-Inicial-Administrador.html", cu: "CU-01 (alta inicial)", chrome: "acceso", icono: "user-plus",
      titulo: "Alta inicial del administrador",
      descripcion: "Aprovisionamiento del primer arranque: crear la unica identidad. Sin chrome ni cancelar.",
      estados: [
        { id: "cargando", nombre: "Cargando (resolviendo destino)" },
        { id: "datos", nombre: "Con datos (formulario listo)" },
        { id: "error-requisito", nombre: "Error (requisito de contraseña no cumplido)" },
        { id: "error-confirmacion", nombre: "Error (confirmacion no coincide)" },
        { id: "enviando", nombre: "Enviando" },
        { id: "fuera-de-tiempo", nombre: "Envio fuera de tiempo (redirige)" },
        { id: "aprovisionado", nombre: "Aprovisionado (redirige a login)" },
        { id: "vacio", nombre: "Vacio (N/A: es un acto unico)" }
      ], estadoInicial: "datos" },

    { id: "Acceso-Login", archivo: "Acceso-Login.html", cu: "CU-01 (ingreso)", chrome: "acceso", icono: "login",
      titulo: "Acceso al panel",
      descripcion: "Ingreso del operador unico. Sin registro, sin selector, sin recuperacion. Sello de version.",
      estados: [
        { id: "datos", nombre: "Con datos (listo para ingresar)" },
        { id: "cargando", nombre: "Cargando (enviando)" },
        { id: "error-rechazo", nombre: "Error (credenciales rechazadas)" },
        { id: "error-restringido", nombre: "Error (acceso restringido temporalmente)" },
        { id: "error-formulario", nombre: "Error (formulario vencido)" },
        { id: "confirmacion-creada", nombre: "Identidad recien creada" },
        { id: "confirmacion-secreto", nombre: "Contraseña actualizada" },
        { id: "sesion-expirada", nombre: "Sesion expirada" },
        { id: "vacio", nombre: "Vacio (N/A: formulario de acto)" }
      ], estadoInicial: "datos" },

    { id: "Panel-Estado-En-Vivo", archivo: "Panel-Estado-En-Vivo.html", cu: "CU-04 + CU-05", chrome: "trabajo", icono: "activity",
      titulo: "Panel de estado en vivo",
      descripcion: "Home del shell. Estado del SAI, bateria (carga derivada), conectividad, supuestos, eventos.",
      estados: [
        { id: "datos", nombre: "Con datos" },
        { id: "vacio", nombre: "Vacio (sin equipos, orientacion)" },
        { id: "cargando", nombre: "Cargando (skeleton)" },
        { id: "error-sin-conexion", nombre: "Error (sin conexion con el SAI)" },
        { id: "error-circuito", nombre: "Error (circuito del panel caido)" },
        { id: "politica-degradada", nombre: "Politica degradada a solo aviso" },
        { id: "tension-fuera-rango", nombre: "Tension fuera de rango" }
      ], estadoInicial: "datos" },

    { id: "Alta-De-Equipos", archivo: "Alta-De-Equipos.html", cu: "CU-02 (alta de equipos)", chrome: "trabajo", icono: "plug",
      titulo: "Alta de equipos",
      descripcion: "Descubrimiento del dispositivo, datos declarados del SAI, bateria y host. Siembra verificaciones.",
      estados: [
        { id: "datos", nombre: "Con datos (candidato / inventario)" },
        { id: "vacio", nombre: "Vacio (sin dispositivos)" },
        { id: "cargando", nombre: "Cargando (descubriendo / probando)" },
        { id: "error-conexion", nombre: "Error (prueba de conexion fallida)" },
        { id: "error-dato", nombre: "Error (dato obligatorio invalido)" },
        { id: "descubierto-sin-marca", nombre: "Descubierto sin marca ni modelo" }
      ], estadoInicial: "datos" },

    { id: "Configuracion-De-Politicas", archivo: "Configuracion-De-Politicas.html", cu: "CU-03 (politicas)", chrome: "trabajo", icono: "sliders",
      titulo: "Configuracion de politicas",
      descripcion: "Configuracion dirigida por esquema: descriptores, presets, simulacion, propuesta.",
      estados: [
        { id: "datos", nombre: "Con datos" },
        { id: "vacio", nombre: "Vacio (sin politica previa)" },
        { id: "cargando", nombre: "Cargando (descriptores)" },
        { id: "error-limite", nombre: "Error (valor fuera de limites)" },
        { id: "error-propuesta", nombre: "Error (propuesta rechazada por el sistema)" },
        { id: "simulacion", nombre: "Modo simulacion" },
        { id: "previsualizacion", nombre: "Propuesta en previsualizacion" }
      ], estadoInicial: "datos" },

    { id: "Prueba-De-Bateria", archivo: "Prueba-De-Bateria.html", cu: "CU-07 (prueba de bateria)", chrome: "trabajo", icono: "battery",
      titulo: "Prueba de bateria",
      descripcion: "Prueba densa a 1 Hz, veredicto con confianza y reserva, historial con comparabilidad.",
      estados: [
        { id: "datos", nombre: "Con datos (veredicto emitido)" },
        { id: "vacio", nombre: "Vacio (sin pruebas)" },
        { id: "cargando", nombre: "Cargando (prueba en curso)" },
        { id: "error-precondicion", nombre: "Error (precondicion no cumplida)" },
        { id: "error-muestras", nombre: "Error (muestras perdidas en conmutacion)" },
        { id: "no-comparable", nombre: "Prueba no comparable" }
      ], estadoInicial: "datos" },

    { id: "Historicos-Y-Graficas", archivo: "Historicos-Y-Graficas.html", cu: "CU-06 (historicos)", chrome: "trabajo", icono: "chart",
      titulo: "Historicos y graficas",
      descripcion: "Evolucion de variables por periodo. Distingue muestras de agregados con su cobertura.",
      estados: [
        { id: "datos-muestras", nombre: "Con datos (serie de muestras)" },
        { id: "datos-agregados", nombre: "Con datos (serie de agregados)" },
        { id: "vacio", nombre: "Vacio (sin datos en el periodo)" },
        { id: "cargando", nombre: "Cargando (serie)" },
        { id: "error-cobertura", nombre: "Error (cobertura insuficiente)" }
      ], estadoInicial: "datos-muestras" },

    { id: "Panel-De-Verificaciones", archivo: "Panel-De-Verificaciones.html", cu: "CU-10 + CU-05", chrome: "trabajo", icono: "shield-check",
      titulo: "Panel de verificaciones",
      descripcion: "Estado de los 4 supuestos y ventana de mantenimiento (stepper) por efecto observado.",
      estados: [
        { id: "vacio", nombre: "Vacio (4 nunca verificados)" },
        { id: "datos", nombre: "Con datos (estado de supuestos)" },
        { id: "cargando", nombre: "Cargando (paso en ejecucion)" },
        { id: "error-paso", nombre: "Error (paso fallido)" },
        { id: "refutado", nombre: "Supuesto refutado (bloqueo permanente)" },
        { id: "vencido", nombre: "Supuesto vencido" },
        { id: "ventana", nombre: "Ventana en curso (stepper)" },
        { id: "desbloqueado", nombre: "Desbloqueado (4 de 4)" }
      ], estadoInicial: "vacio" },

    { id: "Registro-De-Intervenciones", archivo: "Registro-De-Intervenciones.html", cu: "CU-08 (recambio)", chrome: "trabajo", icono: "wrench",
      titulo: "Registro de intervenciones",
      descripcion: "Intervencion con costos y cuadre, disposicion final, ficha de vida util, fuente local/externa.",
      estados: [
        { id: "datos", nombre: "Con datos" },
        { id: "vacio", nombre: "Vacio (sin intervenciones)" },
        { id: "cargando", nombre: "Cargando (validando / aplicando)" },
        { id: "error-cuadre", nombre: "Error (costos no cuadran)" },
        { id: "error-importe", nombre: "Error (importe sin moneda o fecha)" },
        { id: "error-coherencia", nombre: "Error (coherencia temporal)" },
        { id: "efecto-aplicado", nombre: "Efecto aplicado (ficha proyectada)" },
        { id: "fuente-externa", nombre: "Intervencion por fuente externa" }
      ], estadoInicial: "datos" },

    { id: "Sustitucion-Del-SAI", archivo: "Sustitucion-Del-SAI.html", cu: "CU-09 (sustitucion)", chrome: "trabajo", icono: "swap",
      titulo: "Reparacion y sustitucion del SAI",
      descripcion: "Cobertura vigente, sucesion de coberturas, dias sin proteccion, aviso de caracterizacion. Datos R-11 reconstruidos.",
      estados: [
        { id: "datos", nombre: "Con datos" },
        { id: "vacio", nombre: "Vacio (sin sucesion registrada)" },
        { id: "cargando", nombre: "Cargando (validando / aplicando)" },
        { id: "error-solapada", nombre: "Error (cobertura solapada)" },
        { id: "error-coherencia", nombre: "Error (coherencia temporal)" },
        { id: "host-sin-cobertura", nombre: "Host sin cobertura (alerta)" },
        { id: "suplente-activa", nombre: "Cobertura suplente activa" },
        { id: "en-reparacion", nombre: "SAI en reparacion" }
      ], estadoInicial: "datos" },

    { id: "Informe-De-Periodo", archivo: "Informe-De-Periodo.html", cu: "CU-12 (informe)", chrome: "trabajo", icono: "report",
      titulo: "Informe de periodo",
      descripcion: "Informe por periodo y comparacion de marcas por costo por anio normalizado a USD.",
      estados: [
        { id: "datos", nombre: "Con datos" },
        { id: "vacio", nombre: "Vacio (sin seleccion)" },
        { id: "cargando", nombre: "Cargando (intersecando intervalos)" },
        { id: "error-periodo", nombre: "Error (periodo sin datos suficientes)" },
        { id: "error-agregado", nombre: "Error (agregado sin cobertura)" },
        { id: "informe-advertencia", nombre: "Informe con advertencia de cobertura" },
        { id: "comparacion-confianza", nombre: "Comparacion con confianza baja" }
      ], estadoInicial: "datos" }
  ];

  D.porId = function (id) {
    for (var i = 0; i < D.superficies.length; i++) { if (D.superficies[i].id === id) return D.superficies[i]; }
    return null;
  };

  global.DatosMaqueta = D;
})(window);
