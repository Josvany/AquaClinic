using AquaClinic.Domain.Entities;

namespace AquaClinic.Application.Features.Promociones
{
    /// <summary>
    /// Decide que promocion aplica a un servicio cuando varias son candidatas.
    /// No se pueden acumular dos descuentos sobre el mismo servicio, asi que
    /// gana UNA sola, segun este orden de prioridad:
    /// <list type="number">
    ///   <item>La mas especifica: una promocion con servicios asociados
    ///   (PromocionServicio) gana a una global (sin servicios asociados).</item>
    ///   <item>A igual alcance, la de vigencia mas reciente (VigenciaDesde mayor).</item>
    ///   <item>Si siguen empatadas, el mayor valor de descuento.</item>
    /// </list>
    /// Solo entran las promociones ya filtradas por
    /// <see cref="Contracts.Persistence.IPromocionRepository.GetVigentesAsync"/>
    /// (activas y dentro de su vigencia).
    /// </summary>
    public static class ResolucionPromocion
    {
        /// <summary>
        /// Devuelve la promocion que aplica al servicio, o null si ninguna le
        /// corresponde. <paramref name="vigentes"/> son las promociones activas y
        /// vigentes, con sus servicios asociados cargados.
        /// </summary>
        public static Promocion? Elegir(IReadOnlyList<Promocion> vigentes, Guid servicioId)
        {
            if (vigentes.Count == 0)
            {
                return null;
            }

            Promocion? elegido = null;
            var elegidoEsGlobal = false;

            foreach (var candidata in vigentes)
            {
                if (!Aplica(candidata, servicioId))
                {
                    continue;
                }

                var candidataEsGlobal = EsGlobal(candidata);

                if (elegido is null
                    || (elegidoEsGlobal && !candidataEsGlobal)          // 1. la especifica gana
                    || (elegidoEsGlobal == candidataEsGlobal
                        && (candidata.VigenciaDesde ?? DateTimeOffset.MinValue)
                            > (elegido.VigenciaDesde ?? DateTimeOffset.MinValue))) // 2. mas reciente
                {
                    elegido = candidata;
                    elegidoEsGlobal = candidataEsGlobal;
                }
                else if (elegidoEsGlobal == candidataEsGlobal
                         && (candidata.VigenciaDesde ?? DateTimeOffset.MinValue)
                             == (elegido.VigenciaDesde ?? DateTimeOffset.MinValue)
                         && candidata.Valor > elegido.Valor)          // 3. mayor descuento
                {
                    elegido = candidata;
                }
            }

            return elegido;
        }

        /// <summary>True si la promocion no tiene servicios asociados: aplica a todos.</summary>
        public static bool EsGlobal(Promocion promocion) =>
            promocion.PromocionesServicios is null || promocion.PromocionesServicios.Count == 0;

        /// <summary>True si la promocion aplica a ese servicio concreto.</summary>
        public static bool Aplica(Promocion promocion, Guid servicioId) =>
            EsGlobal(promocion)
            || (promocion.PromocionesServicios!.Any(ps => ps.ServicioId == servicioId));
    }
}