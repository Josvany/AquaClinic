using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AquaClinic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EliminarOpcionesServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiciosOpciones");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiciosOpciones",
                columns: table => new
                {
                    ServicioOpcionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrupoNombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosOpciones", x => x.ServicioOpcionId);
                    table.ForeignKey(
                        name: "FK_ServiciosOpciones_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "ServicioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ServiciosOpciones",
                columns: new[] { "ServicioOpcionId", "Activo", "ColorHex", "CreatedBy", "CreatedDate", "GrupoNombre", "LastModifiedBy", "LastModifiedDate", "Orden", "ServicioId", "Valor" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-2701-7c10-a9f3-5d21c4a7e101"), true, "#E8C4B8", null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Color", null, null, 1, new Guid("01a0bc01-2514-7922-a163-24d411678c6f"), "Nude clásico" },
                    { new Guid("01a0bc01-2701-7c10-a9f3-5d21c4a7e102"), true, "#C0392B", null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Color", null, null, 2, new Guid("01a0bc01-2514-7922-a163-24d411678c6f"), "Rojo apasionado" },
                    { new Guid("01a0bc01-2701-7c10-a9f3-5d21c4a7e103"), true, "#2A6B66", null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Color", null, null, 3, new Guid("01a0bc01-2514-7922-a163-24d411678c6f"), "Teal aqua" },
                    { new Guid("01a0bc01-2701-7c10-a9f3-5d21c4a7e104"), true, "#D4B36A", null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Color", null, null, 4, new Guid("01a0bc01-2514-7922-a163-24d411678c6f"), "Dorado suave" },
                    { new Guid("01a0bc01-2701-7c20-a9f3-5d21c4a7e201"), true, null, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tamaño", null, null, 1, new Guid("01a0bc01-2514-7922-a163-24d411678c6f"), "Corta" },
                    { new Guid("01a0bc01-2701-7c20-a9f3-5d21c4a7e202"), true, null, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tamaño", null, null, 2, new Guid("01a0bc01-2514-7922-a163-24d411678c6f"), "Mediana" },
                    { new Guid("01a0bc01-2701-7c20-a9f3-5d21c4a7e203"), true, null, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tamaño", null, null, 3, new Guid("01a0bc01-2514-7922-a163-24d411678c6f"), "Larga" },
                    { new Guid("01a0bc01-2701-7c30-a9f3-5d21c4a7e301"), true, null, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diseño", null, null, 1, new Guid("01a0bc01-2583-7f61-a561-01f4cf10e04a"), "Natural" },
                    { new Guid("01a0bc01-2701-7c30-a9f3-5d21c4a7e302"), true, "#5D4037", null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diseño", null, null, 2, new Guid("01a0bc01-2583-7f61-a561-01f4cf10e04a"), "Con tinte" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosOpciones_ServicioId",
                table: "ServiciosOpciones",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosOpciones_ServicioId_GrupoNombre",
                table: "ServiciosOpciones",
                columns: new[] { "ServicioId", "GrupoNombre" });
        }
    }
}
