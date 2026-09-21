using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AquaClinic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
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
                name: "HorariosClinica",
                columns: table => new
                {
                    HorarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraApertura = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraCierre = table.Column<TimeOnly>(type: "time", nullable: false),
                    Cerrado = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosClinica", x => x.HorarioId);
                });

            migrationBuilder.CreateTable(
                name: "Promociones",
                columns: table => new
                {
                    PromocionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoDescuento = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    VigenciaDesde = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    VigenciaHasta = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promociones", x => x.PromocionId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Personal",
                columns: table => new
                {
                    PersonalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personal", x => x.PersonalId);
                    table.ForeignKey(
                        name: "FK_Personal_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EspecialidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.ServicioId);
                    table.ForeignKey(
                        name: "FK_Servicios_Especialidades_EspecialidadId",
                        column: x => x.EspecialidadId,
                        principalTable: "Especialidades",
                        principalColumn: "EspecialidadId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cupones",
                columns: table => new
                {
                    CuponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromocionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FechaCanje = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupones", x => x.CuponId);
                    table.ForeignKey(
                        name: "FK_Cupones_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cupones_Promociones_PromocionId",
                        column: x => x.PromocionId,
                        principalTable: "Promociones",
                        principalColumn: "PromocionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bloqueos",
                columns: table => new
                {
                    BloqueoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaInicio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaFin = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PersonalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bloqueos", x => x.BloqueoId);
                    table.ForeignKey(
                        name: "FK_Bloqueos_Personal_PersonalId",
                        column: x => x.PersonalId,
                        principalTable: "Personal",
                        principalColumn: "PersonalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NombreInvitado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmailInvitado = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TelefonoInvitado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PersonalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaInicio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaFin = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.CitaId);
                    table.ForeignKey(
                        name: "FK_Citas_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Citas_Personal_PersonalId",
                        column: x => x.PersonalId,
                        principalTable: "Personal",
                        principalColumn: "PersonalId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PersonalEspecialidades",
                columns: table => new
                {
                    PersonalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EspecialidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalEspecialidades", x => new { x.PersonalId, x.EspecialidadId });
                    table.ForeignKey(
                        name: "FK_PersonalEspecialidades_Especialidades_EspecialidadId",
                        column: x => x.EspecialidadId,
                        principalTable: "Especialidades",
                        principalColumn: "EspecialidadId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonalEspecialidades_Personal_PersonalId",
                        column: x => x.PersonalId,
                        principalTable: "Personal",
                        principalColumn: "PersonalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromocionesServicios",
                columns: table => new
                {
                    PromocionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromocionesServicios", x => new { x.PromocionId, x.ServicioId });
                    table.ForeignKey(
                        name: "FK_PromocionesServicios_Promociones_PromocionId",
                        column: x => x.PromocionId,
                        principalTable: "Promociones",
                        principalColumn: "PromocionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromocionesServicios_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "ServicioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CitasServicios",
                columns: table => new
                {
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitasServicios", x => new { x.CitaId, x.ServicioId });
                    table.ForeignKey(
                        name: "FK_CitasServicios_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "CitaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CitasServicios_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "ServicioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosPuntos",
                columns: table => new
                {
                    MovimientoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Puntos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosPuntos", x => x.MovimientoId);
                    table.ForeignKey(
                        name: "FK_MovimientosPuntos_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosPuntos_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "CitaId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Bloqueos",
                columns: new[] { "BloqueoId", "CreatedBy", "CreatedDate", "FechaFin", "FechaInicio", "LastModifiedBy", "LastModifiedDate", "Motivo", "PersonalId" },
                values: new object[] { new Guid("01a0bc01-265b-7685-a5ac-92c0c18cacdc"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTimeOffset(new DateTime(2026, 10, 5, 17, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 5, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), null, null, "Capacitación del equipo", null });

            migrationBuilder.InsertData(
                table: "Especialidades",
                columns: new[] { "EspecialidadId", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "Nombre" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-2484-76f6-add2-b68394c7f1ac"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Tratamientos Faciales" },
                    { new Guid("01a0bc01-24e2-7db8-a95a-7f4a793a97f6"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Cuidado Avanzado de la Piel" },
                    { new Guid("01a0bc01-24f4-79e6-a0f3-e83a946f9209"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Tratamientos Inyectables" },
                    { new Guid("01a0bc01-2505-7a4e-a157-2f9393424890"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Cejas" }
                });

            migrationBuilder.InsertData(
                table: "HorariosClinica",
                columns: new[] { "HorarioId", "Cerrado", "CreatedBy", "CreatedDate", "DiaSemana", "HoraApertura", "HoraCierre", "LastModifiedBy", "LastModifiedDate" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-25d3-7a78-aef1-5d50aeb3e1fb"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, new TimeOnly(9, 0, 0), new TimeOnly(18, 0, 0), null, null },
                    { new Guid("01a0bc01-25f3-7b30-a329-25589a5711b3"), false, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, new TimeOnly(9, 0, 0), new TimeOnly(18, 0, 0), null, null },
                    { new Guid("01a0bc01-2602-73ac-a02e-32fbcb3bdd8f"), false, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new TimeOnly(9, 0, 0), new TimeOnly(18, 0, 0), null, null },
                    { new Guid("01a0bc01-2612-7e08-a00d-f93c067a4c09"), false, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, new TimeOnly(9, 0, 0), new TimeOnly(18, 0, 0), null, null },
                    { new Guid("01a0bc01-2622-75cf-af4d-d77cf2210516"), false, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, new TimeOnly(9, 0, 0), new TimeOnly(18, 0, 0), null, null },
                    { new Guid("01a0bc01-263c-7dd1-ace4-4a9fe536f4f6"), false, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, new TimeOnly(9, 0, 0), new TimeOnly(18, 0, 0), null, null },
                    { new Guid("01a0bc01-264b-77ba-a44b-31d1732df51f"), false, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, new TimeOnly(10, 0, 0), new TimeOnly(16, 0, 0), null, null }
                });

            migrationBuilder.InsertData(
                table: "Personal",
                columns: new[] { "PersonalId", "Activo", "Apellidos", "ApplicationUserId", "CreatedBy", "CreatedDate", "Email", "LastModifiedBy", "LastModifiedDate", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-25a3-7f89-a190-15ac098e2d00"), true, "Solano", null, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "walner@aquaskinandglow.ca", null, null, "Walner", "+1 604-555-0001" },
                    { new Guid("01a0bc01-25b2-7f49-a1e7-e4e7afb2645e"), true, "Rojas", null, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "maria.rojas@aquaskinandglow.ca", null, null, "María Fernanda", "+1 604-555-0002" },
                    { new Guid("01a0bc01-25c2-7148-a710-befdeeb20ef7"), true, "Méndez", null, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "carolina.mendez@aquaskinandglow.ca", null, null, "Carolina", "+1 604-555-0003" }
                });

            migrationBuilder.InsertData(
                table: "Promociones",
                columns: new[] { "PromocionId", "Activo", "CreatedBy", "CreatedDate", "Descripcion", "LastModifiedBy", "LastModifiedDate", "Nombre", "TipoDescuento", "Valor", "VigenciaDesde", "VigenciaHasta" },
                values: new object[] { new Guid("01a0bc01-26a9-7fb3-a2e7-2dc235635667"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "15% de descuento en todos los servicios por la gran inauguración de la clínica.", null, null, "Gran inauguración", 1, 15.00m, new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 14, 23, 59, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "Bloqueos",
                columns: new[] { "BloqueoId", "CreatedBy", "CreatedDate", "FechaFin", "FechaInicio", "LastModifiedBy", "LastModifiedDate", "Motivo", "PersonalId" },
                values: new object[] { new Guid("01a0bc01-266b-7287-a46b-2350ca53d7b4"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTimeOffset(new DateTime(2026, 11, 6, 18, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -8, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 11, 2, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -8, 0, 0, 0)), null, null, "Vacaciones", new Guid("01a0bc01-25c2-7148-a710-befdeeb20ef7") });

            migrationBuilder.InsertData(
                table: "Citas",
                columns: new[] { "CitaId", "CreatedBy", "CreatedDate", "EmailInvitado", "Estado", "FechaFin", "FechaInicio", "LastModifiedBy", "LastModifiedDate", "NombreInvitado", "Notas", "PersonalId", "TelefonoInvitado", "UsuarioId" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-267b-72d7-abd9-df8492b67a3e"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ana.torres@example.com", 2, new DateTimeOffset(new DateTime(2026, 9, 25, 16, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 9, 25, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), null, null, "Ana Torres", "Primera visita; piel sensible.", new Guid("01a0bc01-25b2-7f49-a1e7-e4e7afb2645e"), "+1 604-555-0101", null },
                    { new Guid("01a0bc01-268a-7884-af3f-6d7b6df74218"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "luis.gomez@example.com", 1, new DateTimeOffset(new DateTime(2026, 9, 26, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 9, 26, 11, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), null, null, "Luis Gómez", null, new Guid("01a0bc01-25b2-7f49-a1e7-e4e7afb2645e"), "+1 604-555-0202", null },
                    { new Guid("01a0bc01-269a-77f7-a5f2-fbcbb5422872"), null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "priya.sharma@example.com", 1, new DateTimeOffset(new DateTime(2026, 10, 2, 15, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 2, 14, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), null, null, "Priya Sharma", null, new Guid("01a0bc01-25c2-7148-a710-befdeeb20ef7"), "+1 604-555-0303", null }
                });

            migrationBuilder.InsertData(
                table: "PersonalEspecialidades",
                columns: new[] { "EspecialidadId", "PersonalId" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-2484-76f6-add2-b68394c7f1ac"), new Guid("01a0bc01-25a3-7f89-a190-15ac098e2d00") },
                    { new Guid("01a0bc01-24f4-79e6-a0f3-e83a946f9209"), new Guid("01a0bc01-25a3-7f89-a190-15ac098e2d00") },
                    { new Guid("01a0bc01-2484-76f6-add2-b68394c7f1ac"), new Guid("01a0bc01-25b2-7f49-a1e7-e4e7afb2645e") },
                    { new Guid("01a0bc01-24e2-7db8-a95a-7f4a793a97f6"), new Guid("01a0bc01-25b2-7f49-a1e7-e4e7afb2645e") },
                    { new Guid("01a0bc01-2484-76f6-add2-b68394c7f1ac"), new Guid("01a0bc01-25c2-7148-a710-befdeeb20ef7") },
                    { new Guid("01a0bc01-2505-7a4e-a157-2f9393424890"), new Guid("01a0bc01-25c2-7148-a710-befdeeb20ef7") }
                });

            migrationBuilder.InsertData(
                table: "Servicios",
                columns: new[] { "ServicioId", "Activo", "CreatedBy", "CreatedDate", "Descripcion", "DuracionMinutos", "EspecialidadId", "ImagenUrl", "LastModifiedBy", "LastModifiedDate", "Nombre", "Precio" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-2514-7922-a163-24d411678c6f"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Limpieza facial profunda adaptada a tu tipo de piel, con extracciones e hidratación final.", 60, new Guid("01a0bc01-2484-76f6-add2-b68394c7f1ac"), "https://picsum.photos/seed/aquaclinic1/300/200", null, null, "Limpieza profunda personalizada", 120.00m },
                    { new Guid("01a0bc01-2523-78d0-a114-6d55dac0a477"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exfoliación e hidratación con tecnología de vortex-fusion para renovar y revitalizar la piel.", 45, new Guid("01a0bc01-2484-76f6-add2-b68394c7f1ac"), "https://picsum.photos/seed/aquaclinic2/300/200", null, null, "Hidrodermoabrasión", 110.00m },
                    { new Guid("01a0bc01-2533-7e91-af1e-b75928c66155"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tratamiento con alta frecuencia diseñado para hidratar y renovar la piel del rostro.", 45, new Guid("01a0bc01-2484-76f6-add2-b68394c7f1ac"), "https://picsum.photos/seed/aquaclinic3/300/200", null, null, "Facial de alta frecuencia", 95.00m },
                    { new Guid("01a0bc01-2543-7a75-ad35-6cbe60d25a62"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Microinfundido para mejorar la textura de la piel y reducir las cicatrices del acné.", 60, new Guid("01a0bc01-24e2-7db8-a95a-7f4a793a97f6"), "https://picsum.photos/seed/aquaclinic4/300/200", null, null, "Microneedling", 250.00m },
                    { new Guid("01a0bc01-2553-77a8-a7b9-9d9b607248a5"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tratamiento de radiofrecuencia para estimular la producción natural de colágeno.", 30, new Guid("01a0bc01-24e2-7db8-a95a-7f4a793a97f6"), "https://picsum.photos/seed/aquaclinic5/300/200", null, null, "Radiofrecuencia", 140.00m },
                    { new Guid("01a0bc01-2563-7d94-abbc-b9a1df6195cc"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Toxina botulínica para combatir las líneas de expresión y el envejecimiento facial.", 30, new Guid("01a0bc01-24f4-79e6-a0f3-e83a946f9209"), "https://picsum.photos/seed/aquaclinic6/300/200", null, null, "Botox", 350.00m },
                    { new Guid("01a0bc01-2573-7b42-a339-14023acbc2eb"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bioestimuladores de colágeno para un rejuvenecimiento facial progresivo y natural.", 45, new Guid("01a0bc01-24f4-79e6-a0f3-e83a946f9209"), "https://picsum.photos/seed/aquaclinic7/300/200", null, null, "Bioestimuladores", 400.00m },
                    { new Guid("01a0bc01-2583-7f61-a561-01f4cf10e04a"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Laminado para lograr cejas más densas, fijas y con una forma definida.", 60, new Guid("01a0bc01-2505-7a4e-a157-2f9393424890"), "https://picsum.photos/seed/aquaclinic8/300/200", null, null, "Laminado de cejas", 85.00m },
                    { new Guid("01a0bc01-2593-79db-aec9-9e1d37b12b55"), true, null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diseño, perfilado y tinte de cejas para realzar la mirada.", 45, new Guid("01a0bc01-2505-7a4e-a157-2f9393424890"), "https://picsum.photos/seed/aquaclinic9/300/200", null, null, "Diseño y perfilado de cejas", 65.00m }
                });

            migrationBuilder.InsertData(
                table: "CitasServicios",
                columns: new[] { "CitaId", "ServicioId" },
                values: new object[,]
                {
                    { new Guid("01a0bc01-267b-72d7-abd9-df8492b67a3e"), new Guid("01a0bc01-2514-7922-a163-24d411678c6f") },
                    { new Guid("01a0bc01-268a-7884-af3f-6d7b6df74218"), new Guid("01a0bc01-2543-7a75-ad35-6cbe60d25a62") },
                    { new Guid("01a0bc01-269a-77f7-a5f2-fbcbb5422872"), new Guid("01a0bc01-2583-7f61-a561-01f4cf10e04a") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bloqueos_FechaInicio",
                table: "Bloqueos",
                column: "FechaInicio");

            migrationBuilder.CreateIndex(
                name: "IX_Bloqueos_PersonalId",
                table: "Bloqueos",
                column: "PersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_FechaInicio",
                table: "Citas",
                column: "FechaInicio");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_PersonalId",
                table: "Citas",
                column: "PersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_UsuarioId",
                table: "Citas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CitasServicios_ServicioId",
                table: "CitasServicios",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_Codigo",
                table: "Cupones",
                column: "Codigo",
                unique: true,
                filter: "[Codigo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_PromocionId",
                table: "Cupones",
                column: "PromocionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_UsuarioId",
                table: "Cupones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosClinica_DiaSemana",
                table: "HorariosClinica",
                column: "DiaSemana",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosPuntos_CitaId",
                table: "MovimientosPuntos",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosPuntos_UsuarioId",
                table: "MovimientosPuntos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Personal_ApplicationUserId",
                table: "Personal",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalEspecialidades_EspecialidadId",
                table: "PersonalEspecialidades",
                column: "EspecialidadId");

            migrationBuilder.CreateIndex(
                name: "IX_PromocionesServicios_ServicioId",
                table: "PromocionesServicios",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_EspecialidadId",
                table: "Servicios",
                column: "EspecialidadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Bloqueos");

            migrationBuilder.DropTable(
                name: "CitasServicios");

            migrationBuilder.DropTable(
                name: "Cupones");

            migrationBuilder.DropTable(
                name: "HorariosClinica");

            migrationBuilder.DropTable(
                name: "MovimientosPuntos");

            migrationBuilder.DropTable(
                name: "PersonalEspecialidades");

            migrationBuilder.DropTable(
                name: "PromocionesServicios");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Promociones");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "Personal");

            migrationBuilder.DropTable(
                name: "Especialidades");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
