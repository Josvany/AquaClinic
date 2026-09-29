using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaClinic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarContabilidadServicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovimientosServicios",
                columns: table => new
                {
                    MovimientoServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    NombreCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PrecioSnapshot = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FechaAtencion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosServicios", x => x.MovimientoServicioId);
                    table.ForeignKey(
                        name: "FK_MovimientosServicios_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosServicios_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "CitaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosServicios_Personal_PersonalId",
                        column: x => x.PersonalId,
                        principalTable: "Personal",
                        principalColumn: "PersonalId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosServicios_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "ServicioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResumenesDiarios",
                columns: table => new
                {
                    ResumenDiarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaLocal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadAtenciones = table.Column<int>(type: "int", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    ActualizadoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumenesDiarios", x => x.ResumenDiarioId);
                    table.ForeignKey(
                        name: "FK_ResumenesDiarios_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "ServicioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosServicios_CitaId_ServicioId",
                table: "MovimientosServicios",
                columns: new[] { "CitaId", "ServicioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosServicios_FechaAtencion",
                table: "MovimientosServicios",
                column: "FechaAtencion");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosServicios_PersonalId",
                table: "MovimientosServicios",
                column: "PersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosServicios_ServicioId",
                table: "MovimientosServicios",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosServicios_UsuarioId",
                table: "MovimientosServicios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ResumenesDiarios_FechaLocal_ServicioId",
                table: "ResumenesDiarios",
                columns: new[] { "FechaLocal", "ServicioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResumenesDiarios_ServicioId",
                table: "ResumenesDiarios",
                column: "ServicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosServicios");

            migrationBuilder.DropTable(
                name: "ResumenesDiarios");
        }
    }
}
