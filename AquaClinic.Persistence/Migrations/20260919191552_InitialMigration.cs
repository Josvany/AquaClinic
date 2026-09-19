using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AquaClinic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consultas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalConsulta = table.Column<int>(type: "int", nullable: false),
                    FechaSolicitada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pagada = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Especialidades",
                columns: table => new
                {
                    EspecialidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.EspecialidadId);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Precio = table.Column<int>(type: "int", nullable: false),
                    Doctor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EspecialidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.CitaId);
                    table.ForeignKey(
                        name: "FK_Citas_Especialidades_EspecialidadId",
                        column: x => x.EspecialidadId,
                        principalTable: "Especialidades",
                        principalColumn: "EspecialidadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Consultas",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "FechaSolicitada", "LastModifiedBy", "LastModifiedDate", "Pagada", "TotalConsulta", "UserId" },
                values: new object[,]
                {
                    { new Guid("3dcb3ea0-80b1-4781-b5c0-4d85c41e55a6"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 13, 9, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, 245, new Guid("4ad901be-f447-46dd-bcf7-dbe401afa203") },
                    { new Guid("771cca4b-066c-4ac7-b3df-4d12837fe7e0"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, 85, new Guid("d97a15fc-0d32-41c6-9ddf-62f0735c4c1c") },
                    { new Guid("7e94bc5b-71a5-4c8c-bc3b-71bb7976237e"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, 400, new Guid("a441eb40-9636-4ee6-be49-a66c5ec1330b") },
                    { new Guid("86d3a045-b42d-4854-8150-d6a374948b6e"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 11, 9, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, 135, new Guid("ac3cfaf5-34fd-4e4d-bc04-ad1083ddc340") },
                    { new Guid("ba0eb0ef-b69b-46fd-b8e2-41b4178ae7cb"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 16, 9, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, 116, new Guid("7aeb2c01-fe8e-4b84-a5ba-330bdf950f5c") },
                    { new Guid("e6a2679c-79a3-4ef1-a478-6f4c91b405b6"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 14, 9, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, 142, new Guid("7aeb2c01-fe8e-4b84-a5ba-330bdf950f5c") },
                    { new Guid("f5a6a3a0-4227-4973-abb5-a63fbe725923"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, 40, new Guid("f5a6a3a0-4227-4973-abb5-a63fbe725923") }
                });

            migrationBuilder.InsertData(
                table: "Especialidades",
                columns: new[] { "EspecialidadId", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "Nombre" },
                values: new object[,]
                {
                    { new Guid("6313179f-7837-473a-a4d5-a5571b43e6a6"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Pediatría" },
                    { new Guid("b0788d2f-8003-43c1-92a4-edc76a7c5dde"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Cardiología" },
                    { new Guid("bf3f3002-7e53-441e-8b76-f6280be284aa"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Dermatología" },
                    { new Guid("fe98f549-e790-4e9f-aa16-18c2292a2ee9"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Traumatología" }
                });

            migrationBuilder.InsertData(
                table: "Citas",
                columns: new[] { "CitaId", "CreatedBy", "CreatedDate", "Descripcion", "Doctor", "EspecialidadId", "Fecha", "ImagenUrl", "LastModifiedBy", "LastModifiedDate", "Nombre", "Precio" },
                values: new object[,]
                {
                    { new Guid("1babd057-e980-4cb3-9cd2-7fdd9e525668"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Estudio completo del aparato locomotor orientado a deportistas, con pruebas de movilidad y plan de recuperación.", "Dr. Luis Fernández", new Guid("fe98f549-e790-4e9f-aa16-18c2292a2ee9"), new DateTime(2027, 7, 8, 11, 0, 0, 0, DateTimeKind.Unspecified), "https://picsum.photos/seed/aquaclinic5/300/200", null, null, "Evaluación traumatológica deportiva", 400 },
                    { new Guid("3448d5a4-0f72-4dd7-bf15-c14a46b26c00"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Seguimiento de pacientes con hipertensión: control de valores, ajuste de medicación y recomendaciones de estilo de vida.", "Dra. María González", new Guid("b0788d2f-8003-43c1-92a4-edc76a7c5dde"), new DateTime(2027, 6, 20, 10, 30, 0, 0, DateTimeKind.Unspecified), "https://picsum.photos/seed/aquaclinic2/300/200", null, null, "Control de hipertensión arterial", 85 },
                    { new Guid("62787623-4c52-43fe-b0c9-b7044fb5929b"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Revisión pediátrica anual con control de crecimiento, desarrollo y calendario vacunal del niño.", "Dra. Ana Martínez", new Guid("6313179f-7837-473a-a4d5-a5571b43e6a6"), new DateTime(2027, 1, 25, 16, 0, 0, 0, DateTimeKind.Unspecified), "https://picsum.photos/seed/aquaclinic4/300/200", null, null, "Chequeo pediátrico anual", 25 },
                    { new Guid("adc42c09-08c1-4d2c-9f96-2d15bb1af299"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mapeo corporal de lunares y lesiones cutáneas con dermatoscopia digital para la detección temprana de alteraciones.", "Dra. Laura Sánchez", new Guid("bf3f3002-7e53-441e-8b76-f6280be284aa"), new DateTime(2027, 5, 30, 15, 0, 0, 0, DateTimeKind.Unspecified), "https://picsum.photos/seed/aquaclinic6/300/200", null, null, "Revisión dermatológica completa", 135 },
                    { new Guid("b419a7ca-3321-4f38-be8e-4d7b6a529319"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Realización de electrocardiograma en reposo junto con evaluación del ritmo cardíaco por parte del especialista.", "Dr. Carlos Rodríguez", new Guid("b0788d2f-8003-43c1-92a4-edc76a7c5dde"), new DateTime(2027, 1, 12, 8, 30, 0, 0, DateTimeKind.Unspecified), "https://picsum.photos/seed/aquaclinic3/300/200", null, null, "Electrocardiograma y evaluación cardíaca", 85 },
                    { new Guid("ee272f8b-6096-4cb6-8625-bb4bb2d89e8b"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Evaluación cardiológica completa que incluye revisión de historial clínico, medición de presión arterial y auscultación cardíaca.", "Dr. Juan Pérez", new Guid("b0788d2f-8003-43c1-92a4-edc76a7c5dde"), new DateTime(2027, 3, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "https://picsum.photos/seed/aquaclinic1/300/200", null, null, "Consulta de cardiología general", 65 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_EspecialidadId",
                table: "Citas",
                column: "EspecialidadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Consultas");

            migrationBuilder.DropTable(
                name: "Especialidades");
        }
    }
}
