using AquaClinic.Application.Contracts.Authentication;
using AquaClinic.Domain.Entities;
using AquaClinic.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace AquaClinic.Identity
{
    /// <summary>
    /// Implementacion de las operaciones de cuenta sobre UserManager<ApplicationUser>.
    /// La tabla fisica es AspNetUsers (esquema de Identity en AquaClinicDbContext).
    /// </summary>
    public class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
    {
        public async Task<ResultadoRegistro> RegistrarAsync(string email, string password, string? nombre, string? apellidos, string? telefono)
        {
            var existente = await userManager.FindByEmailAsync(email);
            if (existente is not null)
            {
                return ResultadoRegistro.Fallo("El correo ya esta; registrado.");
            }

            var usuario = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Nombre = nombre,
                Apellidos = apellidos
            };

            var resultado = await userManager.CreateAsync(usuario, password);
            if (!resultado.Succeeded)
            {
                return ResultadoRegistro.Fallo(Traducir(resultado));
            }

            if (!string.IsNullOrWhiteSpace(telefono))
            {
                await userManager.SetPhoneNumberAsync(usuario, telefono);
            }

            await userManager.AddToRoleAsync(usuario, RolesClinica.Cliente);

            return ResultadoRegistro.Ok(usuario.Id);
        }

        public async Task<Usuario?> ValidarCredencialesAsync(string email, string password)
        {
            var usuario = await userManager.FindByEmailAsync(email);

            if (usuario is null || !await userManager.CheckPasswordAsync(usuario, password))
            {
                return null;
            }

            return await MaterializarAsync(usuario);
        }

        public async Task<Usuario?> ObtenerPorIdAsync(string usuarioId)
        {
            var usuario = await userManager.FindByIdAsync(usuarioId);

            return usuario is null ? null : await MaterializarAsync(usuario);
        }

        public async Task<IReadOnlyList<Usuario>> BuscarClientesAsync(string? texto, int limite)
        {
            var clientes = await userManager.GetUsersInRoleAsync(RolesClinica.Cliente);
            var filtro = texto?.Trim();

            var coincidencias = clientes
                .Where(u => string.IsNullOrWhiteSpace(filtro)
                    || (u.Nombre ?? string.Empty).Contains(filtro, StringComparison.OrdinalIgnoreCase)
                    || (u.Apellidos ?? string.Empty).Contains(filtro, StringComparison.OrdinalIgnoreCase)
                    || (u.Email ?? string.Empty).Contains(filtro, StringComparison.OrdinalIgnoreCase))
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellidos)
                .Take(limite <= 0 ? 20 : limite)
                .ToList();

            var resultado = new List<Usuario>(coincidencias.Count);
            foreach (var cliente in coincidencias)
            {
                resultado.Add(await MaterializarAsync(cliente));
            }

            return resultado;
        }

        public async Task<ResultadoIdentity> CambiarPasswordAsync(string usuarioId, string passwordActual, string passwordNueva)
        {
            var usuario = await userManager.FindByIdAsync(usuarioId);

            if (usuario is null)
            {
                return ResultadoIdentity.Fallo("Usuario no encontrado.");
            }

            var resultado = await userManager.ChangePasswordAsync(usuario, passwordActual, passwordNueva);

            return resultado.Succeeded
                ? ResultadoIdentity.Ok()
                : ResultadoIdentity.Fallo(Traducir(resultado));
        }

        public async Task<ResultadoIdentity> ActualizarPerfilAsync(string usuarioId, string? nombre, string? apellidos, string? telefono)
        {
            var usuario = await userManager.FindByIdAsync(usuarioId);

            if (usuario is null)
            {
                return ResultadoIdentity.Fallo("Usuario no encontrado.");
            }

            if (nombre is not null) usuario.Nombre = nombre;
            if (apellidos is not null) usuario.Apellidos = apellidos;

            var resultado = await userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
            {
                return ResultadoIdentity.Fallo(Traducir(resultado));
            }

            if (telefono is not null)
            {
                var resultadoTelefono = await userManager.SetPhoneNumberAsync(usuario, telefono);
                if (!resultadoTelefono.Succeeded)
                {
                    return ResultadoIdentity.Fallo(Traducir(resultadoTelefono));
                }
            }

            return ResultadoIdentity.Ok();
        }

        public async Task<string?> GenerarTokenRestablecimientoAsync(string email)
        {
            var usuario = await userManager.FindByEmailAsync(email);

            return usuario is null ? null : await userManager.GeneratePasswordResetTokenAsync(usuario);
        }

        public async Task<ResultadoIdentity> RestablecerPasswordAsync(string email, string token, string passwordNueva)
        {
            var usuario = await userManager.FindByEmailAsync(email);

            if (usuario is null)
            {
                return ResultadoIdentity.Fallo("El correo no est&#225; registrado.");
            }

            var resultado = await userManager.ResetPasswordAsync(usuario, token, passwordNueva);

            return resultado.Succeeded
                ? ResultadoIdentity.Ok()
                : ResultadoIdentity.Fallo(Traducir(resultado));
        }

        private async Task<Usuario> MaterializarAsync(ApplicationUser usuario)
        {
            var roles = await userManager.GetRolesAsync(usuario);

            return new Usuario
            {
                UsuarioId = usuario.Id,
                Email = usuario.Email ?? string.Empty,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                Telefono = usuario.PhoneNumber,
                Roles = roles.ToList()
            };
        }

        private static string[] Traducir(IdentityResult resultado)
        {
            return resultado.Errors.Select(e => e.Description).ToArray();
        }
    }
}
