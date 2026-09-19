# AGENTS.md — AquaClinic

Guía operativa para agentes de IA trabajando en este repositorio.

## Descripción del proyecto

Sistema de gestión de citas clínicas en **.NET 10** con Clean Architecture + CQRS (MediatR). API REST + Blazor Web App con el **dominio renombrado a español**

## Comandos

```bash
# Compilar toda la solucion
dotnet build AquaClinic.sln

# Ejecutar todos los tests
# dotnet test AquaClinic.sln

# Arrancar la API (Swagger en /swagger solo en Development)
dotnet run --project AquaClinic.Api
#   http  -> http://localhost:5224   (perfil "http", primero en launchSettings)
#   https -> https://localhost:7081  (perfil "https")
#   Al iniciar recrea AquaClinicDb (EnsureDeleted + Migrate) con datos de ejemplo

# Arrancar la App Blazor (requiere la API corriendo)
dotnet run --project AquaClinic.App      # https://localhost:7080

# Migracion de la BD de identidad (UNA SOLA VEZ; la API NO la aplica sola)
dotnet ef database update --project AquaClinic.Identity --startup-project AquaClinic.Api --context AquaClinicIdentityDbContext

# Regenerar la migracion de Persistence (borrar antes la carpeta AquaClinic.Persistence/Migrations)
dotnet ef migrations add InitialMigration --project AquaClinic.Persistence --startup-project AquaClinic.Api --context AquaClinicDbContext

# Regenerar el cliente tipado de la UI (requiere la API corriendo y nswag global)
nswag run nswagsettings.nswag
```

## Arquitectura

```
src
  API/            AquaClinic.Api           Web API (controllers, middleware de seguridad, Swagger, Serilog)
  Core/           AquaClinic.Domain        Entidades (Cita, Especialidad, Consulta) — sin dependencias
                  AquaClinic.Application   CQRS con MediatR: Features/Citas|Especialidades|Consultas
  Infrastructure/ AquaClinic.Persistence   EF Core 10 (AquaClinicDbContext, repositorios, migraciones)
                  AquaClinic.Infrastructure Email (SendGrid) + export CSV (CsvHelper)
                  AquaClinic.Identity      ASP.NET Identity (ApplicationUser, AquaClinicIdentityDbContext)
  UI/             AquaClinic.App           Blazor Web App: Server interactivo por pagina; Login/Registro SSR estatico
test              Application.UnitTests, Api.IntegrationTests, Persistence.IntegrationTests
```

- **Dos bases de datos** (LocalDB): `AquaClinicDb` (dominio) y `AquaClinicIdentityDb` (usuarios), compartidas por la API y la App.
- **La UI consume la API via REST** con un cliente tipado NSwag (`AquaClinic.App/Services/Base/ServiceClient.cs`). No referencia Application/Persistence directamente.
- **Autenticación**: cookies de ASP.NET Identity. La App valida localmente y guarda la cookie de la API en servidor (`UserCookieStore`) para reenviarla a los endpoints `[Authorize]`. Diagrama completo: `docs/mapa-comunicacion-login.md`.

## Glosario de dominio (obligatorio)

| Español | Prohibido (original) |
|---|---|
| Cita | Event |
| Especialidad | Category |
| Consulta | Order |
| Doctor | Artist |
| Nombre, Precio, Fecha, Descripcion, ImagenUrl, EspecialidadId, CitaId, TotalConsulta, FechaSolicitada, Pagada | Name, Price, Date, Description, ImageUrl, CategoryId, EventId, OrderTotal, OrderPlaced, OrderPaid |

Endpoints: `/api/citas`, `/api/especialidades`, `/getpagedconsultasformonth`. UI y mensajes de validacion en español.

## Reglas y advertencias criticas

