# AquaClinic

Sistema de gestión de citas clínicas para **Aqua Skin and Glow Clinic** (clínica de estética y cuidado de la piel, Langley, Columbia Británica), construido con **.NET 10** sobre una arquitectura limpia (Clean Architecture + CQRS): API REST + aplicación web **Blazor Web App**.

> **Origen del proyecto:** clon modernizado de [GloboTicket.TicketManagement](https://github.com/PacktPublishing/Ultimate-ASP.NET-Core-Web-API-Second-Edition) — migrado a .NET 10 y rediseñado para el negocio real de la clínica.

---

## Características

- **Catálogo de servicios**: los clientes pueden ver los servicios de la clínica **sin registrarse** (limpieza profunda, hidrodermoabrasión, microneedling, radiofrecuencia, botox, bioestimuladores, laminado de cejas...)
- **Citas**: agendamiento con usuario registrado **o invitado** (datos de contacto), con estado (Pendiente/Confirmada/Cancelada/Completada) y asignación opcional de personal
- **Interfaz bilingüe**: inglés canadiense (`en-CA`) predeterminado y español (`es-ES`) seleccionable desde la navegación; la preferencia se conserva en una cookie
- **Ubicación de la clínica**: enlaces a mapas para Aqua Skin & Glow Clinic en Langley, BC; no se solicita geolocalización del visitante
- **Personal y horarios**: esteticistas con sus especialidades, horario semanal de la clínica y bloqueos (feriados, vacaciones)
- **Roles y permisos granulares**: Administrador, Recepcion, Esteticista y Cliente; los endpoints de escritura exigen permisos (`servicios:crear`, `especialidades:crear`...)
- **Fidelidad (esquema listo)**: promociones, cupones y puntos acumulables para futuras regalías por cliente frecuente
- **Autenticación con ASP.NET Identity** (cookies) con registro e inicio de sesión
- **Swagger**, **Serilog** (con CorrelationId), rate limiting, health checks y OpenTelemetry
- **Seed de datos de ejemplo**: 4 especialidades, 9 servicios reales con duración y precio CAD, 3 personal, horarios, bloqueos, 3 citas de invitado y 1 promoción

## Conceptos del dominio

| Entidad | Descripción |
|---|---|
| `Especialidad` | Categoría de servicios: Tratamientos Faciales, Cuidado Avanzado de la Piel, Inyectables, Cejas |
| `Servicio` | Servicio de la clínica: `Nombre`, `Descripcion`, `Precio` (CAD, decimal), `DuracionMinutos`, `ImagenUrl`, `Activo`, `EspecialidadId` |
| `Cita` | Cita agendada: `UsuarioId?` (null = invitado con `NombreInvitado/EmailInvitado/TelefonoInvitado`), `PersonalId?`, `FechaInicio/FechaFin` (**UTC**, datetimeoffset), `Estado`, `Notas` |
| `CitaServicio` | N:N citas↔servicios (1 servicio por cita hoy; el esquema permite N opcionales) |
| `Personal` | Esteticistas/administrativos: `Nombre`, `Apellidos`, contacto, `Activo`, `ApplicationUserId?` (cuenta de login vinculada, única) |
| `PersonalEspecialidad` | N:N personal↔especialidades que atiende |
| `HorarioClinica` | Horario semanal (un registro por día): `DiaSemana`, `HoraApertura/HoraCierre` (TimeOnly), `Cerrado` |
| `Bloqueo` | Cierre temporal (feriado/vacaciones): `Motivo`, rango UTC, clínico o de un personal |
| `Promocion` / `PromocionServicio` / `Cupon` | Fidelidad: descuentos (% o monto CAD) con vigencia, alcance por servicio y cupones asignables a clientes con estado |
| `MovimientoPuntos` | Puntos acumulables/canjeables por visita (saldo = suma) |

**Decisiones de diseño:**
- **UUID v7** en todos los Ids (`Guid.CreateVersion7()` en constructores) — index-friendly en SQL Server; `ApplicationUser.Id` es el v7 en string
- **Zona horaria**: fechas en UTC con `datetimeoffset`; la clínica opera en `America/Vancouver` (PST/PDT) — el cambio de horario no afecta el almacenamiento
- **Formato regional**: fechas y horas siguen el idioma seleccionado; los precios se muestran siempre en dólares canadienses (CAD)
- **Una sola base de datos** (`AquaClinicDb`): el esquema de Identity (AspNet*) vive junto al negocio

## Arquitectura

```
AquaClinic.sln
├── src
│   ├── API/              AquaClinic.Api          → Web API (controllers, Swagger, Serilog, rate limiting, permisos granulares)
│   ├── Core/             AquaClinic.Domain       → Entidades del negocio (sin dependencias)
│   │                     AquaClinic.Application   → CQRS (MediatR): Features/Servicios|Especialidades
│   ├── Infrastructure/   AquaClinic.Persistence  → EF Core 10 (AquaClinicDbContext ÚNICO: Identity + negocio)
│   │                     AquaClinic.Infrastructure→ Email (SendGrid) y exportación CSV (CsvHelper)
│   │                     AquaClinic.Identity     → ASP.NET Identity (ApplicationUser, roles, registro DI)
│   └── UI/               AquaClinic.App          → Blazor Web App (render Server interactivo + SSR estático)
└── test/                 AquaClinic.Application.UnitTests, AquaClinic.Api.IntegrationTests,
                          AquaClinic.Persistence.IntegrationTests
```

**Flujo de la UI:** la App consume la API vía REST con un cliente tipado generado por NSwag (`Services/Base/ServiceClient.cs`). Login y Registro usan SSR estático (ver [docs/mapa-comunicacion-login.md](docs/mapa-comunicacion-login.md)).

> **Nota:** las páginas del catálogo de la App aún referencian los endpoints anteriores (`/api/citas`); su adaptación a `/api/servicios` es la siguiente fase del desarrollo.

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

### 2. Ejecutar la API

```bash
dotnet run --project AquaClinic.Api
```

- API: `https://localhost:7021` (Swagger en `/swagger` solo en Development)
- Al iniciar, **recrea** `AquaClinicDb` (única: negocio + Identity), aplica la migración con el seed del catálogo real y asegura los roles + el usuario administrador demo (solo en Development)
- Ya no hay una migración separada de Identity: todo vive en el mismo `AquaClinicDbContext`

### 3. Ejecutar la App (UI)

En otra terminal:

```bash
dotnet run --project AquaClinic.App
```

- App: `https://localhost:7081`
- La App lee la URL de la API desde `AquaClinic.App/appsettings.json` (`ApiSettings:BaseUrl`)

### 4. Cuentas demo

- **Administrador (dueño):** `josvanyg@mail.com` / `JosvanyG#2026` — rol Administrador
- **Admin demo:** `admin@aquaclinic.com` / `AquaClinic#2026` — rol Administrador
- Configurables vía `AdminInicial:Email`/`Password` y `UsuarioJosvany:Email`/`Password`
- Cualquier usuario nuevo que se registra recibe el rol `Cliente`
- Los usuarios registrados se **pierden al reiniciar la API** (la BD se recrea en cada arranque en este modo demo)

## Roles y permisos

| Rol | Permisos (derivados en `Api/Authorization/PermisosPorRol.cs`) |
|---|---|
| `Administrador` | Todos (`servicios:crear/editar/eliminar`, `especialidades:crear`, `citas:*`...) |
| `Recepcion` | `servicios:ver`, `especialidades:ver`, `citas:ver`, `citas:gestionar` |
| `Esteticista` | `servicios:ver`, `especialidades:ver`, `citas:ver` |
| `Cliente` | `servicios:ver`, `especialidades:ver` |

Los endpoints declaran el permiso con `[HasPermission(Permisos.ServiciosCrear)]`; los roles viajan como claims en la cookie de Identity.

## Autenticación

El diseño usa **dos cookies con contextos separados** sobre la base de datos única (App y API comparten `AquaClinicDbContext`):

- La App autentica localmente con `SignInManager` (cookie `HttpOnly` en el navegador)
- Al iniciar sesión, la App también solicita a la API su propia cookie (vía `POST /login?useCookies=true`) y la guarda en el servidor (`UserCookieStore`) para reenviarla a los endpoints protegidos
- El diagrama completo está en [docs/mapa-comunicacion-login.md](docs/mapa-comunicacion-login.md)

## Endpoints de la API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/api/servicios` | — | Todos los servicios (catálogo público) |
| GET | `/api/servicios/{id}` | — | Detalle de un servicio |
| POST/PUT/DELETE | `/api/servicios` | `[HasPermission]` | Crear (`servicios:crear`) / actualizar (`servicios:editar`) / eliminar (`servicios:eliminar`) |
| GET | `/api/servicios/export` | — | Exportar servicios a CSV |
| GET | `/api/especialidades/all` | `especialidades:ver` | Todas las especialidades |
| GET | `/api/especialidades/allwithservicios` | `especialidades:ver` | Especialidades con sus servicios |
| POST | `/api/especialidades` | `especialidades:crear` | Crear especialidad |
| GET | `/health` | — | Health check de la base de datos |
| POST | `/api/usuarios/registro` | — | Registrar cliente (rol Cliente, rate limited) |
| POST | `/api/usuarios/login?useCookies=` | — | Login + perfil con roles (rate limited) |
| GET | `/api/usuarios/actual` | `[Authorize]` | Perfil del usuario autenticado |
| PUT | `/api/usuarios/actual` | `[Authorize]` | Actualizar perfil (nombre, apellidos, teléfono) |
| POST | `/api/usuarios/cambiar-password` | `[Authorize]` | Cambiar contraseña (rate limited) |
| POST | `/api/usuarios/olvide-password` | — | Solicitar restablecimiento (rate limited; token visible fuera de Production) |
| POST | `/api/usuarios/restablecer-password` | — | Restablecer con token (rate limited) |
| POST | `/register`, `/login`, `/logout` | — | Identity (`MapIdentityApi`) |

## Seguridad

La API incorpora las siguientes medidas (inspiradas en el template [clean-architecture](https://github.com/amantinband/clean-architecture)):

| Medida | Descripción |
|---|---|
| **Rate limiting** | Límite global de 100 req/60s por usuario autenticado o IP. Las rutas de autenticación (`/login`, `/register`, `/logout`, `/refresh`) reciben un límite estricto de 10 req/60s para frenar ataques de fuerza bruta. Responde `429 Too Many Requests`. Configurable en la sección `RateLimiting` del appsettings. |
| **CorrelationId** | Middleware que propaga el header `Correlation-Id` (o el `TraceIdentifier`) al LogContext de Serilog, para correlacionar todos los logs de una misma petición. |
| **Manejo de excepciones** | Las excepciones conocidas del dominio (validación, bad request, no encontrado) devuelven respuestas específicas; las no controladas caen en un `GlobalExceptionHandler` que responde `500 ProblemDetails` (RFC 7231) sin exponer detalles internos de la excepción. |
| **Permisos granulares** | `[HasPermission(...)]` + `PermissionAuthorizationPolicyProvider` dinámico + handler que deriva permisos de los roles de Identity (claims en la cookie). Mapeo en `Api/Authorization/PermisosPorRol.cs`. |
| **Health checks** | `GET /health` reporta en JSON el estado de la base de datos. |
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

La App usa un cliente tipado generado con NSwag a partir del swagger de la API. Para regenerarlo (con la API corriendo en `https://localhost:7021`):

```bash
dotnet tool install --global Nswag.ConsoleCore   # si no está instalado
nswag run nswagsettings.nswag
```

## Notas

- **Licencias:** AutoMapper 16 y MediatR 14 se distribuyen bajo licencia comercial (de pago para uso empresarial). Para uso personal/evaluación funcionan con una advertencia de compilación.
- **Advertencias:** el código conserva las advertencias de nulabilidad del proyecto original (propiedades sin inicializar en DTOs heredadas de la plantilla base).
- **Datos de ejemplo:** imágenes con [picsum.photos](https://picsum.photos) (requieren internet para verse).
