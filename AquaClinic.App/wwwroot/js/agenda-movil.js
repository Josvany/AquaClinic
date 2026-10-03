let _mediaQuery = null;
let _dotNetRef = null;

export function iniciar(mediaQuery, dotNetRef) {
    _mediaQuery = window.matchMedia(mediaQuery);
    _dotNetRef = dotNetRef;
    _mediaQuery.addEventListener('change', alCambiar);
    return _mediaQuery.matches;
}

function alCambiar(e) {
    _dotNetRef?.invokeMethodAsync('AlCambiarAncho', e.matches);
}

export function detener() {
    _mediaQuery?.removeEventListener('change', alCambiar);
    _mediaQuery = null;
    _dotNetRef = null;
}
