# AGENTS.md — AquaClinic

Guía operativa para agentes de IA trabajando en este repositorio.

## Descripción del proyecto

Sistema de gestión de citas para **Aqua Skin and Glow Clinic** (clínica de estética, Langley BC) en **.NET 10** con Clean Architecture + CQRS (MediatR). API REST + Blazor Web App. **Una sola base de datos** (`AquaClinicDb`): el esquema de ASP.NET Identity (AspNet*) vive dentro de `AquaClinicDbContext`. Los clientes pueden ver el catálogo de servicios sin registrarse y agendar como invitados o usuarios registrados.

## Comandos

```bash
# Compilar toda la solucion
dotnet build AquaClinic.sln

# Ejecutar todos los tests
dotnet test AquaClinic.sln

# Arrancar la API (Swagger en /swagger solo en Development)
dotnet run --project AquaClinic.Api
#   http  -> http://localhost:5224   (perfil "http", primero en launchSettings)
#   https -> https://localhost:7081  (perfil "https")
#   Al iniciar recrea AquaClinicDb (EnsureDeleted + Migrate, UNICO contexto con
#   Identity + negocio), siembra roles y, en Development, el admin demo.

# Arrancar la App Blazor (requiere la API corriendo)
dotnet run --project AquaClinic.App      # https://localhost:7080

# Regenerar la migracion de Persistence (borrar antes la carpeta AquaClinic.Persistence/Migrations)
#   IMPORTANTE: la migracion NUEVA solo existe en el DLL despues de un build.
#   Secuencia: dotnet ef migrations add ... -> dotnet build -> dotnet ef database update
dotnet ef migrations add InitialMigration --project AquaClinic.Persistence --startup-project AquaClinic.Api --context AquaClinicDbContext

# Aplicar la migracion manualmente (la API lo hace sola al arrancar)
dotnet ef database update --project AquaClinic.Persistence --startup-project AquaClinic.Api --context AquaClinicDbContext

# Regenerar el cliente tipado de la UI (requiere la API corriendo y nswag global)
nswag run nswagsettings.nswag
```

## Arquitectura

```
src
  API/            AquaClinic.Api           Web API (controllers, middleware de seguridad, Swagger, Serilog,
                                          Authorization/ con permisos granulares [HasPermission])
  Core/           AquaClinic.Domain        Entidades del negocio — sin dependencias
                  AquaClinic.Application   CQRS con MediatR: Features/Servicios|Especialidades
  Infrastructure/ AquaClinic.Persistence   EF Core 10 (AquaClinicDbContext UNICO: Identity + negocio)
                  AquaClinic.Infrastructure Email (SendGrid) + export CSV (CsvHelper)
                  AquaClinic.Identity      ApplicationUser + RolesClinica + AddIdentityServices<TDbContext>
  UI/             AquaClinic.App           Blazor Web App: Server interactivo por pagina; Login/Registro SSR estatico
test              Application.UnitTests, Api.IntegrationTests, Persistence.IntegrationTests
```

- **Una sola base de datos** (LocalDB): `AquaClinicDb` con esquema AspNet* + negocio. `AquaClinicDbContext : IdentityDbContext<ApplicationUser>` (Persistence referencia Identity).
- **La UI consume la API via REST** con un cliente tipado NSwag (`AquaClinic.App/Services/Base/ServiceClient.cs`). No referencia Application/Persistence directamente.
- **Autenticación**: cookies de ASP.NET Identity. La App valida localmente y guarda la cookie de la API en servidor (`UserCookieStore`) para reenviarla a los endpoints con `[HasPermission]`. Diagrama completo: `docs/mapa-comunicacion-login.md`.
- **Roles y permisos**: Administrador/Recepcion/Esteticista/Cliente (`RolesClinica` en Identity) → permisos granulares (`Permisos` + `PermisosPorRol` en `Api/Authorization`). Los roles viajan como claims en la cookie.

## Glosario de dominio (obligatorio)

| Concepto | Significado |
|---|---|
| `Servicio` | Servicio del catálogo de la clínica (con `DuracionMinutos`, `Precio` decimal CAD, `Activo`). Antes era el "Event" del template |
| `Especialidad` | Categoría de servicios (Tratamientos Faciales, Cuidado Avanzado de la Piel, Inyectables, Cejas) |
| `Cita` | Cita agendada (UsuarioId? = invitado con contacto, PersonalId?, Fechas UTC datetimeoffset, Estado enum) |
| `CitaServicio` | N:N citas↔servicios (multi opcional; hoy 1 por cita) |
| `Personal` | Esteticistas/administrativos con `ApplicationUserId?` (cuenta login vinculada, única) |
| `HorarioClinica` / `Bloqueo` | Horario semanal por día (TimeOnly) / cierres (feriados, vacaciones) |
| `Promocion` / `Cupon` / `MovimientoPuntos` | Fidelidad futura (solo esquema, sin features aún) |
| `Usuario` | Modelo de dominio del cliente (NO mapeado por EF; la tabla real es AspNetUsers). Se materializa desde UserManager en Identity |

Prohibido usar los nombres del template original (Event, Category, Order, Artist). UI y mensajes de validación en español.

## Reglas y advertencias criticas

