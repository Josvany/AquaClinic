# AquaClinic

Sistema de gestión de citas clínicas para **Aqua Skin and Glow Clinic** (clínica de estética y cuidado de la piel, Langley, Columbia Británica), construido con **.NET 10** sobre una arquitectura limpia (Clean Architecture + CQRS): API REST + aplicación web **Blazor Web App**.


---

## Características

- **Catálogo de servicios**: los clientes pueden ver los servicios de la clínica **sin registrarse** (limpieza profunda, hidrodermoabrasión, microneedling, radiofrecuencia, botox, bioestimuladores, laminado de cejas...)
- **Citas**: agendamiento con usuario registrado **o invitado** (datos de contacto), con estado (Pendiente/Confirmada/Cancelada/Completada) y asignación opcional de personal
- **Interfaz bilingüe**: inglés canadiense (`en-CA`) predeterminado y español (`es-ES`) seleccionable desde la navegación; la preferencia se conserva en una cookie
- **Ubicación de la clínica**: enlaces a mapas para Aqua Skin & Glow Clinic en Langley, BC; no se solicita geolocalización del visitante
- **Personal y horarios**: esteticistas con sus especialidades, horario semanal de la clínica y bloqueos (feriados, vacaciones)
- **Roles y permisos granulares**: Administrador, Recepcion, Esteticista y Cliente; los endpoints de escritura exigen permisos (`servicios:crear`, `especialidades:crear`...)
- **Fidelidad (esquema listo)**: promociones, cupones y puntos acumulables para futuras regalías por cliente frecuente
- **Autenticación con ASP.NET Identity** (usuarios y roles) y sesión por JWT: la API emite el token y la App lo conserva cifrado en una cookie `httpOnly` de 7 días
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
| `RefreshToken` | Refresh tokens de la sesión: `TokenHash` (SHA-256, nunca el token en claro), `UsuarioId`, `ExpiraUtc`. Se canjean y se borran en el mismo uso |

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

**Flujo de la UI:** la App consume la API vía REST con un cliente tipado generado por NSwag (`Services/Base/ServiceClient.cs`). Login y Registro usan SSR estático (ver la sección Autenticación más abajo).

> **Nota:** las páginas del catálogo de la App aún referencian los endpoints anteriores (`/api/citas`); su adaptación a `/api/servicios` es la siguiente fase del desarrollo.

## Tecnologías

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 10.0 | Framework base |
| ASP.NET Core Blazor | 10.0 | UI web (Blazor Web App) |
| Entity Framework Core | 10.0.12 | Persistencia (SQL Server / LocalDB) |
| ASP.NET Core Identity | 10.0.12 | Usuarios, roles y hashing de contraseñas |
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

- API: `https://localhost:7022` (Swagger en `/swagger` solo en Development)
- Al iniciar, **recrea** `AquaClinicDb` (única: negocio + Identity), aplica la migración con el seed del catálogo real y asegura los roles + el usuario administrador demo (solo en Development)
- Ya no hay una migración separada de Identity: todo vive en el mismo `AquaClinicDbContext`

### 3. Ejecutar la App (UI)

En otra terminal:

```bash
dotnet run --project AquaClinic.App
```

- App: `https://localhost:7082`
- La App lee la URL de la API desde `AquaClinic.App/appsettings.json` (`ApiSettings:BaseUrl`)

### 4. Cuentas demo
- **Admin demo:** `admin@aquaclinic.com` / `AquaClinic#2026` — rol Administrador
- Cualquier usuario nuevo que se registra recibe el rol `Cliente`

## Roles y permisos

| Rol | Permisos (derivados en `Api/Authorization/PermisosPorRol.cs`) |
|---|---|
| `Administrador` | Todos (`servicios:crear/editar/eliminar`, `especialidades:crear`, `citas:*`...) |
| `Recepcion` | `servicios:ver`, `especialidades:ver`, `citas:ver`, `citas:gestionar` |
| `Esteticista` | `servicios:ver`, `especialidades:ver`, `citas:ver` |
| `Cliente` | `servicios:ver`, `especialidades:ver` |

Los endpoints declaran el permiso con `[HasPermission(Permisos.ServiciosCrear)]`; los roles viajan como claims en el JWT.

## Autenticación

Login y Registro son SSR estático: el formulario hace `POST /auth/login` en la App, que pide el JWT a la API y lo guarda **cifrado en una cookie `httpOnly`** (`aqua_sesion`, 7 días) mediante Data Protection.

- **No hay estado de sesión en el servidor.** La cookie *es* la sesión: sobrevive a recargas, al cierre del navegador y a reinicios del proceso de la App
- Los tokens viajan en la cookie cifrados con `SesionCookieStore` (`App/Auth/`); el correo y los roles se leen de los claims del propio access token
- La App los copia al `TokenStore` del circuito (`AuthHeaderHandler` los inyecta como `Authorization: Bearer`); al abrir un circuito nuevo, `TokenStoreInitializer` los recupera de la cookie
- Al vencer el access token (60 min) hay refresh automático en ambos caminos (SSR y circuito); el refresh token vive 7 días y es de un solo uso, así que al rotarlo se reescribe la cookie
- **Los refresh tokens se guardan hasheados en la tabla `RefreshTokens`** (SHA-256, nunca en claro). Antes vivían en memoria y reiniciar la API cerraba todas las sesiones; ahora sobreviven
- **Las claves de Data Protection deben persistir en disco.** Por defecto van al perfil del usuario (`%LOCALAPPDATA%\ASP.NET\DataProtection-Keys`); en un contenedor hay que montar ese directorio en un volumen o la cookie dejaría de descifrarse al reiniciar
- Reiniciar el App o la API **no** expulsa al usuario; recargar o reabrir el navegador tampoco
- Trade-off del logout: al no haber estado de sesión en servidor, cerrar sesión solo borra la cookie del navegador. Una copia del valor cifrado podría reusarse hasta que venza el access token (60 min); el logout sí revoca el refresh token en la API, así que pasado ese plazo la sesión ya no se renueva

## Seguridad

La API incorpora las siguientes medidas

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

La App usa un cliente tipado generado con NSwag a partir del swagger de la API. Para regenerarlo (con la API corriendo en `https://localhost:7022`):

```bash
dotnet tool install --global Nswag.ConsoleCore   # si no está instalado
nswag run nswagsettings.nswag
```
