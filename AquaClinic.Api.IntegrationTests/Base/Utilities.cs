using AquaClinic.Domain.Entities;
using AquaClinic.Persistence;
using System;

namespace AquaClinic.Api.IntegrationTests.Base
{
    public class Utilities
    {
        /// <summary>
        /// Siembra especialidades de prueba en el contexto InMemory. Idempotente:
        /// con EnsureCreated el modelo ya aplica el seed de HasData (las especialidades
        /// reales de la clínica), por lo que no hace falta repetirlo.
        /// </summary>
        public static void InitializeDbForTests(AquaClinicDbContext context)
        {
            if (context.Especialidades.Any())
            {
                return;
            }

            var test1Guid = Guid.Parse("{B0788D2F-8003-43C1-92A4-EDC76A7C5DDE}");
            var test2Guid = Guid.Parse("{6313179F-7837-473A-A4D5-A5571B43E6A6}");
            var test3Guid = Guid.Parse("{BF3F3002-7E53-441E-8B76-F6280BE284AA}");
            var test4Guid = Guid.Parse("{FE98F549-E790-4E9F-AA16-18C2292A2EE9}");

            context.Especialidades.Add(new Especialidad
            {
                EspecialidadId = test1Guid,
                Nombre = "Especialidad de prueba 1"
            });
            context.Especialidades.Add(new Especialidad
            {
                EspecialidadId = test2Guid,
                Nombre = "Especialidad de prueba 2"
            });
            context.Especialidades.Add(new Especialidad
            {
                EspecialidadId = test3Guid,
                Nombre = "Especialidad de prueba 3"
            });
            context.Especialidades.Add(new Especialidad
            {
                EspecialidadId = test4Guid,
                Nombre = "Especialidad de prueba 4"
            });

            context.SaveChanges();
        }
    }
}
