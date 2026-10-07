using AquaClinic.App.ViewModels;
using Microsoft.AspNetCore.Components;

namespace AquaClinic.App.Components
{
    /// <summary>
    /// Vista reutilizable del detalle de una cita (cliente, contacto,
    /// servicios, horario, esteticista y notas) con las acciones de
    /// mover y cancelar. La comparten la pagina /citadetails y el modal
    /// de edicion del listado de citas.
    /// </summary>
    public partial class CitaDetalleVista
    {
        [Parameter]
        public CitaDetalleViewModel Cita { get; set; } = default!;

        [Parameter]
        public EventCallback OnMover { get; set; }

        [Parameter]
        public EventCallback OnCancelar { get; set; }
    }
}
