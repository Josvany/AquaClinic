# Mapa de comunicación de login/cuenta — actualizado

## Resumen ejecutivo

La App (Blazor Server) no usa cookies de Identity ni `UserCookieStore`/`ApiCookieHandler`. El flujo es JWT con sesión persistente vía un ticket opaco en cookie (`aqua_ticket`):

1. El usuario ingresa a /login. La página SSR consulta el `TokenStore` del circuito; si está vacío, restaura la sesión del ticket de la cookie.
2. El usuario se autentica. La página /login envía un POST a `POST api/usuarios/login` (JWT, sin `MapIdentityApi` ni cookies de Identity).
3. La API devuelve `accessToken`, `refreshToken`, `expiraUtc` y el perfil. La App guarda la sesión JWT en `SesionPendiente` (servidor), indexada por un ticket opaco que se guarda en una cookie `aqua_ticket` (httpOnly).
4. El circuito restaura el `TokenStore` desde el ticket (`TokenStoreInitializer`). Las llamadas REST usan `AuthHeaderHandler`, que inyecta `Authorization: Bearer` del `TokenStore`.
5. `SesionTicketAuthHandler` (esquema `SesionTicket`) reconstruye el `ClaimsPrincipal` para [Authorize] en SSR, igual que lo hace `TokenStoreInitializer` para el circuito interactivo. No emite cookies de sesión.

## Pistas importantes (para releer después de entrar)

- Autenticación: JWT. Login = `POST api/usuarios/login`; no hay `MapIdentityApi`.
- Sesión del navegador: cookie `aqua_ticket` (httpOnly), valor = ticket opaco; los tokens permanecen en `SesionPendiente` del servidor.
- Disponibilidad del circuito: `TokenStore` (scoped del circuito) restaurado desde el ticket; `AuthHeaderHandler` inyecta `Authorization: Bearer`.
- Autorización SSR: `SesionTicketAuthHandler`, que reconstruye el principal desde el access token del ticket; no hay cookies de sesión.
- Logout: `POST api/usuarios/logout` revoca los refresh tokens y borra el ticket.
- Servicios relacionados: `TokenStore`, `TokenStoreInitializer`, `SesionTicketAuthHandler`, `SesionPendiente`, `AuthHeaderHandler`, `ApiAuthenticationStateProvider`.
