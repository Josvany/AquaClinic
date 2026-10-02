using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AquaClinic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DatosDemoAgendaOctubre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Bloqueos",
                columns: new[] { "BloqueoId", "CreatedBy", "CreatedDate", "FechaFin", "FechaInicio", "LastModifiedBy", "LastModifiedDate", "Motivo", "PersonalId" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-26c1-7c01-a9f3-5d21c4a7e001"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTimeOffset(new DateTime(2026, 10, 12, 18, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), null, null, "Día de Acción de Gracias (Canadá)", null },
                    { new Guid("01a0bc01-26c2-7c02-a9f3-5d21c4a7e002"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTimeOffset(new DateTime(2026, 10, 7, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 7, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), null, null, "Capacitación del personal", null }
                });

            migrationBuilder.InsertData(
                table: "Citas",
                columns: new[] { "CitaId", "CreatedBy", "CreatedDate", "EmailInvitado", "Estado", "FechaFin", "FechaInicio", "LastModifiedBy", "LastModifiedDate", "NombreInvitado", "Notas", "PersonalId", "TelefonoInvitado", "UsuarioId" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-26c3-7c03-a9f3-5d21c4a7e003"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "valentina.cruz@example.com", 2, new DateTimeOffset(new DateTime(2026, 10, 6, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 6, 11, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), null, null, "Valentina Cruz", "Prefiere productos sin fragancia.", new Guid("01a0bc01-25b2-7f49-a1e7-e4e7afb2645e"), "+1 604-555-0404", null },
                    { new Guid("01a0bc01-26c4-7c04-a9f3-5d21c4a7e004"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "derek.chen@example.com", 1, new DateTimeOffset(new DateTime(2026, 10, 8, 16, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 8, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), null, null, "Derek Chen", null, new Guid("01a0bc01-25a3-7f89-a190-15ac098e2d00"), "+1 604-555-0505", null },
                    { new Guid("01a0bc01-26c5-7c05-a9f3-5d21c4a7e005"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "sofia.ramirez@example.com", 2, new DateTimeOffset(new DateTime(2026, 10, 10, 11, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 10, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), null, null, "Sofía Ramírez", null, new Guid("01a0bc01-25c2-7148-a710-befdeeb20ef7"), "+1 604-555-0606", null }
                });

            migrationBuilder.InsertData(
                table: "CitasServicios",
                columns: new[] { "CitaId", "ServicioId" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-26c3-7c03-a9f3-5d21c4a7e003"), new Guid("01a0bc01-2514-7922-a163-24d411678c6f") },
                    { new Guid("01a0bc01-26c4-7c04-a9f3-5d21c4a7e004"), new Guid("01a0bc01-2543-7a75-ad35-6cbe60d25a62") },
                    { new Guid("01a0bc01-26c5-7c05-a9f3-5d21c4a7e005"), new Guid("01a0bc01-2583-7f61-a561-01f4cf10e04a") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Bloqueos",
                keyColumn: "BloqueoId",
                keyValue: new Guid("01a0bc01-26c1-7c01-a9f3-5d21c4a7e001"));

            migrationBuilder.DeleteData(
                table: "Bloqueos",
                keyColumn: "BloqueoId",
                keyValue: new Guid("01a0bc01-26c2-7c02-a9f3-5d21c4a7e002"));

            migrationBuilder.DeleteData(
                table: "CitasServicios",
                keyColumns: new[] { "CitaId", "ServicioId" },
                keyValues: new object[] { new Guid("01a0bc01-26c3-7c03-a9f3-5d21c4a7e003"), new Guid("01a0bc01-2514-7922-a163-24d411678c6f") });

            migrationBuilder.DeleteData(
                table: "CitasServicios",
                keyColumns: new[] { "CitaId", "ServicioId" },
                keyValues: new object[] { new Guid("01a0bc01-26c4-7c04-a9f3-5d21c4a7e004"), new Guid("01a0bc01-2543-7a75-ad35-6cbe60d25a62") });

            migrationBuilder.DeleteData(
                table: "CitasServicios",
                keyColumns: new[] { "CitaId", "ServicioId" },
                keyValues: new object[] { new Guid("01a0bc01-26c5-7c05-a9f3-5d21c4a7e005"), new Guid("01a0bc01-2583-7f61-a561-01f4cf10e04a") });

            migrationBuilder.DeleteData(
                table: "Citas",
                keyColumn: "CitaId",
                keyValue: new Guid("01a0bc01-26c3-7c03-a9f3-5d21c4a7e003"));

            migrationBuilder.DeleteData(
                table: "Citas",
                keyColumn: "CitaId",
                keyValue: new Guid("01a0bc01-26c4-7c04-a9f3-5d21c4a7e004"));

            migrationBuilder.DeleteData(
                table: "Citas",
                keyColumn: "CitaId",
                keyValue: new Guid("01a0bc01-26c5-7c05-a9f3-5d21c4a7e005"));
        }
    }
}
