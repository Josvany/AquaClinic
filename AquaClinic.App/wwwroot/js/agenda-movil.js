// Deteccion de ancho para /reservar: la vista del RadzenScheduler cambia
// entre agenda (movil) y semana/dia (escritorio) sin recargar la pagina.
// El circuito Blazor crea este modulo en OnAfterRenderAsync(firstRender)
// y lo suelta en DisposeAsync (detener + DisposeAsync del modulo).

let _mediaQuery = null;
let _dotNetRef = null;

export function iniciar(mediaQuery, dotNetRef) {
    _mediaQuery = window.matchMedia(mediaQuery);
    _dotNetRef = dotNetRef;

    _mediaQuery.addEventListener('change', alCambiar);

    // Estado inicial: true si el ancho cae en el media query
    return _mediaQuery.matches;
}

function alCambiar(e) {
    if (_dotNetRef) {
        _dotNetRef.invokeMethodAsync('AlCambiarAncho', e.matches);
    }
}

export function detener() {
    if (_mediaQuery) {
        _mediaQuery.removeEventListener('change', alCambiar);
        _mediaQuery = null;
    }
    _dotNetRef = null;
}
