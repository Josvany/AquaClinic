(function () {
    'use strict';

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

        if (!('IntersectionObserver' in window)) {
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

    document.addEventListener('DOMContentLoaded', iniciarContadores);
    document.addEventListener('enhancedload', iniciarContadores);
})();