1. **NUNCA uses `DateTime.Now` en `HasData`** (seed). EF Core 9/10 lanza `PendingModelChangesWarning` en `MigrateAsync` y el arranque de la API falla silenciosamente. Usa fechas `new DateTime(...)` literales (ver seed de `AquaClinicDbContext`).
2. **No elimines el guard de entorno `Testing`** en `AquaClinic.Api/Program.cs`: los tests de integración dependen de que `ResetDatabaseAsync` no se ejecute en el host de pruebas.
3. **Los tests de integración de la API** (`CustomWebApplicationFactory`) sobreescriben el esquema de autenticación con `TestAuthHandler` (necesario para `[Authorize]`) y reemplazan SqlServer por InMemory vía `RemoveAll(IDbContextOptionsConfiguration<...>)` + `AddDbContext`. El seeding se hace con `SeedDatabase()` sobre `factory.Services`, no con un provider aparte.
4. **Rate limiting**: particionado por usuario autenticado o IP. La clave de partición lleva prefijo `auth:`/`global:` — obligatorio porque `PartitionedRateLimiter` cachea el limiter por clave (sin prefijo, la particion estricta contaminaria la global). Las rutas de autenticación de `MapIdentityApi` se limitan por detección de path en el `GlobalLimiter`.
5. **Excepciones**: `ExceptionHandlerMiddleware` solo captura Validation/BadRequest/NotFound; el resto sube a `GlobalExceptionHandler` (500 ProblemDetails). No agregues catch-all con `exception.Message` al response (fuga de información).
6. **Versionado de paquetes** (breaking changes ya resueltos, no regresar):
   - AutoMapper 16: `AddAutoMapper(cfg => { }, assemblies)` y `new MapperConfiguration(cfg, NullLoggerFactory.Instance)`.
   - CsvHelper 33: `new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture))`.
   - Swashbuckle 10 / Microsoft.OpenApi 2: tipos en `Microsoft.OpenApi` (sin `.Models`); `OpenApiSchema.Type` es enum `JsonSchemaType`.
   - `RevalidatingServerAuthenticationStateProvider` (.NET 10) recibe `ILoggerFactory`, no `ILogger<T>`.
7. **Correo**: `EmailService` usa `EmailAddress.Name` (propiedad de SendGrid, NO traducir a `Nombre`).
8. **Clases CSS del App** (`event-name`, `category-event-header`, `ticket-sales-table`, etc.) se mantienen en inglés a propósito: están definidas en `wwwroot/css/*.css` y renombrarlas sin tocar el CSS rompe los estilos.

## Entorno de desarrollo (Windows)

  ```powershell
  $env:ConnectionStrings__AquaClinicConnectionString = "Server=(localdb)\aquaclinic;Database=AquaClinicDb;Trusted_Connection=True;"
  $env:ConnectionStrings__AquaClinicIdentityConnectionString = "Server=(localdb)\aquaclinic;Database=AquaClinicIdentityDb;Trusted_Connection=True;"
  ```

- OpenTelemetry solo exporta si existe `OTEL_EXPORTER_OTLP_ENDPOINT` (no requiere backend local).
- Serilog: consola + `./logs/log-.txt`. El template incluye `{Properties:j}` para que el `CorrelationId` sea visible — no lo quites.

## Documentacion de referencia

- `README.md` — puesta en marcha, endpoints, stack, sección de seguridad.
- `docs/mapa-comunicacion-login.md` — diseño de autenticación (dos cookies, diagramas Mermaid).
- `.github/copilot-instructions.md` — heredado del proyecto original (irrelevante para este dominio).



## Alcance del trabajo

Desarrolla únicamente la funcionalidad solicitada en la tarea actual.

No implementes funciones adicionales, mejoras no solicitadas ni características ralacionadas que pueden parecer útiles pero que no hayan sido pedidas explicitamente.

No te adelantes a desarrolar funcionalidades futuras, aunque parezcan una continuación lógica de la tarea actual.

No agregues pantallas, componentes, campos, acciones, validaciones, filtros, botones, rutas, estados ni comportamientos que no sean necesarios para completar exactamente la solicitud actual.

Si una funcionalidad relacionada será necesaria más adelante déjala pendiente hasta que sea solicitada explícitamente.

Mantén cada cambio estrictamente limitado al alcance de la tarea solicitada y evita refactorizaciones o modificaciones adicionales que no sean necesarias para completarla.

## Seguridad y Configuración

No incluyas credenciales, tokens ni configuraciones especificas del entorno en los commits. Utiliza variables de entorno de Vite únicamente cuando sea necesario y mantén los archivos locales que contengan secretos fuera del control de versiones. Revisa `.gitignore` antes de agregar nuevos archivos de configuración.


<!-- ## Al terminar cambios

1. `dotnet build AquaClinic.sln` → 0 errores (los ~120 warnings CS86xx de nulabilidad son del código heredado del original; no es necesario eliminarlos, pero tampoco agregar nuevos).
2. `dotnet test AquaClinic.sln` → todos en verde (5 tests: 2 unitarios + 3 de integración, incluye `RateLimitingTests`).
3. Si tocaste la UI, verifica que los nombres de ruta sigan en español (`/citaoverview`, `/citadetails`, `/especialidadoverview`, `/addespecialidad`, `/consultas`).
4. Si cambiaste el modelo de dominio: regenera la migración de Persistence (ver comandos) y verifica que el seed no use valores dinámicos. -->
