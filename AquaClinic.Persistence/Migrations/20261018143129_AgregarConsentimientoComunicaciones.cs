using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaClinic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConsentimientoComunicaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AceptaComunicaciones",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaConsentimientoUtc",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AceptaComunicaciones",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FechaConsentimientoUtc",
                table: "AspNetUsers");
        }
    }
}
