(function () {
    'use strict';

    // Marcador de JS activo: las animaciones CSS de reveal (css/animaciones.css)
    // solo ocultan contenido cuando esta clase esta presente. Sin JS, todo visible.
    document.documentElement.classList.add('ag-js');

    var movimientoReducido = window.matchMedia
        && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    function correrContador(el) {
        var destino = parseInt(el.dataset.cuenta, 10);
        var inicio = performance.now();
        var duracion = 1400;
        function paso(ahora) {
            var p = Math.min((ahora - inicio) / duracion, 1);
            var eased = 1 - Math.pow(1 - p, 3);
            el.textContent = Math.round(destino * eased).toLocaleString('es-MX');
            if (p < 1) requestAnimationFrame(paso);
        }
        requestAnimationFrame(paso);
    }

    function iniciarContadores() {
        var contadores = document.querySelectorAll('[data-cuenta]:not([data-animado])');
        if (!contadores.length) return;

        if (!('IntersectionObserver' in window) || movimientoReducido) {
            contadores.forEach(function (el) {
                el.dataset.animado = '1';
                el.textContent = parseInt(el.dataset.cuenta, 10).toLocaleString('es-MX');
            });
            return;
        }

        var observador = new IntersectionObserver(function (entradas) {
            entradas.forEach(function (entrada) {
                if (!entrada.isIntersecting) return;
                entrada.target.dataset.animado = '1';
                correrContador(entrada.target);
                observador.unobserve(entrada.target);
            });
        }, { threshold: 0.4 });

        contadores.forEach(function (c) { observador.observe(c); });
    }

    // ------------------------------------------------------------
    // Reveal on scroll: revela [data-ag-reveal] y los hijos de
    // [data-ag-reveal-escalon] con retraso escalonado de 60ms.
    // Solo transform/opacity; nunca corre con movimiento reducido.
    // ------------------------------------------------------------
    var observadorRevelado = null;

    function prepararRevelado() {
        var pendientes = document.querySelectorAll('[data-ag-reveal]:not(.ag-revelado), [data-ag-reveal-escalon]:not(.ag-revelado)');

        if (!pendientes.length) return;

        if (!('IntersectionObserver' in window) || movimientoReducido) {
            pendientes.forEach(function (el) { el.classList.add('ag-revelado'); });
            return;
        }

        if (!observadorRevelado) {
            observadorRevelado = new IntersectionObserver(function (entradas) {
                entradas.forEach(function (entrada) {
                    if (!entrada.isIntersecting) return;
                    entrada.target.classList.add('ag-revelado');
                    observadorRevelado.unobserve(entrada.target);
                });
            }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });
        }

        pendientes.forEach(function (el) {
            if (el.hasAttribute('data-ag-reveal-escalon')) {
                // Enumerar hijos para el retraso escalonado (el CSS usa --ag-orden)
                Array.prototype.forEach.call(el.children, function (hijo, i) {
                    hijo.style.setProperty('--ag-orden', i);
                });
            }
            observadorRevelado.observe(el);
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        iniciarContadores();
        prepararRevelado();
    });
    // La navegacion "enhanced" de Blazor reemplaza el DOM: re-observar
    document.addEventListener('enhancedload', function () {
        iniciarContadores();
        prepararRevelado();
    });

    // ------------------------------------------------
    // Modal de login: enviar con fetch para que un fallo
    // de credenciales NO cierre el modal. Muestra el mensaje
    // de error dentro del propio modal y permite reintentar.
    // ------------------------------------------------
    var MENSAJES_LOGIN = {
        credenciales: 'Correo o contrase\u00f1a inv\u00e1lido. Prueba nuevamente.',
        ratelimit: 'Demasiados intentos. Espera un momento y prueba nuevamente.',
        api: 'No se pudo conectar con el servidor. Intenta de nuevo en unos segundos.'
    };

    function mostrarErrorLogin(form, texto) {
        var caja = form.querySelector('.ag-login-error');
        var texto_ = form.querySelector('.ag-login-error-texto');
        if (!caja) return;
        if (texto_) texto_.textContent = texto;
        caja.hidden = false;
    }

    function ocultarErrorLogin(form) {
        var caja = form.querySelector('.ag-login-error');
        if (caja) caja.hidden = true;
    }

    window.agLoginModal = async function (form) {
        if (!form || form.dataset.agEnviando === '1') return;

        // El navegador ya valido required/minlength/email antes de @onsubmit
        var datos = new FormData(form);
        var token = datos.get('__RequestVerificationToken');
        if (!token) return; // sin antiforgery no se puede enviar: dejar el flujo normal

        form.dataset.agEnviando = '1';
        var boton = form.querySelector('button[type="submit"]');
        if (boton) boton.disabled = true;
        ocultarErrorLogin(form);

        try {
            var response = await fetch(form.action || '/auth/login', {
                method: 'POST',
                body: datos,
                headers: { 'X-Requested-With': 'fetch' }
            });

            if (!response.ok) {
                // 400/500: fallo inesperado del servidor
                mostrarErrorLogin(form, MENSAJES_LOGIN.api);
                return;
            }

            // Contrato JSON del endpoint: { redirect: '/url' }. Si la URL lleva
            // ?error=..., las credenciales fallaron: el modal permanece abierto
            // y muestra el mensaje. Si no, es exito: navegar al destino.
            var data = await response.json();
            var destino = (data && data.redirect) || '/';
            var errorMatch = destino.match(/[?&]error=([a-z]+)/i);

            if (errorMatch) {
                mostrarErrorLogin(form, MENSAJES_LOGIN[errorMatch[1]] || MENSAJES_LOGIN.credenciales);
            } else {
                window.location.href = destino;
            }
        } catch (e) {
            mostrarErrorLogin(form, MENSAJES_LOGIN.api);
        } finally {
            delete form.dataset.agEnviando;
            if (boton) boton.disabled = false;
        }
    };

    // Red de seguridad: si algo impide que terminen las animaciones de entrada
    // del hero (webview sin composicion, pestana en segundo plano), forzar la
    // visibilidad del contenido tras 1.6s. En un navegador normal la animacion
    // ya termino y esta clase no cambia nada.
    window.setTimeout(function () {
        document.documentElement.classList.add('ag-hero-listo');
    }, 1600);
})();
