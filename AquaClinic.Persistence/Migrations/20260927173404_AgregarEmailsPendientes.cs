using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaClinic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEmailsPendientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailsPendientes",
                columns: table => new
                {
                    EmailPendienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Destinatario = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cuerpo = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Intentos = table.Column<int>(type: "int", nullable: false),
                    UltimoIntentoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnviadoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UltimoError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailsPendientes", x => x.EmailPendienteId);
                    table.ForeignKey(
                        name: "FK_EmailsPendientes_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "CitaId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailsPendientes_CitaId",
                table: "EmailsPendientes",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailsPendientes_Estado_UltimoIntentoUtc",
                table: "EmailsPendientes",
                columns: new[] { "Estado", "UltimoIntentoUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailsPendientes");
        }
    }
}