1. **NUNCA uses `DateTime.Now` en `HasData`** (seed). EF Core 9/10 lanza `PendingModelChangesWarning` en `MigrateAsync` y el arranque de la API falla silenciosamente. Usa fechas `new DateTime(...)` / `new DateTimeOffset(...)` literales (ver seed de `AquaClinicDbContext`).
2. **No elimines el guard de entorno `Testing`** en `AquaClinic.Api/Program.cs`: los tests de integración dependen de que `ResetDatabaseAsync` no se ejecute en el host de pruebas.
3. **Los Ids son UUID v7** generados en el constructor de cada entidad (`Guid.CreateVersion7()`); no uses `Guid.NewGuid()` para entidades nuevas. `ApplicationUser` usa el v7 en string. Los FKs a AspNetUsers son `string` (no Guid) porque IdentityUser.Id es string.
4. **Fechas de citas y bloqueos en UTC** (`DateTimeOffset` → `datetimeoffset`). La clínica opera en America/Vancouver (PST/PDT); la conversión es responsabilidad de la capa de aplicación, nunca cambies el storage.
5. **Una sola BD / un solo contexto**: no crees contextos adicionales ni consultes `AquaClinicIdentityDb` (ya no existe). El registro de identity es `AddIdentityServices<AquaClinicDbContext>()` (genérico, en Identity).
6. **Permisos**: los endpoints protegidos usan `[HasPermission(Permisos.X)]` (no `[Authorize(Roles=...)]`). Para agregar un permiso: constante en `Api/Authorization/Permisos.cs` + fila en `PermisosPorRol.cs`. El `PermissionProvider` lee los roles de los claims de la cookie (no consulta la BD).
7. **Cuenta de usuarios**: los endpoints de cuenta están en `Api/Controllers/UsuariosController.cs` con features CQRS en `Application/Features/Usuarios/`. La abstracción `IIdentityService` está en `Application/Contracts/Authentication/` y la implementa `Identity/IdentityService.cs` con `UserManager<ApplicationUser>`. El login de la App usa `SignInManager` local (cookie navegador) + mint de cookie de la API (`UserCookieStore`). El perfil/cambio de contraseña usan `IUsuarioDataService` → HttpClient con `ApiCookieHandler`.
7. **Los tests de integración de la API** (`CustomWebApplicationFactory`) sobreescriben el esquema de autenticación con `TestAuthHandler` (necesario para `[HasPermission]`) y reemplazan SqlServer por InMemory vía `RemoveAll(IDbContextOptionsConfiguration<...>)` + `AddDbContext`. El seeding se hace con `SeedDatabase()` sobre `factory.Services` (incluye `EnsureCreated` para que InMemory aplique el HasData), y `Utilities` es idempotente.
8. **Rate limiting**: particionado por usuario autenticado o IP. La clave de partición lleva prefijo `auth:`/`global:` — obligatorio porque `PartitionedRateLimiter` cachea el limiter por clave (sin prefijo, la particion estricta contaminaria la global). Las rutas de autenticación de `MapIdentityApi` se limitan por detección de path en el `GlobalLimiter`.
9. **Excepciones**: `ExceptionHandlerMiddleware` solo captura Validation/BadRequest/NotFound; el resto sube a `GlobalExceptionHandler` (500 ProblemDetails). No agregues catch-all con `exception.Message` al response (fuga de información).
10. **Versionado de paquetes** (breaking changes ya resueltos, no regresar):
    - AutoMapper 16: `AddAutoMapper(cfg => { }, assemblies)` y `new MapperConfiguration(cfg, NullLoggerFactory.Instance)`.
    - CsvHelper 33: `new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture))`.
    - Swashbuckle 10 / Microsoft.OpenApi 2: tipos en `Microsoft.OpenApi` (sin `.Models`); `OpenApiSchema.Type` es enum `JsonSchemaType`.
    - `RevalidatingServerAuthenticationStateProvider` (.NET 10) recibe `ILoggerFactory`, no `ILogger<T>`.
    - `IdentityDbContext<T>` está en el namespace `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
11. **Correo**: `EmailService` usa `EmailAddress.Name` (propiedad de SendGrid, NO traducir a `Nombre`).
12. **Clases CSS del App** (`event-name`, `category-event-header`, `ticket-sales-table`, etc.) se mantienen en inglés a propósito: están definidas en `wwwroot/css/*.css` y renombrarlas sin tocar el CSS rompe los estilos.
13. **EF en consola**: la clave de las tablas compuestas (CitaServicio, PersonalEspecialidad, PromocionServicio) se configura con `HasKey(x => new { ... })`. Para llaves simples con nombre no convencional (HorarioId, MovimientoId) declara `HasKey` explícito.

## Entorno de desarrollo (Windows)

- SQL LocalDB. **OJO en la máquina del dueño**: la instancia `(localdb)\MSSQLLocalDB` tiene un trigger de logon que bloquea el login de Windows ("Logon failed ... due to trigger execution"). Se creó la instancia limpia **`(localdb)\aquaclinic`**; para usarla, sobreescribe con variables de entorno (los archivos del proyecto quedan con los valores estándar). Ya no existe un connection string de identity: hay uno solo.

  ```powershell
  $env:ConnectionStrings__AquaClinicConnectionString = "Server=(localdb)\aquaclinic;Database=AquaClinicDb;Trusted_Connection=True;"
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
