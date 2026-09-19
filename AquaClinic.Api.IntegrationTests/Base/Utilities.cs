using AquaClinic.Domain.Entities;
using AquaClinic.Persistence;
using System;

namespace AquaClinic.Api.IntegrationTests.Base
{
    public class Utilities
    {
        public static void InitializeDbForTests(AquaClinicDbContext context)
        {
            var cardiologiaGuid = Guid.Parse("{B0788D2F-8003-43C1-92A4-EDC76A7C5DDE}");
            var pediatriaGuid = Guid.Parse("{6313179F-7837-473A-A4D5-A5571B43E6A6}");
            var dermatologiaGuid = Guid.Parse("{BF3F3002-7E53-441E-8B76-F6280BE284AA}");
            var traumatologiaGuid = Guid.Parse("{FE98F549-E790-4E9F-AA16-18C2292A2EE9}");

            context.Especialidades.Add(new Especialidad
            {
                EspecialidadId = cardiologiaGuid,
                Nombre = "Cardiología"
            });
            context.Especialidades.Add(new Especialidad
            {
                EspecialidadId = pediatriaGuid,
                Nombre = "Pediatría"
            });
            context.Especialidades.Add(new Especialidad
            {
                EspecialidadId = dermatologiaGuid,
                Nombre = "Dermatología"
            });
            context.Especialidades.Add(new Especialidad
            {
                EspecialidadId = traumatologiaGuid,
                Nombre = "Traumatología"
            });

            context.SaveChanges();
        }
    }
}
