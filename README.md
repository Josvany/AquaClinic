# AquaClinic

Sistema de gestión de citas clínicas construido con **.NET 10** sobre una arquitectura limpia (Clean Architecture + CQRS), con una API REST y una aplicación web **Blazor Web App** (render interactivo Server).

> **Origen del proyecto:** clon modernizado de [GloboTicket.TicketManagement](https://github.com/PacktPublishing/Ultimate-ASP.NET-Core-Web-API-Second-Edition) — migrado a .NET 10, con el dominio renombrado a conceptos de clínica en español (Event→Cita, Category→Especialidad, Order→Consulta, Artist→Doctor) y la UI convertida de Blazor WebAssembly standalone a Blazor Web App.

---

## Características

- **Gestión de citas**: listado, detalle, creación, edición, eliminación y exportación a CSV
- **Especialidades**: listado, especialidades con sus citas y creación
- **Consultas**: reporte paginado de consultas por mes
- **Autenticación con ASP.NET Identity** (cookies) con registro e inicio de sesión
- **Swagger** para exploración de la API
- **Serilog** para logging estructurado (consola + archivo)
- **Seed de datos de ejemplo** (4 especialidades, 6 citas, 7 consultas)

## Conceptos del dominio

| Entidad | Descripción | Propiedades |
|---|---|---|
| `Cita` | Una cita médica de la clínica | `CitaId`, `Nombre`, `Precio`, `Doctor`, `Fecha`, `Descripcion`, `ImagenUrl`, `EspecialidadId` |
| `Especialidad` | Especialidad médica (Cardiología, Pediatría...) | `EspecialidadId`, `Nombre` |
| `Consulta` | Registro de una consulta facturada | `Id`, `UserId`, `TotalConsulta`, `FechaSolicitada`, `Pagada` |

## Arquitectura

```
AquaClinic.sln
├── src
│   ├── API/              AquaClinic.Api          → Web API (controllers, Swagger, Serilog, middleware)
│   ├── Core/             AquaClinic.Domain       → Entidades y contratos del dominio
│   │                     AquaClinic.Application   → CQRS (MediatR): queries, commands, validators, DTOs
│   ├── Infrastructure/   AquaClinic.Persistence  → EF Core 10 (DbContext, repositorios, migraciones)
│   │                     AquaClinic.Infrastructure→ Email (SendGrid) y exportación CSV (CsvHelper)
│   │                     AquaClinic.Identity     → ASP.NET Identity (ApplicationUser, DbContext, migraciones)
│   └── UI/               AquaClinic.App          → Blazor Web App (render Server interactivo + SSR estático)
└── test/                 AquaClinic.Application.UnitTests, AquaClinic.Api.IntegrationTests,
                         AquaClinic.Persistence.IntegrationTests
```

**Flujo de la UI:** la App consume la API vía REST con un cliente tipado generado por NSwag (`Services/Base/ServiceClient.cs`). Las páginas de datos usan render interactivo Server; Login y Registro usan SSR estático (ver [docs/mapa-comunicacion-login.md](docs/mapa-comunicacion-login.md)).

## Tecnologías

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 10.0 | Framework base |
| ASP.NET Core Blazor | 10.0 | UI web (Blazor Web App) |
| Entity Framework Core | 10.0.12 | Persistencia (SQL Server / LocalDB) |
| ASP.NET Core Identity | 10.0.12 | Autenticación por cookies |
| MediatR | 14.2.0 | CQRS (mediador) |
| AutoMapper | 16.2.0 | Mapeo entidad ↔ DTO |
| FluentValidation | 12.1.1 | Validación de comandos |
| Serilog | 10.0.0 | Logging estructurado |
| Swashbuckle | 10.2.3 | Swagger/OpenAPI |
| SendGrid / CsvHelper | 9.29 / 33.1 | Email y exportación |
| xUnit / Moq / Shouldly | 2.9 / 4.20 / 4.3 | Pruebas |

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- **SQL Server LocalDB** (incluido con Visual Studio) o un SQL Server accesible
- (Opcional) Visual Studio 2026 con la carga de trabajo de ASP.NET

## Ejecución

### 1. Restaurar

```bash
dotnet restore AquaClinic.sln
```

### 2. Crear la base de datos de identidad (una sola vez)

La API crea y migra automáticamente `AquaClinicDb` al iniciar (con datos de ejemplo). La base de datos de **identidad** requiere aplicar su migración una única vez:

```bash
dotnet ef database update --project AquaClinic.Identity --startup-project AquaClinic.Api --context AquaClinicIdentityDbContext
```

> Requiere la herramienta `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

### 3. Ejecutar la API

```bash
dotnet run --project AquaClinic.Api
```

- API: `https://localhost:7081` (Swagger en `/swagger` solo en Development)
- Al iniciar, **recrea** `AquaClinicDb` y aplica la migración inicial con los datos de ejemplo

### 4. Ejecutar la App (UI)

En otra terminal:

```bash
dotnet run --project AquaClinic.App
```

- App: `https://localhost:7080`
- La App lee la URL de la API desde `AquaClinic.App/appsettings.json` (`ApiSettings:BaseUrl`)

### 5. Primer uso

1. Abre `https://localhost:7080` y regístrate (botón *"Haga clic aquí para registrarse"*)
2. Inicia sesión — el menú lateral (Citas, Especialidades, Nueva especialidad, Consultas) aparece al estar autenticado

> **Nota:** la API debe estar corriendo para que las páginas de la App funcionen (consume la API vía REST).

## Autenticación

El diseño usa **dos cookies con contextos separados** y una base de datos de identidad compartida entre la App y la API:

- La App autentica localmente con `SignInManager` (cookie `HttpOnly` en el navegador)
- Al iniciar sesión, la App también solicita a la API su propia cookie (vía `POST /login?useCookies=true`) y la guarda en el servidor (`UserCookieStore`) para reenviarla a los endpoints protegidos con `[Authorize]`
- El diagrama completo está en [docs/mapa-comunicacion-login.md](docs/mapa-comunicacion-login.md)

## Endpoints de la API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/api/citas` | — | Todas las citas |
| GET | `/api/citas/{id}` | — | Detalle de una cita |
| POST/PUT/DELETE | `/api/citas` | — | Crear / actualizar / eliminar cita |
| GET | `/api/citas/export` | — | Exportar citas a CSV |
| GET | `/api/especialidades/all` | `[Authorize]` | Todas las especialidades |
| GET | `/api/especialidades/allwithcitas` | `[Authorize]` | Especialidades con sus citas |
| POST | `/api/especialidades` | — | Crear especialidad |
| GET | `/getpagedconsultasformonth?date=&page=&size=` | — | Consultas paginadas por mes |
| POST | `/register`, `/login`, `/logout` | — | Identity (`MapIdentityApi`) |

## Seguridad

La API incorpora las siguientes medidas (inspiradas en el template [clean-architecture](https://github.com/amantinband/clean-architecture)):

| Medida | Descripción |
|---|---|
| **Rate limiting** | Límite global de 100 req/60s por usuario autenticado o IP. Las rutas de autenticación (`/login`, `/register`, `/logout`, `/refresh`) reciben un límite estricto de 10 req/60s para frenar ataques de fuerza bruta. Responde `429 Too Many Requests`. Configurable en la sección `RateLimiting` del appsettings. |
| **CorrelationId** | Middleware que propaga el header `Correlation-Id` (o el `TraceIdentifier`) al LogContext de Serilog, para correlacionar todos los logs de una misma petición. |
| **Manejo de excepciones** | Las excepciones conocidas del dominio (validación, bad request, no encontrado) devuelven respuestas específicas; las no controladas caen en un `GlobalExceptionHandler` que responde `500 ProblemDetails` (RFC 7231) sin exponer detalles internos de la excepción. |
| **Health checks** | `GET /health` reporta en JSON el estado de las dos bases de datos (`AquaClinicDb` y `AquaClinicIdentityDb`). |
| **Observabilidad** | OpenTelemetry con traces y métricas de ASP.NET Core, HttpClient y runtime. Exporta a OTLP solo si se define la variable `OTEL_EXPORTER_OTLP_ENDPOINT`. |

## Pruebas

```bash
dotnet test AquaClinic.sln
```

| Proyecto | Tipo | Cobertura |
|---|---|---|
| `AquaClinic.Application.UnitTests` | Unitarias | Handlers de Especialidades (con AutoMapper + Moq) |
| `AquaClinic.Api.IntegrationTests` | Integración | API en memoria con autenticación simulada (entorno `Testing`) |
| `AquaClinic.Persistence.IntegrationTests` | Integración | `AquaClinicDbContext` con EF InMemory |

## Regenerar el cliente NSwag de la UI

La App usa un cliente tipado generado con NSwag a partir del swagger de la API. Para regenerarlo (con la API corriendo en `https://localhost:7081`):

```bash
dotnet tool install --global Nswag.ConsoleCore   # si no está instalado
nswag run nswagsettings.nswag
```

## Notas

- **Licencias:** AutoMapper 16 y MediatR 14 se distribuyen bajo licencia comercial (de pago para uso empresarial). Para uso personal/evaluación funcionan con una advertencia de compilación.
- **Advertencias:** el código conserva las advertencias de nulabilidad del proyecto original (propiedades sin inicializar en DTOs heredadas de la plantilla base).
- **Datos de ejemplo:** imágenes con [picsum.photos](https://picsum.photos) (requieren internet para verse).
