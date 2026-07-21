/* ==========================================================================
   Maqueta.js — Sai-Service-Core
   Render de datos (desde Datos-Maqueta.js), navegacion entre superficies,
   conmutacion de estados y barra de validacion con recarga automatica.
   Sin datos hardcodeados: todo sale de window.DatosMaqueta.
   ========================================================================== */
(function (global) {
  "use strict";

  var D = global.DatosMaqueta;

  /* --------------------------------------------------------------------
     Iconografia SVG inline (currentColor). Sin packs por CDN, sin raster.
     -------------------------------------------------------------------- */
  var P = { fill: 'none', s: 'stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"' };
  var ICONS = {
    activity: '<path d="M22 12h-4l-3 9L9 3l-3 9H2"/>',
    plug: '<path d="M9 2v6M15 2v6M7 8h10v3a5 5 0 0 1-10 0V8zM12 16v6"/>',
    sliders: '<path d="M4 21v-7M4 10V3M12 21v-9M12 8V3M20 21v-5M20 12V3M1 14h6M9 8h6M17 16h6"/>',
    battery: '<rect x="2" y="7" width="16" height="10" rx="2"/><path d="M22 11v2"/><path d="M6 10v4M9 10v4"/>',
    chart: '<path d="M3 3v18h18"/><path d="M7 14l3-3 3 2 4-5"/>',
    'shield-check': '<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/><path d="M9 12l2 2 4-4"/>',
    wrench: '<path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18v3h3l6.3-6.3a4 4 0 0 0 5.4-5.4l-2.5 2.5-2.5-2.5z"/>',
    swap: '<path d="M4 7h13l-3-3M20 17H7l3 3"/>',
    report: '<path d="M4 3h10l6 6v12H4z"/><path d="M14 3v6h6M8 13h8M8 17h8"/>',
    'user-plus': '<circle cx="9" cy="8" r="4"/><path d="M2 21v-1a6 6 0 0 1 12 0v1M18 8v6M21 11h-6"/>',
    login: '<path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4M10 17l5-5-5-5M15 12H3"/>',
    info: '<circle cx="12" cy="12" r="9"/><path d="M12 11v5M12 8h.01"/>',
    copy: '<rect x="9" y="9" width="12" height="12" rx="2"/><path d="M5 15V5a2 2 0 0 1 2-2h10"/>',
    alert: '<path d="M12 9v4M12 17h.01M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0z"/>',
    check: '<path d="M20 6 9 17l-5-5"/>',
    home: '<path d="M5 12H3l9-9 9 9h-2M5 12v7a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2v-7"/>',
    arrow: '<path d="M5 12h14M13 6l6 6-6 6"/>',
    inbox: '<path d="M3 12h5l2 3h4l2-3h5"/><path d="M5 4h14l3 8v6a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-6z"/>',
    flask: '<path d="M9 3h6M10 3v6l-5 9a2 2 0 0 0 2 3h10a2 2 0 0 0 2-3l-5-9V3"/>'
  };
  function icon(name, size) {
    var s = size || 20;
    var body = ICONS[name] || ICONS.info;
    return '<svg viewBox="0 0 24 24" width="' + s + '" height="' + s + '" ' + P.s + ' fill="none" aria-hidden="true">' + body + '</svg>';
  }
  function esc(v) {
    if (v === null || v === undefined) return '';
    return String(v).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }

  /* --------------------------------------------------------------------
     Componentes reutilizables
     -------------------------------------------------------------------- */
  function badge(texto, tipo) {
    return '<span class="mq-badge mq-badge--' + (tipo || 'neutral') + '">' + esc(texto) + '</span>';
  }
  function banda(tipo, htmlContenido, role) {
    var r = role ? ' role="' + role + '"' : '';
    return '<div class="mq-banda mq-banda--' + tipo + '"' + r + '>' + htmlContenido + '</div>';
  }
  function proc(origen) {
    if (!origen) return '';
    return '<span class="mq-proc">' + esc(origen) + '</span>';
  }
  function card(titulo, body) {
    var t = titulo ? '<h2 class="mq-card-titulo">' + esc(titulo) + '</h2>' : '';
    return '<section class="mq-card">' + t + body + '</section>';
  }
  function kv(pairs) {
    var s = '<dl class="mq-kv">';
    pairs.forEach(function (p) {
      s += '<dt>' + p[0] + '</dt><dd class="num">' + p[1] + '</dd>';
    });
    return s + '</dl>';
  }
  function tabla(headers, rows) {
    var s = '<div class="mq-scroll-x"><table class="mq-tabla"><thead><tr>';
    headers.forEach(function (h) { s += '<th scope="col">' + esc(h) + '</th>'; });
    s += '</tr></thead><tbody>';
    rows.forEach(function (r) {
      s += '<tr>';
      r.forEach(function (c) { s += '<td>' + c + '</td>'; });
      s += '</tr>';
    });
    return s + '</tbody></table></div>';
  }
  function vacio(iconName, titulo, texto, accionHtml) {
    return '<div class="mq-vacio">' + icon(iconName, 48) +
      '<h2>' + esc(titulo) + '</h2><p>' + esc(texto) + '</p>' + (accionHtml || '') + '</div>';
  }
  function cargando(nBloques) {
    var s = '<div class="mq-skeleton mq-skeleton--titulo"></div>';
    for (var i = 0; i < (nBloques || 3); i++) { s += '<div class="mq-card"><div class="mq-skeleton mq-skeleton--bloque"></div></div>'; }
    return '<div class="mq-grid">' + s + '</div>';
  }
  function h1(texto) { return '<h1 id="titulo-vista">' + esc(texto) + '</h1>'; }

  /* --------------------------------------------------------------------
     Sello de version + detalle de diagnostico (Identidad-De-Version)
     -------------------------------------------------------------------- */
  function selloHtml() {
    var c = D.version.contrato;
    var chip = c.esPreliminar ? ' ' + badge('preliminar', 'warning') : '';
    return '<span class="mq-sello-linea">v: <button type="button" class="mq-sello" ' +
      'data-accion="diagnostico" aria-haspopup="dialog">' + esc(c.versionLegible) + '</button>' + chip + '</span>';
  }
  function footerSello(extraClase) {
    return '<footer class="mq-footer ' + (extraClase || '') + '">' +
      '<span>' + esc(D.version.proyecto) + '</span><span>·</span>' +
      '<span>Modelo UX-UI: ' + esc(D.version.modeloUx) + '</span><span>·</span>' +
      '<span>Iteracion ' + esc(D.version.fechaIteracion) + '</span>' +
      '</footer>';
  }
  function diagnosticoTexto() {
    var c = D.version.contrato;
    return 'Sai-Service-Core — diagnostico de version\n' +
      'versionLegible: ' + c.versionLegible + '\n' +
      'identificadorDeConstruccion: ' + c.identificadorDeConstruccion + '\n' +
      'esPreliminar: ' + c.esPreliminar + '\n' +
      'origenIndeterminado: ' + c.origenIndeterminado + '\n' +
      'modeloUx: ' + D.version.modeloUx + '\n' +
      'iteracion: ' + D.version.fechaIteracion;
  }

  /* ====================================================================
     VISTAS por superficie: funcion(estado) -> HTML del contenido de main
     ==================================================================== */
  var V = {};

  /* ---- Alta-Inicial-Administrador (sin chrome, primer arranque) ---- */
  V['Alta-Inicial-Administrador'] = function (e) {
    var head = '<h1 id="titulo-vista">Crear la cuenta de administrador</h1>' +
      '<p class="mq-hint">Es la unica cuenta del sistema.</p>';
    var bandaHtml = '';
    if (e === 'error-requisito') bandaHtml = banda('danger', '<strong>Contraseña insuficiente.</strong> La contraseña no cumple la politica: usa al menos 12 caracteres con letras y numeros.', 'alert');
    if (e === 'error-confirmacion') bandaHtml = banda('danger', '<strong>Las contraseñas no coinciden.</strong> Volve a escribir la confirmacion para que sea igual a la contraseña.', 'alert');
    if (e === 'cargando') {
      return card(null, head +
        '<div class="progress my-3" role="progressbar" aria-label="Resolviendo destino"><div class="progress-bar progress-bar-striped progress-bar-animated" style="width:100%;background:var(--color-brand-primary)"></div></div>' +
        '<p class="mq-hint">Resolviendo si el sistema ya esta aprovisionado…</p>') + footerSello('mq-footer-acceso');
    }
    if (e === 'aprovisionado') {
      return card(null, head + banda('info', 'El sistema ya esta aprovisionado. Redirigiendo a <a href="Acceso-Login.html">Acceso al panel</a>…', 'status')) + footerSello('mq-footer-acceso');
    }
    if (e === 'fuera-de-tiempo') {
      return card(null, head + banda('info', 'El sistema se aprovisiono mientras cargabas. Redirigiendo de forma neutra a <a href="Acceso-Login.html">Acceso al panel</a>…', 'status')) + footerSello('mq-footer-acceso');
    }
    if (e === 'vacio') {
      return card(null, head + banda('info', 'Estado <strong>vacio: no aplica</strong>. Es un acto unico, no una lista; la superficie siempre presenta el formulario.', 'status') + formAlta(e)) + footerSello('mq-footer-acceso');
    }
    return card(null, head + bandaHtml + formAlta(e)) + footerSello('mq-footer-acceso');

    function formAlta(estado) {
      var enviando = estado === 'enviando';
      var invReq = estado === 'error-requisito' ? ' is-invalid' : '';
      var invConf = estado === 'error-confirmacion' ? ' is-invalid' : '';
      return '<form onsubmit="return false" novalidate>' +
        '<div class="mb-3"><label class="mq-label" for="ai-user">Usuario</label>' +
        '<input class="form-control" id="ai-user" autocomplete="username" value="' + esc(D.usuario.nombre) + '"></div>' +
        '<div class="mb-2"><label class="mq-label" for="ai-secreto">Contraseña</label>' +
        '<input type="password" class="form-control' + invReq + '" id="ai-secreto" autocomplete="new-password" aria-describedby="ai-req"></div>' +
        '<p class="mq-hint" id="ai-req">Requisito: al menos 12 caracteres, con letras y numeros.</p>' +
        '<div class="mb-3"><label class="mq-label" for="ai-conf">Repetir contraseña</label>' +
        '<input type="password" class="form-control' + invConf + '" id="ai-conf" autocomplete="new-password"></div>' +
        '<button class="btn btn-mq-primary w-100" ' + (enviando ? 'disabled' : '') + '>' +
        (enviando ? '<span class="spinner-border spinner-border-sm me-2"></span>Creando…' : 'Crear administrador') + '</button>' +
        '<div class="mt-3 text-center">' + selloHtml() + '</div></form>';
    }
  };

  /* ---- Acceso-Login (sin chrome) ---- */
  V['Acceso-Login'] = function (e) {
    var head = '<h1 id="titulo-vista">Iniciar sesion</h1><p class="mq-hint">Es la unica cuenta del sistema.</p>';
    var mapa = {
      'error-rechazo': 'ACC-RECHAZO', 'error-restringido': 'ACC-RESTRINGIDO', 'error-formulario': 'ACC-FORM-VENCIDO',
      'confirmacion-creada': 'ACC-IDENTIDAD-CREADA', 'confirmacion-secreto': 'ACC-SECRETO-ACTUALIZADO', 'sesion-expirada': 'ACC-SESION-VENCIDA'
    };
    var bandaHtml = '';
    if (mapa[e]) {
      var cod = D.codigosResultado[mapa[e]];
      var role = cod.tipo === 'success' ? 'status' : (cod.tipo === 'warning' ? 'status' : 'alert');
      bandaHtml = banda(cod.tipo, esc(cod.texto) + ' <span class="mq-proc">' + mapa[e] + '</span>', role);
    }
    if (e === 'vacio') bandaHtml = banda('info', 'Estado <strong>vacio: no aplica</strong>. Es un formulario de acto; siempre presenta los campos.', 'status');
    var enviando = e === 'cargando';
    var card1 = card(null, head + bandaHtml +
      '<form onsubmit="return false" novalidate>' +
      '<div class="mb-3"><label class="mq-label" for="lg-user">Usuario</label>' +
      '<input class="form-control" id="lg-user" autocomplete="username" value="' + esc(D.usuario.nombre) + '"></div>' +
      '<div class="mb-3"><label class="mq-label" for="lg-secreto">Contraseña</label>' +
      '<input type="password" class="form-control" id="lg-secreto" autocomplete="current-password"></div>' +
      '<button class="btn btn-mq-primary w-100" ' + (enviando ? 'disabled' : '') + '>' +
      (enviando ? '<span class="spinner-border spinner-border-sm me-2"></span>Ingresando…' : 'Ingresar') + '</button>' +
      '<div class="mt-3 text-center">' + selloHtml() + '</div></form>');
    var omis = '<p class="mq-omisiones">sin registro · sin recordarme · sin recuperacion · sin selector de cuentas</p>';
    return card1 + omis + footerSello('mq-footer-acceso');
  };

  /* ---- Panel-Estado-En-Vivo ---- */
  V['Panel-Estado-En-Vivo'] = function (e) {
    var d = D.panelEstado;
    var head = h1('Panel de estado en vivo');
    if (e === 'cargando') return head + cargando(4);
    if (e === 'vacio') {
      var tarjetas = '<div class="mq-grid">';
      d.orientacion.forEach(function (o) {
        tarjetas += '<a class="mq-tarjeta-acceso-index" href="' + o.destino + '"><span class="mq-ico">' + icon('arrow', 22) + '</span>' +
          '<h2>' + esc(o.titulo) + '</h2><p>' + esc(o.descripcion) + '</p></a>';
      });
      tarjetas += '</div>';
      return head + banda('info', 'Sistema aprovisionado y con sesion, pero <strong>sin equipos dados de alta</strong>. Estos son los proximos pasos sugeridos (orientacion posterior, no un asistente obligatorio).', 'status') + tarjetas;
    }
    // banner de bloqueo condicional
    var banner = '';
    if (e === 'politica-degradada') {
      banner = banda('warning', '<strong>Politica degradada a solo aviso.</strong> ' + esc(d.bloqueo.motivo) +
        ' Modalidad solicitada: ' + esc(d.bloqueo.modalidadSolicitada) + '; efectiva: ' + esc(d.bloqueo.modalidadEfectiva) + '. ' +
        '<a href="Panel-De-Verificaciones.html">Ir a verificar →</a>', 'alert');
    } else if (d.supuestos.verificados < d.supuestos.total) {
      banner = banda('warning', '<strong>' + d.supuestos.verificados + ' de ' + d.supuestos.total + ' supuestos verificados.</strong> El apagado automatico esta en solo aviso. <a href="Panel-De-Verificaciones.html">Ir a verificar →</a>', 'alert');
    }

    var estadoSai = card('Estado del SAI',
      '<div class="mb-2">' + badge(d.sai.estado.texto, d.sai.estado.tipo) + '</div>' +
      kv([
        ['input.voltage', '<span class="num">' + d.sai.inputVoltage.valor + ' ' + d.sai.inputVoltage.unidad + '</span>' + proc(d.sai.inputVoltage.origen)],
        ['output.voltage', '<span class="num">' + d.sai.outputVoltage.valor + ' ' + d.sai.outputVoltage.unidad + '</span>' + proc(d.sai.outputVoltage.origen)],
        ['ups.load', '<span class="num">' + d.sai.upsLoad.valor + ' ' + d.sai.upsLoad.unidad + '</span>' + proc(d.sai.upsLoad.origen)]
      ]));

    var conectividad = d.conectividad;
    var estadoConec;
    if (e === 'error-sin-conexion') {
      estadoConec = card('Conectividad', banda('danger', '<strong>Sin conexion con el SAI.</strong> 3 sondeos consecutivos sin respuesta. El resto del panel muestra el ultimo estado conocido (' + esc(d.antiguedadUltimoEstado) + ').', 'alert'));
    } else {
      estadoConec = card('Conectividad', kv([
        ['SAI', badge(conectividad.sai.texto, conectividad.sai.tipo)],
        ['ultimo sondeo', conectividad.ultimoSondeo],
        ['calidad de muestra', conectividad.calidad],
        ['intervalo', conectividad.intervalo]
      ]));
    }

    var bateria = card('Bateria', kv([
      ['battery.voltage', '<span class="num">' + d.bateria.batteryVoltage.valor + ' ' + d.bateria.batteryVoltage.unidad + '</span>' + proc(d.bateria.batteryVoltage.origen)],
      ['battery.charge', '<span class="num">~ ' + d.bateria.batteryCharge.valor + ' ' + d.bateria.batteryCharge.unidad + '</span>' + proc(d.bateria.batteryCharge.origen)],
      ['estado', badge(d.bateria.estado.texto, d.bateria.estado.tipo)]
    ]) + '<p class="mq-hint">La carga de bateria siempre se marca como <strong>derivada</strong> (RN-05, CA-03): no construir conclusiones sobre un valor interpolado.</p>');

    var supuestos = card('Supuestos', kv([
      ['verificados', '<span class="num">' + d.supuestos.verificados + ' de ' + d.supuestos.total + '</span>'],
      ['modalidad efectiva', badge(d.supuestos.modalidadEfectiva.texto, d.supuestos.modalidadEfectiva.tipo)]
    ]) + '<a class="btn btn-mq-secondary btn-mq-pill btn-sm mt-2" href="Panel-De-Verificaciones.html">Ir a verificar</a>');

    var eventos = d.eventosRecientes.slice();
    if (e === 'tension-fuera-rango') eventos = [d.eventoTension].concat(eventos);
    var filasEv = eventos.map(function (ev) {
      return [ev.hora, esc(ev.tipo), '<span class="num">' + esc(ev.detalle) + '</span>', esc(ev.regla) + ' v' + ev.version];
    });
    var eventosCard = card('Eventos recientes', tabla(['Hora', 'Evento', 'Detalle', 'Regla'], filasEv));

    var avisoCircuito = '';
    if (e === 'error-circuito') {
      avisoCircuito = banda('info', '<strong>Se corto el transporte del panel con el servidor.</strong> Aviso de reconexion no bloqueante; el historico local no se pierde; reconexion automatica en curso.', 'status');
    }

    return head + banner + avisoCircuito + '<div class="mq-grid">' + estadoSai + estadoConec + bateria + supuestos + '</div>' +
      '<div class="mt-3">' + eventosCard + '</div>';
  };

  /* ---- Alta-De-Equipos ---- */
  V['Alta-De-Equipos'] = function (e) {
    var d = D.altaEquipos;
    var head = h1('Alta de equipos');
    if (e === 'cargando') return head + card('Descubrimiento del dispositivo', '<div class="mq-skeleton mq-skeleton--bloque"></div><p class="mq-hint">Descubriendo dispositivos USB / probando conexion…</p>');

    var descubrimiento;
    if (e === 'vacio') {
      descubrimiento = card('Descubrimiento del dispositivo', vacio('plug', 'Sin dispositivos', 'Todavia no se ejecuto el descubrimiento o no hay candidatos USB conectados.', '<button class="btn btn-mq-primary btn-sm">Descubrir</button>'));
    } else {
      var c = d.candidatosUsb[0];
      var etq = (e === 'descubierto-sin-marca')
        ? ' ' + badge('sin marca ni modelo', 'warning')
        : '';
      descubrimiento = card('Descubrimiento del dispositivo',
        '<div class="mb-2"><button class="btn btn-mq-secondary btn-sm">Descubrir</button></div>' +
        tabla(['Candidato USB', 'Fabricante', 'iSerial', ''], [[
          '<span class="num">' + esc(c.vendorId) + ':' + esc(c.productId) + '</span>' + etq,
          esc(c.descriptorFabricante),
          '<em>vacio</em>',
          '<button class="btn btn-mq-secondary btn-sm">Seleccionar</button>'
        ]]) +
        (e === 'descubierto-sin-marca' ? '<p class="mq-hint">El adaptador no devolvio marca ni modelo: se piden a mano con procedencia <strong>declarado</strong>.</p>' : ''));
    }

    var errDato = (e === 'error-dato');
    var sai = card('Datos declarados del SAI',
      '<div class="row g-3">' +
      campo('Marca', 'text', d.sai.marca.valor, d.sai.marca.origen) +
      campo('Modelo', 'text', d.sai.modelo.valor, d.sai.modelo.origen) +
      campo('Potencia nominal (VA)', 'text', '', d.sai.potenciaVaNominal.origen, errDato, 'Desconocida = vacio con procedencia imputado; nunca un numero inventado (RN-05).') +
      '</div>' +
      (errDato ? banda('danger', '<strong>Dato obligatorio invalido.</strong> Vida de flotacion sin temperatura de referencia (RN-13), o dato mal formado. Corregi el campo marcado.', 'alert') : ''));

    var bat = card('Bateria montada',
      '<div class="row g-3">' +
      campo('Numero de serie', 'text', '', d.bateria.numeroSerie.origen, false, 'Anulable: muchas unidades no lo exponen.') +
      campo('Modelo', 'text', d.bateria.modelo, '') +
      campo('Fecha de compra', 'date', d.bateria.fechaCompra, '') +
      campo('Fecha de fabricacion', 'date', '', d.bateria.fechaFabricacion.origen, false, 'Fabricacion anterior a compra es normal; sin fecha, la edad real no es calculable.') +
      '</div>');

    var host = card('Host protegido',
      '<div class="row g-3">' +
      campo('Nombre', 'text', d.host.nombre, '') +
      '<div class="col-md-6"><label class="mq-label" for="ap-crit">Criticidad</label>' +
      '<select class="form-select" id="ap-crit"><option>alta</option><option>media</option><option>baja</option></select></div>' +
      '</div>');

    var pie;
    if (e === 'error-conexion') {
      pie = banda('danger', '<strong>Prueba de conexion fallida.</strong> El equipo no responde a la prueba. Validado por efecto observado, no por ausencia de error (RN-03). Revisa la conexion y volve a probar.', 'alert') +
        '<div class="d-flex gap-2"><button class="btn btn-mq-secondary">Probar conexion</button><button class="btn btn-mq-primary ms-auto" disabled>Dar de alta</button></div>';
    } else {
      pie = '<div class="d-flex gap-2"><button class="btn btn-mq-secondary">Probar conexion</button><button class="btn btn-mq-primary ms-auto">Dar de alta</button></div>';
    }

    return head + descubrimiento + '<div class="mt-3">' + sai + '</div><div class="mt-3">' + bat + '</div><div class="mt-3">' + host + '</div>' +
      '<div class="mt-3">' + card(null, '<p class="mq-hint">Al dar de alta se abren los vinculos con <code>hasta = null</code> y se siembran las 4 verificaciones en «nunca verificado», lo que fuerza el modo solo aviso.</p>' + pie) + '</div>';

    function campo(label, tipo, valor, origen, invalido, hint) {
      var id = 'ap-' + label.replace(/[^a-z]/gi, '').toLowerCase();
      return '<div class="col-md-6"><label class="mq-label" for="' + id + '">' + esc(label) + (origen ? proc(origen) : '') + '</label>' +
        '<input type="' + tipo + '" class="form-control' + (invalido ? ' is-invalid' : '') + '" id="' + id + '" value="' + esc(valor) + '"' +
        (hint ? ' aria-describedby="' + id + '-h"' : '') + '>' +
        (hint ? '<p class="mq-hint" id="' + id + '-h">' + esc(hint) + '</p>' : '') + '</div>';
    }
  };

  /* ---- Configuracion-De-Politicas (config dirigida por esquema) ---- */
  V['Configuracion-De-Politicas'] = function (e) {
    var head = h1('Configuracion de politicas');
    if (e === 'cargando') return head + cargando(4);

    var chip = (e === 'simulacion' || e === 'previsualizacion') ? ' ' + badge('Modo simulacion', 'warning') : '';
    var cabecera = '<div class="d-flex align-items-center mb-3"><span class="me-2">Estado de edicion:</span>' + (chip || badge('Sin cambios', 'neutral')) + '</div>';

    // presets
    var presets = '<div class="mb-3"><span class="mq-label">Presets</span><div class="d-flex flex-wrap gap-2">';
    D.politica.presets.forEach(function (p) { presets += '<button class="btn btn-mq-secondary btn-mq-pill btn-sm">' + esc(p.nombre) + '</button>'; });
    presets += '</div></div>';

    // campos dirigidos por descriptor (fuente unica)
    var comunes = '', avanzados = '';
    D.descriptores.forEach(function (des) {
      var vig = D.politica.vigente[des.parametro];
      var html = campoDescriptor(des, vig, e);
      if (des.avanzado) avanzados += html; else comunes += html;
    });

    var enPalabras = card('En palabras',
      '<p>«' + esc(D.politica.enPalabras) + '»</p>' +
      '<p class="mq-hint">Texto generado desde los descriptores + valores; se regenera al cambiar un valor, no se escribe a mano.</p>');

    var ranura = '<div class="mt-3 mq-ranura-asistente" aria-disabled="true">' + icon('info', 20) +
      '<div><strong>Asistente (proximamente)</strong> ' + badge('deshabilitado', 'neutral') +
      '<div class="mq-hint">Ranura reservada para el futuro asistente de IA. No realiza ninguna accion.</div></div></div>';

    var pie;
    if (e === 'previsualizacion') {
      pie = card('Previsualizacion de la propuesta',
        banda('info', 'Alcance afectado: <strong>' + esc(D.politica.alcance) + '</strong>. La UI propone, el humano confirma, el sistema valida.', 'status') +
        '<p>«' + esc(D.politica.enPalabras) + '»</p>' +
        '<div class="d-flex gap-2"><button class="btn btn-mq-secondary">Descartar</button><button class="btn btn-mq-primary ms-auto">Confirmar version</button></div>');
    } else if (e === 'error-propuesta') {
      pie = banda('danger', '<strong>Propuesta rechazada por el sistema.</strong> La validacion del motor rechazo la propuesta al confirmar: invariante de politica violado. La propuesta no se aplica.', 'alert') +
        '<div class="d-flex gap-2"><button class="btn btn-mq-secondary">Descartar</button><button class="btn btn-mq-primary ms-auto">Confirmar version</button></div>';
    } else {
      pie = '<div class="d-flex gap-2 mt-3"><button class="btn btn-mq-secondary">Descartar</button><button class="btn btn-mq-primary ms-auto">Confirmar version</button></div>';
    }

    var vacioAviso = (e === 'vacio') ? banda('info', 'No hay version de politica todavia. Los campos vienen precargados con los <strong>default</strong> de los descriptores.', 'status') : '';

    return head + card(null, cabecera + vacioAviso + presets + comunes +
      '<details class="mb-2"><summary class="btn btn-mq-secondary btn-sm">Opciones avanzadas</summary><div class="mt-3">' + avanzados + '</div></details>') +
      '<div class="mt-3">' + enPalabras + '</div>' + ranura + '<div class="mt-3">' + pie + '</div>';

    function campoDescriptor(des, valorVigente, estado) {
      var id = 'cfg-' + des.parametro;
      var ayuda = '<button type="button" class="btn btn-sm btn-mq-secondary btn-mq-pill ms-1" aria-label="Ayuda de ' + esc(des.etiqueta) + '" title="' + esc(des.leyenda) + (des.ejemplos.length ? ' — ' + esc(des.ejemplos[0]) : '') + '">i</button>';
      var control;
      var invalido = (estado === 'error-limite' && des.parametro === 'tiempoReservadoApagadoSeg');
      if (des.tipo === 'seleccion') {
        control = '<select class="form-select" id="' + id + '">' + des.enum.map(function (o) {
          return '<option ' + (o === valorVigente ? 'selected' : '') + '>' + esc(o) + '</option>';
        }).join('') + '</select>';
      } else if (des.tipo === 'chips') {
        control = '<div class="d-flex flex-wrap gap-2">' + des.opciones.map(function (o) {
          var sel = (valorVigente || []).indexOf(o) >= 0;
          return '<button type="button" class="btn btn-sm ' + (sel ? 'btn-mq-primary' : 'btn-mq-secondary') + ' btn-mq-pill" aria-pressed="' + sel + '">' + esc(o) + '</button>';
        }).join('') + '</div>';
      } else {
        var val = (valorVigente === null || valorVigente === undefined) ? '' : valorVigente;
        var ph = des.sinDatoDefecto ? 'placeholder="sin dato de ejemplo" ' : '';
        control = '<input type="number" class="form-control' + (invalido ? ' is-invalid' : '') + '" id="' + id + '" value="' + esc(val) + '" ' + ph +
          'min="' + des.min + '" max="' + des.max + '" aria-describedby="' + id + '-h">';
      }
      var hintDefecto = des.tipo === 'numerico'
        ? 'por defecto ' + (des.defecto === null ? '[derivado — sin dato de ejemplo]' : des.defecto) + '; entre ' + des.min + ' y ' + des.max + (des.parametro === 'tiempoReservadoApagadoSeg' ? ' (techo duro del equipo)' : '')
        : (des.tipo === 'seleccion' ? 'por defecto ' + des.defecto : des.leyenda);
      var err = invalido ? '<p class="mq-hint" style="color:var(--color-text-danger)">Fuera de limites: el maximo admitido es ' + des.max + ' s (techo duro del equipo, I-10/RN-04).</p>' : '';
      return '<div class="mb-3"><label class="mq-label" for="' + id + '">' + esc(des.etiqueta) + (des.unidad ? ' (' + des.unidad + ')' : '') + ayuda + '</label>' +
        control + '<p class="mq-hint" id="' + id + '-h">' + esc(hintDefecto) + '</p>' + err + '</div>';
    }
  };

  /* ---- Prueba-De-Bateria ---- */
  V['Prueba-De-Bateria'] = function (e) {
    var d = D.pruebaBateria;
    var head = h1('Prueba de bateria');
    var precon = card(null, '<div class="d-flex align-items-center gap-3"><span>Precondicion: ' + esc(d.precondicion.texto) + '</span>' +
      badge(d.precondicion.ok && e !== 'error-precondicion' ? 'ok' : 'no', d.precondicion.ok && e !== 'error-precondicion' ? 'success' : 'danger') +
      '<button class="btn btn-mq-primary ms-auto" ' + (e === 'error-precondicion' || e === 'cargando' ? 'disabled' : '') + '>Lanzar prueba</button></div>');

    if (e === 'error-precondicion') {
      return head + precon + banda('danger', '<strong>Precondicion no cumplida.</strong> Tiempo minimo en flotacion no alcanzado (por ejemplo, tras un corte reciente). La accion queda deshabilitada con su motivo hasta que se cumpla.', 'alert');
    }
    if (e === 'vacio') {
      return head + precon + card('Historial de pruebas', vacio('flask', 'Sin pruebas registradas', 'Todavia no hay pruebas. Se muestra la linea base y la accion de lanzar.', ''));
    }
    if (e === 'cargando') {
      return head + precon + card('Prueba en curso (' + esc(d.enCurso.cadencia) + ')',
        '<div class="progress mb-2" role="progressbar" aria-label="Progreso de la prueba"><div class="progress-bar" style="width:' + d.enCurso.progreso + '%;background:var(--color-brand-primary)"></div></div>' +
        kv([['muestras tomadas', d.enCurso.muestras], ['muestras perdidas', d.enCurso.perdidas]]));
    }

    var v = d.veredicto;
    var veredicto = card('Veredicto',
      kv([
        ['tendencia', esc(v.tendencia)],
        ['confianza', badge(v.confianza.texto, v.confianza.tipo)],
        ['comparable', badge(v.comparable.texto, v.comparable.tipo)],
        ['caida vs linea base', '<span class="num">' + esc(v.caidaLineaBase) + ' → ' + esc(v.caidaActual) + '</span>']
      ]) +
      '<p class="mq-hint">Reserva: ' + esc(v.reserva) + '</p>' +
      '<p class="mq-hint">Calculado por: ' + esc(v.calculadoPor) + '.</p>');

    var errMuestras = '';
    if (e === 'error-muestras') {
      errMuestras = banda('warning', '<strong>Muestras perdidas en la conmutacion.</strong> ' + esc(d.muestrasPerdidas.total) + ' muestras perdidas: ' + esc(d.muestrasPerdidas.nota) + ' Los derivados toleran nulos por variable, no rompen.', 'status');
    }

    var filas = d.historial.map(function (h) {
      return [h.fecha, '<span class="num">' + esc(h.caida) + '</span>', '<span class="num">' + esc(h.carga) + '</span>', esc(h.comparable), esc(h.confianza)];
    });
    if (e === 'no-comparable') {
      var nc = d.noComparable;
      filas.push(['<strong>' + nc.fecha + '</strong>', '<span class="num">' + esc(nc.caida) + '</span>', '<span class="num">' + esc(nc.carga) + '</span>', badge('No comparable', 'warning'), '—']);
    }
    var historial = card('Historial de pruebas',
      tabla(['Fecha', 'Caida', 'Carga', 'Comparable', 'Confianza'], filas) +
      (e === 'no-comparable' ? '<p class="mq-hint">' + esc(d.noComparable.motivo) + '. Una prueba no comparable se registra pero no entra en la tendencia (CL-26, I-16).</p>' : ''));

    return head + precon + errMuestras + veredicto + '<div class="mt-3">' + historial + '</div>';
  };

  /* ---- Historicos-Y-Graficas ---- */
  V['Historicos-Y-Graficas'] = function (e) {
    var d = D.historicos;
    var head = h1('Historicos y graficas');
    var controles = card(null,
      '<div class="row g-3 align-items-end">' +
      '<div class="col-auto"><label class="mq-label" for="hg-desde">Desde</label><input type="date" class="form-control" id="hg-desde" value="' + d.periodo.desde + '"></div>' +
      '<div class="col-auto"><label class="mq-label" for="hg-hasta">Hasta</label><input type="date" class="form-control" id="hg-hasta" value="' + d.periodo.hasta + '"></div>' +
      '<div class="col-auto"><span class="mq-label">Variables</span><div class="d-flex flex-wrap gap-2">' +
      d.variables.map(function (v) { return '<button class="btn btn-sm btn-mq-secondary btn-mq-pill">' + esc(v) + '</button>'; }).join('') + '</div></div>' +
      '</div>' +
      '<div class="mt-3"><span class="mq-label">Resolucion</span><div class="d-flex gap-2">' +
      '<button class="btn btn-sm ' + (e === 'datos-muestras' ? 'btn-mq-primary' : 'btn-mq-secondary') + '">Muestras P30D</button>' +
      '<button class="btn btn-sm ' + (e === 'datos-agregados' ? 'btn-mq-primary' : 'btn-mq-secondary') + '">Agregados PT1H</button></div></div>');

    if (e === 'cargando') return head + controles + card(null, '<div class="mq-skeleton mq-skeleton--bloque" style="height:220px"></div><p class="mq-hint">Cargando la serie…</p>');
    if (e === 'vacio') return head + controles + card(null, vacio('chart', 'Sin datos en el periodo', 'El periodo seleccionado no tiene datos registrados.', '<button class="btn btn-mq-primary btn-sm">Probar con otro periodo</button>'));

    if (e === 'error-cobertura') {
      return head + controles + card(null,
        '<div class="mq-grafico" role="img" aria-label="Grafico con cobertura insuficiente"></div>' +
        banda('danger', '<strong>Cobertura insuficiente.</strong> La cobertura del periodo es ' + d.coberturaInsuficiente.cobertura + ' (por debajo de lo utilizable). ' + esc(d.coberturaInsuficiente.nota), 'alert'));
    }

    var grafico = '<div class="mq-grafico" role="img" aria-label="Evolucion de tensiones con marcas de evento"></div>';
    if (e === 'datos-agregados') {
      var a = d.agregados;
      return head + controles + card(null, grafico +
        banda('warning', '<strong>Serie agregada.</strong> Cobertura ' + a.cobertura + '; se conservan minimo y maximo ademas del promedio. ' + esc(a.advertencia), 'status') +
        kv([['promedio', a.promedio], ['minimo', a.minimo], ['maximo', a.maximo], ['p95', a.p95], ['muestras agregadas', '<span class="num">' + a.muestrasAgregadas.toLocaleString('es-AR') + '</span>']]) +
        '<p class="mq-hint">Conteo de microcortes del periodo: <strong class="num">' + d.microcortesConteo + '</strong> (desde Evento, nunca desde el promedio horario — CL-16).</p>');
    }
    // datos-muestras
    var m = d.muestras;
    var marcas = m.eventos.map(function (ev) { return [ev.instante, esc(ev.tipo), esc(ev.regla)]; });
    return head + controles + card(null, grafico +
      kv([['n muestras', '<span class="num">' + m.nMuestras + '</span>'], ['promedio', m.resumen.promedio], ['minimo', m.resumen.minimo], ['maximo', m.resumen.maximo], ['p95', m.resumen.p95]]) +
      '<h3 class="mq-card-titulo mt-3">Marcas de evento</h3>' + tabla(['Instante', 'Evento', 'Regla'], marcas));
  };

  /* ---- Panel-De-Verificaciones ---- */
  V['Panel-De-Verificaciones'] = function (e) {
    var d = D.verificaciones;
    var head = h1('Verificaciones');

    function badgeEstado(estado) {
      var map = { NuncaVerificado: 'neutral', Verificado: 'success', Vencido: 'warning', Refutado: 'danger' };
      var txt = { NuncaVerificado: 'Nunca', Verificado: 'Verificado', Vencido: 'Vencido', Refutado: 'Refutado' };
      return badge(txt[estado] || estado, map[estado] || 'neutral');
    }
    function listaSupuestos(sup) {
      var filas = sup.map(function (s) {
        var vig = s.vigenciaDias === null ? 'sin caducidad' : (s.venceEl ? 'vence ' + s.venceEl : 'vigencia ' + s.vigenciaDias + ' d');
        return ['<code>' + esc(s.id) + '</code>', esc(s.supuesto), badgeEstado(s.estado), esc(s.metodo), vig];
      });
      return tabla(['Supuesto', 'Descripcion', 'Estado', 'Metodo', 'Vigencia'], filas);
    }

    // banner de bloqueo (segun estado)
    var verificados, sup, banner;
    if (e === 'desbloqueado') { sup = d.supuestosDesbloqueado; verificados = 4; }
    else if (e === 'datos') { sup = d.supuestosMixtos; verificados = 2; }
    else if (e === 'refutado') { sup = d.supuestosMixtos.slice(); sup[3] = d.supuestosRefutado[0]; verificados = 2; }
    else if (e === 'vencido') { sup = d.supuestosMixtos; verificados = 2; }
    else { sup = d.supuestos; verificados = 0; }

    if (e === 'desbloqueado') {
      banner = banda('success', '<strong>Desbloqueado: 4 de 4 verificados.</strong> La modalidad efectiva deja de degradar; el apagado automatico puede actuar segun la politica.', 'status');
    } else {
      banner = banda('warning', '<strong>Politica en solo aviso · ' + verificados + ' de 4 verificados.</strong> El apagado automatico esta bloqueado por verificacion (RN-02).', 'alert');
    }

    if (e === 'cargando') {
      return head + banner + card('Estado de los supuestos', '<div class="mq-skeleton mq-skeleton--bloque"></div><p class="mq-hint">Ejecutando / registrando un paso…</p>');
    }

    var avisos = '';
    if (e === 'refutado') avisos = banda('danger', '<strong>Supuesto refutado: bloqueo permanente.</strong> ' + esc(d.supuestosRefutado[0].nota) + ' Distinto de «vencido»: refutado bloquea hasta resolverlo.', 'alert');
    if (e === 'vencido') avisos = banda('warning', '<strong>Supuesto vencido.</strong> Paso la vigencia (180/365 dias). Pide repetir la prueba; no bloquea permanentemente.', 'status');
    if (e === 'error-paso') avisos = banda('danger', '<strong>Paso fallido.</strong> La accion no se valido por efecto observado (RN-03): el efecto no se observo, el paso no se da por hecho.', 'alert');

    var estadoCard = card('Estado de los supuestos', listaSupuestos(sup));

    // ventana / stepper
    var ventana;
    if (e === 'ventana') {
      var pasoActual = 2;
      var stepperHtml = '<div class="mq-stepper" role="group" aria-label="Progreso de la ventana de mantenimiento">';
      d.stepper.forEach(function (p, i) {
        var estadoP = p.n < pasoActual ? 'completado' : (p.n === pasoActual ? 'actual' : 'pendiente');
        var attr = p.n === pasoActual ? ' aria-current="step"' : '';
        stepperHtml += '<div class="mq-step" data-estado="' + estadoP + '"' + attr + '><span class="mq-step-num">' + (estadoP === 'completado' ? '✓' : p.n) + '</span><span>Paso ' + p.n + '</span></div>';
        if (i < d.stepper.length - 1) stepperHtml += '<span class="mq-step-conector" data-pintado="' + (p.n < pasoActual ? 'si' : 'no') + '"></span>';
      });
      stepperHtml += '</div>';
      var paso = d.stepper[pasoActual - 1];
      ventana = card('Ventana de mantenimiento — Paso ' + pasoActual + ' de ' + d.stepper.length,
        stepperHtml +
        '<p><strong>' + esc(paso.titulo) + '</strong></p>' +
        '<div class="mb-3"><label class="mq-label" for="pv-obs">Resultado observado</label><input class="form-control" id="pv-obs" placeholder="registrar el efecto observado"></div>' +
        '<button class="btn btn-mq-primary">Confirmar paso</button>' +
        '<p class="mq-hint mt-2">Sin optimistic UI: ninguna accion se muestra como hecha antes de confirmarse por efecto observado.</p>');
    } else if (e !== 'desbloqueado') {
      ventana = card(null,
        '<p class="mq-hint">Requiere presencia fisica · procedimiento destructivo (corta la energia real).</p>' +
        '<button class="btn btn-mq-primary">Iniciar ventana</button>');
    } else {
      ventana = '';
    }

    return head + banner + avisos + estadoCard + '<div class="mt-3">' + ventana + '</div>';
  };

  /* ---- Registro-De-Intervenciones ---- */
  V['Registro-De-Intervenciones'] = function (e) {
    var d = D.intervenciones;
    var head = h1('Registrar intervencion');
    if (e === 'cargando') return head + cargando(3);
    if (e === 'vacio') return head + card(null, vacio('wrench', 'Sin intervenciones', 'Todavia no se cargo ninguna intervencion de servicio tecnico.', '<button class="btn btn-mq-primary btn-sm">Registrar intervencion</button>'));

    if (e === 'fuente-externa') {
      var filas = d.historial.map(function (h) {
        return [h.fecha, esc(h.tipo), '<span class="num">' + esc(h.costo) + '</span>',
          badge(h.fuente, h.fuente === 'externa' ? 'info' : 'neutral') + (h.nota ? '<div class="mq-hint">' + esc(h.nota) + '</div>' : ''),
          esc(h.confianza)];
      });
      return head + card('Historial de intervenciones',
        tabla(['Fecha', 'Tipo', 'Costo', 'Fuente', 'Confianza'], filas) +
        '<p class="mq-hint">La intervencion de fuente externa se muestra con <strong>confianza declarada menor</strong> que la del dato local; no se mezcla en silencio.</p>');
    }

    var f = d.formulario;
    var formu = card(null,
      '<div class="row g-3">' +
      selectCampo('Tipo', ['Recambio de bateria', 'Reparacion de equipo', 'Inspeccion preventiva']) +
      inputCampo('Instante', 'text', f.instante) +
      inputCampo('Dispositivo', 'text', f.dispositivo) +
      inputCampo('Bateria saliente', 'text', f.bateriaSaliente) +
      inputCampo('Bateria entrante', 'text', f.bateriaEntrante) +
      inputCampo('Proveedor', 'text', f.proveedor) +
      inputCampo('Ejecutada por', 'text', f.ejecutadaPor) +
      inputCampo('Registrada', 'text', f.tiempoRegistrado, 'Distinto del instante en que ocurrio (carga diferida, bitemporal).') +
      '</div>');

    // bloque de costos + cuadre
    var costos, errBanda = '';
    if (e === 'error-cuadre') {
      var c = d.costosNoCuadran;
      costos = card('Costos', kv([
        ['repuestos', c.repuestos + ' ARS'], ['mano de obra', c.manoDeObra + ' ARS'],
        ['total declarado', c.total + ' ARS ' + badge('No cuadra', 'danger')]
      ]));
      errBanda = banda('danger', '<strong>Los costos no cuadran.</strong> ' + esc(c.motivo) + ' No se aplica ningun efecto (validacion tipo 422).', 'alert');
    } else if (e === 'error-importe') {
      costos = card('Costos', '<div class="row g-3">' + inputCampo('Repuestos', 'text', d.costos.repuestos.monto) +
        inputCampoInv('Total (sin moneda)', 'text', '12.000') + '</div>');
      errBanda = banda('danger', '<strong>Importe sin moneda o fecha.</strong> ' + esc(d.importeSinMoneda.motivo), 'alert');
    } else {
      var co = d.costos;
      costos = card('Costos', kv([
        ['repuestos', co.repuestos.monto + ' ' + co.repuestos.moneda + ' @ ' + co.repuestos.fecha + ' <span class="mq-proc">' + co.repuestos.usd + ' USD</span>'],
        ['mano de obra', co.manoDeObra.monto + ' ' + co.manoDeObra.moneda + ' @ ' + co.manoDeObra.fecha],
        ['total declarado', '<strong>' + co.total.monto + ' ' + co.total.moneda + '</strong> ' + badge('Cuadra', 'success')]
      ]) + '<p class="mq-hint">Cada importe con moneda y fecha; el cuadre se verifica antes de aplicar (RN-07, RN-08).</p>');
    }

    if (e === 'error-coherencia') {
      errBanda = banda('danger', '<strong>Coherencia temporal.</strong> ' + esc(d.coherenciaTemporal.motivo) + ' La unidad dada de baja se consulta pero no se opera.', 'alert');
    }

    var disp = card('Disposicion final', kv([['destino', esc(d.disposicion.destino)], ['receptor', esc(d.disposicion.receptor)]]) + '<p class="mq-hint">Trazabilidad ambiental de la bateria retirada.</p>');

    var pie = '<div class="d-flex gap-2 mt-3"><button class="btn btn-mq-secondary">Previsualizar efectos</button><button class="btn btn-mq-primary ms-auto" ' + (e === 'error-cuadre' || e === 'error-importe' || e === 'error-coherencia' ? 'disabled' : '') + '>Registrar acto</button></div>';

    // ficha proyectada (efecto-aplicado)
    var ficha = '';
    if (e === 'efecto-aplicado') {
      var fi = d.ficha;
      ficha = banda('success', '<strong>Efecto aplicado.</strong> Se cerro el montaje viejo, se abrio el nuevo sin hueco, se dio de baja la retirada y se proyecto la ficha.', 'status') +
        card('Ficha de vida util (bateria retirada · ' + esc(fi.bateria) + ')', kv([
          ['dias en servicio', '<span class="num">' + fi.diasEnServicio + '</span> (' + fi.aniosEnServicio + ' anios)'],
          ['cumplio expectativa', badge(fi.cumplioExpectativa.texto, fi.cumplioExpectativa.tipo)],
          ['costo por anio', '<span class="num">' + fi.costoPorAnio + ' ' + fi.moneda + '</span> → <span class="num">' + fi.costoPorAnioUsd + ' USD</span> <span class="mq-proc">derivado · ' + fi.fuenteCotizacion + '</span>']
        ]) + '<p class="mq-hint">' + esc(fi.cumplioExpectativa.detalle) + '. El equivalente USD viaja marcado como derivado con su fuente de cotizacion (RN-07).</p>');
    }

    return head + errBanda + formu + '<div class="mt-3">' + costos + '</div><div class="mt-3">' + disp + '</div>' + pie +
      (ficha ? '<div class="mt-3">' + ficha + '</div>' : '');

    function inputCampo(label, tipo, valor, hint) {
      var id = 'ri-' + label.replace(/[^a-z]/gi, '').toLowerCase();
      return '<div class="col-md-6"><label class="mq-label" for="' + id + '">' + esc(label) + '</label>' +
        '<input type="' + tipo + '" class="form-control" id="' + id + '" value="' + esc(valor) + '"' + (hint ? ' aria-describedby="' + id + 'h"' : '') + '>' +
        (hint ? '<p class="mq-hint" id="' + id + 'h">' + esc(hint) + '</p>' : '') + '</div>';
    }
    function inputCampoInv(label, tipo, valor) {
      var id = 'ri-' + label.replace(/[^a-z]/gi, '').toLowerCase();
      return '<div class="col-md-6"><label class="mq-label" for="' + id + '">' + esc(label) + '</label>' +
        '<input type="' + tipo + '" class="form-control is-invalid" id="' + id + '" value="' + esc(valor) + '"></div>';
    }
    function selectCampo(label, opts) {
      var id = 'ri-' + label.replace(/[^a-z]/gi, '').toLowerCase();
      return '<div class="col-md-6"><label class="mq-label" for="' + id + '">' + esc(label) + '</label>' +
        '<select class="form-select" id="' + id + '">' + opts.map(function (o) { return '<option>' + esc(o) + '</option>'; }).join('') + '</select></div>';
    }
  };

  /* ---- Sustitucion-Del-SAI ---- */
  V['Sustitucion-Del-SAI'] = function (e) {
    var d = D.sustitucion;
    var head = h1('Reparacion y sustitucion del SAI');

    // linea de cobertura vigente
    var vigente;
    if (e === 'host-sin-cobertura') {
      vigente = banda('danger', '<strong>Host sin proteccion.</strong> Ningun equipo cubre a <code>' + esc(D.altaEquipos.host.nombre) + '</code> desde ' + esc(d.hostSinCobertura.desde) + ' (' + d.hostSinCobertura.dias + ' dias sin proteccion).', 'alert');
    } else if (e === 'suplente-activa') {
      vigente = card(null, 'Cobertura vigente: <code>' + esc(d.suplenteActivo.equipo) + '</code> protege <code>' + esc(d.suplenteActivo.host) + '</code> ' + badge('Cobertura suplente', 'info'));
    } else if (e === 'en-reparacion') {
      vigente = card(null, 'Cobertura vigente: <code>' + esc(d.coberturaVigente.equipo) + '</code> ' + badge('En reparacion', 'warning') + ' — su cobertura aparece cerrada.');
    } else {
      vigente = card(null, 'Cobertura vigente: <code>' + esc(d.coberturaVigente.equipo) + '</code> protege <code>' + esc(d.coberturaVigente.host) + '</code> ' + badge(d.coberturaVigente.estado.texto, d.coberturaVigente.estado.tipo));
    }

    if (e === 'cargando') return head + vigente + card(null, '<div class="mq-skeleton mq-skeleton--bloque"></div>');

    var errBanda = '';
    if (e === 'error-solapada') errBanda = banda('danger', '<strong>Cobertura solapada.</strong> Se intentaria abrir una cobertura que se solapa con otra vigente. A lo sumo una cobertura vigente por host (RC-02).', 'alert');
    if (e === 'error-coherencia') errBanda = banda('danger', '<strong>Coherencia temporal.</strong> La intervencion se fecha despues de la baja del equipo afectado; no se aplica el efecto (RC-08).', 'alert');

    var formu = card('Intervencion sobre el equipo',
      '<div class="row g-3">' +
      '<div class="col-md-6"><span class="mq-label">Accion</span><div class="d-flex gap-2"><button class="btn btn-sm btn-mq-secondary">Reparacion</button><button class="btn btn-sm btn-mq-primary">Sustitucion</button></div></div>' +
      '<div class="col-md-6"><label class="mq-label" for="ss-inst">Instante</label><input class="form-control" id="ss-inst" value="2026-09-05 10:30"></div>' +
      '<div class="col-md-6"><label class="mq-label" for="ss-sal">Equipo saliente</label><select class="form-select" id="ss-sal"><option>ups-01</option></select></div>' +
      '<div class="col-md-6"><label class="mq-label" for="ss-sup">Suplente</label><select class="form-select" id="ss-sup"><option>ups-02 (en stock)</option></select></div>' +
      '</div>' +
      '<div class="d-flex gap-2 mt-3"><button class="btn btn-mq-secondary">Previsualizar efectos</button><button class="btn btn-mq-primary ms-auto" ' + (errBanda ? 'disabled' : '') + '>Registrar acto</button></div>');

    // sucesion de coberturas
    var sucesion;
    if (e === 'vacio') {
      sucesion = card('Sucesion de coberturas del host', vacio('swap', 'Sin sucesion registrada', 'El host tiene una sola cobertura, sin intervenciones aun.', ''));
    } else {
      var filas = '';
      d.sucesion.tramos.forEach(function (t) {
        if (t.hueco) {
          filas += '<tr><td colspan="3">' + banda('danger', '⚠ ' + esc(t.texto), 'status') + '</td></tr>';
        } else {
          filas += '<tr><td><code>' + esc(t.equipo) + '</code></td><td class="num">' + esc(t.desde) + ' → ' + esc(t.hasta || 'abierto') + '</td><td>' + badge(t.estado.texto, t.estado.tipo) + '</td></tr>';
        }
      });
      sucesion = card('Sucesion de coberturas del host',
        (d.sucesion.reconstruida ? banda('info', 'Datos de sustitucion <strong>reconstruidos para la maqueta</strong>: el flujo UF-7 no tiene escenario de datos completo (R-11). No provienen de §20.', 'status') : '') +
        '<div class="mq-scroll-x"><table class="mq-tabla"><thead><tr><th>Equipo</th><th>Intervalo</th><th>Estado</th></tr></thead><tbody>' + filas + '</tbody></table></div>');
    }

    var aviso = card(null, banda('warning', '<strong>Nota:</strong> ' + esc(d.avisoCaracterizacion) + ' <a href="Panel-De-Verificaciones.html">Ir a caracterizar →</a>', 'status'));

    return head + vigente + errBanda + '<div class="mt-3">' + formu + '</div><div class="mt-3">' + sucesion + '</div><div class="mt-3">' + aviso + '</div>';
  };

  /* ---- Informe-De-Periodo ---- */
  V['Informe-De-Periodo'] = function (e) {
    var d = D.informe;
    var head = h1('Informe de periodo');
    var p = d.parametros;
    var params = card(null,
      '<div class="row g-3 align-items-end">' +
      '<div class="col-auto"><label class="mq-label" for="ip-host">Host</label><select class="form-select" id="ip-host"><option>' + esc(p.host) + '</option></select></div>' +
      '<div class="col-auto"><label class="mq-label" for="ip-desde">Desde</label><input type="date" class="form-control" id="ip-desde" value="' + p.desde + '"></div>' +
      '<div class="col-auto"><label class="mq-label" for="ip-hasta">Hasta</label><input type="date" class="form-control" id="ip-hasta" value="' + p.hasta + '"></div>' +
      '<div class="col-auto"><button class="btn btn-mq-primary">Generar</button></div>' +
      '</div>' +
      '<div class="mt-3 d-flex gap-2"><button class="btn btn-sm ' + (e === 'comparacion-confianza' ? 'btn-mq-secondary' : 'btn-mq-primary') + '">Informe de periodo</button>' +
      '<button class="btn btn-sm ' + (e === 'comparacion-confianza' ? 'btn-mq-primary' : 'btn-mq-secondary') + '">Comparacion de marcas</button></div>');

    if (e === 'cargando') return head + params + cargando(3);
    if (e === 'vacio') return head + params + card(null, vacio('report', 'Sin seleccion', 'Elegi host y periodo y genera el informe.', ''));
    if (e === 'error-periodo') return head + params + banda('danger', '<strong>Periodo sin datos suficientes.</strong> ' + esc(d.periodoSinDatos.motivo), 'alert');
    if (e === 'error-agregado') return head + params + banda('danger', '<strong>Agregado sin cobertura.</strong> ' + esc(d.agregadoSinCobertura.motivo), 'alert');

    if (e === 'comparacion-confianza') {
      var filasCmp = d.comparacion.modelos.map(function (m) {
        return ['<code>' + esc(m.modelo) + '</code>', '<span class="num">' + esc(m.costoAnioUsd) + ' USD</span> <span class="mq-proc">derivado</span>', esc(m.cumplio), esc(m.desvio)];
      });
      return head + params + card('Comparacion de marcas',
        tabla(['Modelo', 'Costo/anio (USD)', 'Cumplio', 'Desvio'], filasCmp) +
        banda('warning', '<strong>Confianza baja.</strong> ' + esc(d.comparacion.aviso), 'status'));
    }

    // informe de periodo (datos / informe-advertencia)
    var cob = d.cobertura;
    var coberturaCard = card('Cobertura', kv([
      ['dispositivos activos', esc(cob.dispositivosActivos)],
      ['dias con proteccion', '<span class="num">' + cob.diasConProteccion + '</span>'],
      ['dias sin proteccion', '<span class="num">' + cob.diasSinProteccion + '</span>']
    ]) + '<p class="mq-label mt-2">Baterias intervinientes (incluye bajas)</p>' +
      tabla(['Bateria', 'Dias en el periodo', 'Estado'], cob.bateriasIntervinientes.map(function (b) {
        return ['<code>' + esc(b.id) + '</code>', '<span class="num">' + b.dias + '</span>', badge(b.estado, b.estado === 'DadoDeBaja' ? 'neutral' : 'success')];
      })));

    var ic = d.intervencionesCostos;
    var costosCard = card('Intervenciones y costos', tabla(['Tipo', 'Cantidad', 'Costo'], ic.porTipo.map(function (t) {
      return [esc(t.tipo), '<span class="num">' + t.cantidad + '</span>', '<span class="num">' + esc(t.costo) + '</span>'];
    })) + '<p class="mq-hint">Equivalente USD: <span class="num">' + esc(ic.totalUsd) + '</span> <span class="mq-proc">derivado · ' + esc(ic.fuenteCotizacion) + '</span></p>');

    var evFilas = Object.keys(d.eventos).map(function (k) { return [esc(k), '<span class="num">' + d.eventos[k] + '</span>']; });
    var advertencia = (e === 'informe-advertencia' || true);
    var calidadCard = card('Eventos y calidad de suministro',
      tabla(['Tipo de evento', 'Conteo'], evFilas) +
      '<p class="mq-hint">Microcortes desde <strong>Evento</strong>, no del promedio horario.</p>' +
      banda('warning', '<strong>Calidad sobre agregados · cobertura ' + d.calidad.cobertura + '.</strong> ' + esc(d.calidad.advertencia), 'status'));

    return head + params + coberturaCard + '<div class="mt-3">' + costosCard + '</div><div class="mt-3">' + calidadCard + '</div>';
  };

  /* ====================================================================
     Shell + barra de validacion + estado
     ==================================================================== */
  var supActual, estadoActual;

  function navHtml(sup) {
    var items = D.superficies.filter(function (s) { return s.chrome === 'trabajo'; });
    var s = '<nav class="mq-nav" aria-label="Modulos">';
    items.forEach(function (it) {
      var cur = it.id === sup.id ? ' aria-current="page"' : '';
      s += '<a class="mq-nav-link" href="' + it.archivo + '"' + cur + '>' + icon(it.icono, 20) + '<span>' + esc(it.titulo) + '</span></a>';
    });
    return s + '</nav>';
  }

  function pintar(estado) {
    estadoActual = estado;
    var main = document.getElementById('contenido');
    main.innerHTML = V[supActual.id](estado);
    var live = document.getElementById('mq-live');
    if (live) {
      var st = supActual.estados.filter(function (x) { return x.id === estado; })[0];
      live.textContent = 'Estado: ' + (st ? st.nombre : estado);
    }
    // marcar el select de la barra
    var sel = document.getElementById('mq-estado-select');
    if (sel && sel.value !== estado) sel.value = estado;
    engancharDiagnostico();
  }

  function construir(sup) {
    supActual = sup;
    var body = document.body;
    body.innerHTML = '';
    var skip = '<a class="mq-skip" href="#contenido">Saltar al contenido</a>';
    var contenido;

    if (sup.chrome === 'trabajo') {
      contenido =
        '<div class="mq-shell">' +
        '<header class="mq-chrome">' +
        '<div class="mq-chrome-marca">' + icon('activity', 20) + '<span>Sai-Service-Core</span></div>' +
        navHtml(sup) +
        '</header>' +
        '<div class="mq-topbar">' +
        '<div class="mq-identidad">' + icon('user-plus', 16) + '<span>' + esc(D.usuario.nombre) + '</span></div>' +
        '<div class="mq-topbar-acciones">' +
        '<button class="btn btn-sm btn-mq-secondary">Cambiar Contraseña</button>' +
        '<a class="btn btn-sm btn-mq-secondary" href="Acceso-Login.html">Cerrar Sesión</a>' +
        selloHtml() +
        '</div></div>' +
        '<main class="mq-main" id="contenido"></main>' +
        '</div>';
    } else {
      contenido =
        '<main class="mq-shell-acceso" id="contenido-wrap">' +
        '<div id="contenido" style="width:100%;max-width:380px;display:flex;flex-direction:column;align-items:center"></div>' +
        '</main>';
    }

    body.insertAdjacentHTML('beforeend', skip + contenido);
    // pie con sello de version en superficies de trabajo (dentro de main)
    // (para acceso, cada vista ya agrega su footer)
    body.insertAdjacentHTML('beforeend', barraValidacionHtml(sup));
    body.insertAdjacentHTML('beforeend', '<div id="mq-live" class="mq-sr-only" role="status" aria-live="polite"></div>');
    body.insertAdjacentHTML('beforeend', modalDiagnosticoHtml());

    engancharBarra(sup);
    pintar(sup.estadoInicial);
    engancharDiagnostico();
  }

  // Para superficies de trabajo el footer debe re-agregarse en cada pintar:
  var _pintarBase = pintar;
  pintar = function (estado) {
    _pintarBase(estado);
    if (supActual.chrome === 'trabajo') {
      var m = document.getElementById('contenido');
      m.insertAdjacentHTML('beforeend', footerSello());
    }
  };

  /* ---- Barra de validacion ---- */
  function barraValidacionHtml(sup) {
    var opts = sup.estados.map(function (st) {
      return '<option value="' + st.id + '"' + (st.id === sup.estadoInicial ? ' selected' : '') + '>' + esc(st.nombre) + '</option>';
    }).join('');
    return '<div class="mq-validacion" role="region" aria-label="Barra de validacion de maqueta">' +
      '<span class="mq-validacion-rotulo">' + icon('alert', 16) + 'Barra de validacion de maqueta — no forma parte del producto</span>' +
      '<label for="mq-estado-select">Estado:</label>' +
      '<select id="mq-estado-select" aria-label="Estado de la superficie">' + opts + '</select>' +
      '<div class="mq-recarga">' +
      '<div class="form-check form-switch m-0">' +
      '<input class="form-check-input" type="checkbox" id="mq-recarga-check">' +
      '<label class="form-check-label" for="mq-recarga-check" style="color:#fff">Recarga automatica</label>' +
      '</div>' +
      '<span class="mq-recarga-razon" id="mq-recarga-razon"></span>' +
      '</div></div>';
  }

  function engancharBarra(sup) {
    var sel = document.getElementById('mq-estado-select');
    sel.addEventListener('change', function () { pintar(sel.value); });
    configurarRecarga();
  }

  /* ---- Recarga automatica (§4.3): localStorage, visibilidad, file:// ---- */
  var recargaTimer = null;
  var recargaClave = 'mq-recarga-auto';
  function configurarRecarga() {
    var check = document.getElementById('mq-recarga-check');
    var razon = document.getElementById('mq-recarga-razon');
    var esFile = location.protocol === 'file:';
    if (esFile) {
      check.checked = false;
      check.disabled = true;
      razon.textContent = 'Deshabilitada sobre file:// — servi la maqueta por HTTP para usarla.';
      return;
    }
    var guardado = localStorage.getItem(recargaClave) === 'on';
    check.checked = guardado;
    if (guardado) iniciarRecarga();
    check.addEventListener('change', function () {
      localStorage.setItem(recargaClave, check.checked ? 'on' : 'off');
      if (check.checked) iniciarRecarga(); else detenerRecarga();
    });
    document.addEventListener('visibilitychange', function () {
      if (document.hidden) detenerRecarga();
      else if (check.checked) iniciarRecarga();
    });
  }
  var etagActual = null;
  function iniciarRecarga() {
    detenerRecarga();
    recargaTimer = setInterval(function () {
      if (document.hidden) return;
      // Deteccion por identificador de recurso (HEAD -> ETag/Last-Modified), sin descarga completa
      fetch(location.href, { method: 'HEAD', cache: 'no-store' }).then(function (r) {
        var id = r.headers.get('ETag') || r.headers.get('Last-Modified');
        if (etagActual === null) { etagActual = id; return; }
        if (id && id !== etagActual) { location.reload(); }
      }).catch(function () { /* degradacion silenciosa */ });
    }, 3000); // intervalo entre 2 y 5 s
  }
  function detenerRecarga() { if (recargaTimer) { clearInterval(recargaTimer); recargaTimer = null; } }

  /* ---- Modal de diagnostico de version ---- */
  function modalDiagnosticoHtml() {
    var c = D.version.contrato;
    var filas = [
      ['versionLegible', c.versionLegible], ['identificadorDeConstruccion', c.identificadorDeConstruccion],
      ['esPreliminar', String(c.esPreliminar)], ['origenIndeterminado', String(c.origenIndeterminado)],
      ['modeloUx', D.version.modeloUx], ['iteracion', D.version.fechaIteracion]
    ];
    var body = '<dl class="mq-kv">' + filas.map(function (f) { return '<dt>' + esc(f[0]) + '</dt><dd>' + esc(f[1]) + '</dd>'; }).join('') + '</dl>' +
      '<p class="mq-hint">Valores de version marcados «sin dato de ejemplo»: la documentacion no ejemplifica la cadena de version (declarado como ambiguedad).</p>';
    return '<div class="modal fade" id="mq-diag" tabindex="-1" aria-labelledby="mq-diag-t" aria-hidden="true">' +
      '<div class="modal-dialog"><div class="modal-content">' +
      '<div class="modal-header"><h2 class="modal-title h5" id="mq-diag-t">Detalle de diagnostico de version</h2>' +
      '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button></div>' +
      '<div class="modal-body">' + body + '</div>' +
      '<div class="modal-footer"><button type="button" class="btn btn-mq-primary" id="mq-diag-copiar">' + icon('copy', 16) + ' Copiar</button></div>' +
      '</div></div></div>';
  }
  function engancharDiagnostico() {
    document.querySelectorAll('[data-accion="diagnostico"]').forEach(function (btn) {
      if (btn._enganchado) return; btn._enganchado = true;
      btn.addEventListener('click', function () {
        var modalEl = document.getElementById('mq-diag');
        var m = global.bootstrap ? global.bootstrap.Modal.getOrCreateInstance(modalEl) : null;
        if (m) m.show();
      });
    });
    var cop = document.getElementById('mq-diag-copiar');
    if (cop && !cop._enganchado) {
      cop._enganchado = true;
      cop.addEventListener('click', function () {
        var txt = diagnosticoTexto();
        if (navigator.clipboard) navigator.clipboard.writeText(txt).catch(function () {});
        var live = document.getElementById('mq-live');
        if (live) live.textContent = 'Diagnostico de version copiado al portapapeles.';
      });
    }
  }

  /* ---- Init ---- */
  function init() {
    var id = document.body.getAttribute('data-superficie');
    var sup = D.porId(id);
    if (!sup) { document.body.innerHTML = '<p style="padding:2rem">Superficie desconocida: ' + esc(id) + '</p>'; return; }
    document.title = sup.titulo + ' — Maqueta Sai-Service-Core';
    construir(sup);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
  else init();

})(window);
