# Mapa de comunicación — Login en AquaClinic

## Actores y componentes

| Componente | Ubicación | Responsabilidad |
|---|---|---|
| Navegador | Cliente | Envía/recibe cookies, renderiza |
| `Login.razor` / `Register.razor` | AquaClinic.App | Páginas con **SSR estático** (form POST real) |
| `UserManager` / `SignInManager` | AquaClinic.App (Identidad local) | Validar credenciales y emitir cookie LOCAL |
| `AquaClinicDbContext` | App **y** API | Ambos apuntan a la misma BD única `AquaClinicDb` (Identity + negocio) |
| Cliente `"Authentication"` | AquaClinic.App | HttpClient hacia la API para "acuñar" la cookie de la API |
| `UserCookieStore` | AquaClinic.App (singleton en memoria) | Guarda `userId → cookie de la API` |
| `ApiCookieHandler` | AquaClinic.App (por circuito) | Adjunta esa cookie en las llamadas salientes a la API |
| `IdentityRevalidatingAuthenticationStateProvider` | AquaClinic.App | Estado de autenticación en circuitos interactivos (revalida cada 30 min) |
| `MapIdentityApi<ApplicationUser>()` | AquaClinic.Api | `POST /login?useCookies=true`, `/register`, `/manage/info` |
| Endpoints `[Authorize]` | AquaClinic.Api | `/api/especialidades/all` y `/api/especialidades/allwithcitas` |
| SQL LocalDB | Servidor | `AquaClinicDb` (única: usuarios AspNet* + citas/especialidades/servicios) |

## Las dos cookies

| Cookie | La emite | Vive en | Sirve para |
|---|---|---|---|
| `.AspNetCore.Identity.Application` (App) | `SignInManager` de la App | **Navegador** (HttpOnly) | Autenticar las páginas de la App |
| `.AspNetCore.Identity.Application` (API) | `MapIdentityApi` de la API | **Memoria del servidor App** (`UserCookieStore`) | Reenviarla a los endpoints protegidos de la API |

> La App y la API comparten la misma BD única (`AquaClinicDb`), por eso las mismas credenciales funcionan en los dos lados **sin necesidad de compartir claves de Data Protection**.

## 1. Secuencia de login

```mermaid
sequenceDiagram
    participant U as Navegador
    participant L as Login.razor (SSR estatico)
    participant SG as SignInManager (App)
    participant DB as AquaClinicDb (única: Identity + negocio)
    participant API as AquaClinic.Api
    participant ST as UserCookieStore (memoria)

    U->>L: GET /login
    L-->>U: formulario HTML (estatico)
    U->>L: POST /login (antiforgery, HTTP completo)
    L->>SG: FindByEmailAsync + CheckPasswordAsync
    SG->>DB: consulta AspNetUsers
    DB-->>SG: usuario valido
    SG-->>U: Set-Cookie .AspNetCore.Identity.Application (cookie LOCAL)
    L->>API: POST login?useCookies=true {email, password}
    API->>DB: valida credenciales (misma BD)
    DB-->>API: ok
    API-->>L: 200 + Set-Cookie .AspNetCore.Identity.Application (cookie de la API)
    L->>ST: Set(userId, cookieDeLaApi)
    L-->>U: 302 -> / (con cookie local en el navegador)
```

## 2. Mapa de componentes

```mermaid
flowchart LR
    U["Navegador"]

    subgraph APP["AquaClinic.App :7080 (Blazor Web App)"]
        L["Login.razor / Register.razor<br/>(SSR estatico)"]
        P["Paginas interactivas<br/>(CitaOverview, EspecialidadOverview...)"]
        RVP["IdentityRevalidating<br/>AuthenticationStateProvider"]
        SVC["Servicios de datos<br/>(CitaDataService...)"]
        NSC["Cliente NSwag IClient<br/>(HttpClient por circuito)"]
        ACH["ApiCookieHandler"]
        ST["UserCookieStore<br/>(singleton en memoria)"]
        ID1["AquaClinicDbContext (App)"]
    end

    subgraph API["AquaClinic.Api :7081"]
        MIA["POST /login?useCookies=true<br/>(MapIdentityApi)"]
        EP1["Endpoints protegidos (Authorize)<br/>/api/especialidades/*"]
        EP2["Endpoints anonimos<br/>/api/citas, /api/consultas"]
        ID2["AquaClinicDbContext (API)"]
    end

    DBI[("AquaClinicDb (única)")]
    DBC[("AquaClinicDb")]

    U -- "cookie LOCAL (HttpOnly)" --> APP
    U -- "GET / paginas" --> APP
    L --> ID1 --> DBI
    L -- "email + password" --> MIA
    MIA --> ID2 --> DBI
    MIA -. "Set-Cookie (API)" .-> ST
    P --> RVP
    P --> SVC --> NSC --> ACH
    ACH -. "lee cookie por userId" .-> ST
    ACH -- "GET + header Cookie" --> EP1
    NSC -- "sin cookie" --> EP2
    EP1 --> DBC
    EP2 --> DBC
```

