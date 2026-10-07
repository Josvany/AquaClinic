using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Features.Promociones;
using AquaClinic.Application.Features.Servicios.Queries.GetServiciosList;
using AquaClinic.Application.Profiles;
using AquaClinic.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AquaClinic.Application.UnitTests.Promociones
{
    /// <summary>
    /// El CRUD de promociones no aplica dos descuentos al mismo servicio. Estos
    /// tests fijan la regla de precedencia de ResolucionPromocion: gana la mas
    /// especifica (por servicio) sobre la global.
    /// </summary>
    public class ResolucionPromocionTests
    {
        private static readonly Guid ServicioA = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ServicioB = Guid.Parse("22222222-2222-2222-2222-222222222222");

        private static IMapper CrearMapper() =>
            new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance)
                .CreateMapper();

        [Fact]
        public void SinPromociones_NoEligeNinguna()
        {
            Assert.Null(ResolucionPromocion.Elegir([], ServicioA));
        }

        [Fact]
        public void PromocionGlobal_AplicaATodosLosServicios()
        {
            var global = Global(15);

            Assert.Same(global, ResolucionPromocion.Elegir([global], ServicioA));
            Assert.Same(global, ResolucionPromocion.Elegir([global], ServicioB));
        }

        [Fact]
        public void PromocionGlobalYPorServicio_GanaLaPorServicio()
        {
            var global = Global(15);
            var porServicio = ConServicios(20, 50, ServicioB);

            Assert.Same(porServicio, ResolucionPromocion.Elegir([global, porServicio], ServicioB));
        }

        [Fact]
        public void PromocionPorServicio_NoAplicaAOtroServicio()
        {
            var porServicio = ConServicios(20, 50, ServicioB);

            Assert.Null(ResolucionPromocion.Elegir([porServicio], ServicioA));
        }

        [Fact]
        public void VariasPorServicio_EligeLaDeVigenciaMasReciente()
        {
            var antigua = ConServicios(10, 1, ServicioA, desde: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
            var reciente = ConServicios(30, 1, ServicioA, desde: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));

            Assert.Same(reciente, ResolucionPromocion.Elegir([antigua, reciente], ServicioA));
        }

        [Fact]
        public void MismoInicio_EligeElMayorDescuento()
        {
            var desde = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var menor = ConServicios(10, 1, ServicioA, desde: desde);
            var mayor = ConServicios(30, 1, ServicioA, desde: desde);

            Assert.Same(mayor, ResolucionPromocion.Elegir([menor, mayor], ServicioA));
        }

        [Fact]
        public void LaOrdenDeLaListaNoAfectaElResultado()
        {
            var global = Global(15);
            var porServicio = ConServicios(20, 50, ServicioA);

            Assert.Same(porServicio, ResolucionPromocion.Elegir([global, porServicio], ServicioA));
            Assert.Same(porServicio, ResolucionPromocion.Elegir([porServicio, global], ServicioA));
        }

        [Fact]
        public async Task GetServiciosList_AplicaLaPromocionVigenteAlPrecio()
        {
            var servicio = new Servicio
            {
                ServicioId = ServicioA,
                Nombre = "Limpieza profunda",
                Precio = 120m,
                Activo = true,
                Especialidad = new Especialidad { EspecialidadId = Guid.NewGuid(), Nombre = "Faciales" }
            };

            var repoServicios = new Mock<IServicioRepository>();
            repoServicios.Setup(r => r.GetActivosConEspecialidadAsync()).ReturnsAsync([servicio]);

            var repoPromociones = new Mock<IPromocionRepository>();
            repoPromociones.Setup(r => r.GetVigentesAsync(It.IsAny<DateTimeOffset>()))
                .ReturnsAsync([Global(15)]); // 120 - 15% = 102.00

            var handler = new GetServiciosListQueryHandler(
                CrearMapper(), repoServicios.Object, repoPromociones.Object);

            var resultado = await handler.Handle(new GetServiciosListQuery(), CancellationToken.None);

            var vm = Assert.Single(resultado);
            Assert.Equal(120m, vm.Precio);
            Assert.Equal(15m, vm.ValorPromocion);
            Assert.Equal("Global", vm.NombrePromocion);
            Assert.Equal(102m, vm.PrecioConDescuento);
        }

        [Fact]
        public async Task GetServiciosList_SinPromociones_MantieneElPrecio()
        {
            var servicio = new Servicio
            {
                ServicioId = ServicioA,
                Nombre = "Limpieza profunda",
                Precio = 120m,
                Activo = true,
                Especialidad = new Especialidad { EspecialidadId = Guid.NewGuid(), Nombre = "Faciales" }
            };

            var repoServicios = new Mock<IServicioRepository>();
            repoServicios.Setup(r => r.GetActivosConEspecialidadAsync()).ReturnsAsync([servicio]);

            var repoPromociones = new Mock<IPromocionRepository>();
            repoPromociones.Setup(r => r.GetVigentesAsync(It.IsAny<DateTimeOffset>()))
                .ReturnsAsync([]);

            var handler = new GetServiciosListQueryHandler(
                CrearMapper(), repoServicios.Object, repoPromociones.Object);

            var vm = Assert.Single(await handler.Handle(new GetServiciosListQuery(), CancellationToken.None));

            Assert.Null(vm.NombrePromocion);
            Assert.Equal(120m, vm.PrecioConDescuento);
        }

        // ---------- Helpers ----------

        private static Promocion Global(decimal valor) => Crear("Global", valor, null);

        private static Promocion ConServicios(decimal valor, decimal _,
            Guid servicioId,
            DateTimeOffset? desde = null) => Crear("Por servicio", valor, desde, servicioId);

        private static Promocion Crear(string nombre, decimal valor, DateTimeOffset? desde, params Guid[] servicios)
        {
            var promocion = new Promocion
            {
                Nombre = nombre,
                Valor = valor,
                TipoDescuento = Domain.Enums.TipoDescuento.Porcentaje,
                Activo = true,
                VigenciaDesde = desde,
                CreatedDate = new DateTime(2026, 1, 1)
            };

            if (servicios.Length > 0)
            {
                promocion.PromocionesServicios = servicios
                    .Select(id => new PromocionServicio { ServicioId = id })
                    .ToList();
            }

            return promocion;
        }
    }
}