## 3. Petición a un endpoint protegido desde un circuito interactivo

```mermaid
sequenceDiagram
    participant U as Navegador
    participant C as Circuito Blazor (EspecialidadOverview)
    participant SVC as EspecialidadDataService
    participant ACH as ApiCookieHandler
    participant ST as UserCookieStore
    participant API as AquaClinic.Api
    participant DB as AquaClinicDb

    U->>C: navega a /especialidadoverview
    Note over C: el usuario del circuito viene del prerender<br/>(IdentityRevalidatingAuthenticationStateProvider)
    C->>SVC: GetAllEspecialidadesWithCitas(incluirHistorial)
    SVC->>ACH: GetAllEspecialidadesWithCitasAsync(...)
    ACH->>ST: Get(userId)
    ST-->>ACH: cookie .AspNetCore.Identity.Application de la API
    ACH->>API: GET /api/especialidades/allwithcitas<br/>+ header "Cookie: .AspNetCore..."
    API->>API: [Authorize] valida la cookie con SU DataProtection
    API->>DB: consulta Especialidades + Citas
    DB-->>API: datos
    API-->>ACH: 200 OK (JSON)
    ACH-->>SVC: EspecialidadCitaListVm[]
    SVC-->>C: EspecialidadCitasViewModel[]
    C-->>U: tabla renderizada (via SignalR)
```

## 4. Logout

```mermaid
sequenceDiagram
    participant U as Navegador
    participant EP as POST /logout (endpoint minimo)
    participant ST as UserCookieStore
    participant SG as SignInManager (App)

    U->>EP: submit del formulario en Index.razor (antiforgery)
    EP->>ST: Remove(userId)
    EP->>SG: SignOutAsync()
    SG-->>U: cookie LOCAL expirada (Set-Cookie pasado)
    EP-->>U: 302 -> /
    Note over ST,API: la cookie de la API deja de reenviarse<br/>(se elimino la copia almacenada)
```

## 5. Registro

Idéntico al login, con un paso previo:

1. `UserManager.CreateAsync(user, password)` → crea el usuario en `AquaClinicDb` (tabla AspNetUsers)
2. `SignInManager.SignInAsync(...)` → cookie LOCAL en el navegador
3. `GuardarCookieDeLaApiAsync(...)` → acuña y guarda la cookie de la API
4. Redirect a `/`

## Decisiones de diseño y por qué

| Decisión | Motivo |
|---|---|
| Login/Registro con **SSR estático** (form POST) | Una cookie solo puede escribirse en una respuesta HTTP real; los circuitos interactivos **no pueden** establecer cookies en el navegador |
| Cookie de la API guardada **en el servidor** | Los circuitos Blazor Server no tienen acceso a las cookies del navegador; el App necesita la cookie de la API en el servidor para reenviarla |
| `UserCookieStore` singleton **en memoria** | Simple y suficiente para un único servidor. Trade-off: si se reinicia la App, el usuario debe volver a iniciar sesión (los endpoints anónimos siguen funcionando) |
| Misma BD de identidad en App y API | Evita compartir claves de DataProtection entre aplicaciones; cada lado valida sus propias cookies |
| `ApiCookieHandler` resuelto **por circuito** | Puede consultar el `AuthenticationStateProvider` del circuito actual para saber qué usuario hace la petición |
| Revalidación cada 30 min | `IdentityRevalidatingAuthenticationStateProvider` compara la marca de seguridad del usuario contra la BD (bloqueo, cambio de contraseña, etc.) |
