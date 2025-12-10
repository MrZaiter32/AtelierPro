using AtelierPro.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AtelierPro.Services
{
    /// <summary>
    /// Servicio de dominio para autenticación y gestión de usuarios.
    /// Implementa lógica de registro, login, cambio de contraseña y gestión de roles.
    /// Sigue Clean Architecture: separación entre autenticación y lógica de negocio.
    /// </summary>
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AuthService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registra un nuevo usuario con validaciones exhaustivas.
        /// </summary>
        public async Task<(bool Exitoso, string? Mensaje)> RegistrarUsuarioAsync(
            string email,
            string contraseña,
            string nombreCompleto,
            string? telefono = null,
            string? rol = null)
        {
            try
            {
                // 1. Validar entrada
                if (string.IsNullOrWhiteSpace(email))
                    return (false, "El email es requerido.");

                if (string.IsNullOrWhiteSpace(contraseña))
                    return (false, "La contraseña es requerida.");

                if (contraseña.Length < 8)
                    return (false, "La contraseña debe tener al menos 8 caracteres.");

                if (string.IsNullOrWhiteSpace(nombreCompleto))
                    return (false, "El nombre completo es requerido.");

                // 2. Verificar si usuario ya existe
                var usuarioExistente = await _userManager.FindByEmailAsync(email);
                if (usuarioExistente != null)
                    return (false, "El email ya está registrado.");

                // 3. Crear usuario
                var nuevoUsuario = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = false,
                    NombreCompleto = nombreCompleto,
                    Telefono = telefono,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                var resultado = await _userManager.CreateAsync(nuevoUsuario, contraseña);

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    _logger.LogWarning($"Error al crear usuario {email}: {errores}");
                    return (false, $"Error al registrar usuario: {errores}");
                }

                // 4. Asignar rol por defecto
                var rolAsignar = rol ?? "Usuario"; // Rol por defecto
                if (await _roleManager.RoleExistsAsync(rolAsignar))
                {
                    await _userManager.AddToRoleAsync(nuevoUsuario, rolAsignar);
                }

                _logger.LogInformation($"Usuario {email} registrado exitosamente con rol {rolAsignar}.");
                return (true, "Usuario registrado exitosamente. Por favor, inicia sesión.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al registrar usuario: {ex.Message}");
                return (false, "Error inesperado. Por favor, intenta más tarde.");
            }
        }

        /// <summary>
        /// Autentica un usuario con email y contraseña.
        /// </summary>
        public async Task<(bool Exitoso, string? Mensaje)> LoginAsync(
            string email,
            string contraseña,
            bool recordarme = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contraseña))
                    return (false, "Email y contraseña son requeridos.");

                var usuario = await _userManager.FindByEmailAsync(email);

                if (usuario == null)
                {
                    _logger.LogWarning($"Intento de login fallido: usuario {email} no encontrado.");
                    return (false, "Email o contraseña incorrectos.");
                }

                if (!usuario.Activo)
                {
                    _logger.LogWarning($"Intento de login de usuario inactivo: {email}");
                    return (false, "Tu cuenta ha sido desactivada. Contacta al administrador.");
                }

                var resultado = await _signInManager.PasswordSignInAsync(usuario, contraseña, recordarme, lockoutOnFailure: true);

                if (resultado.Succeeded)
                {
                    _logger.LogInformation($"Usuario {email} inició sesión exitosamente.");
                    return (true, null);
                }

                if (resultado.IsLockedOut)
                {
                    _logger.LogWarning($"Usuario {email} bloqueado por múltiples intentos fallidos.");
                    return (false, "Tu cuenta está temporalmente bloqueada. Intenta más tarde.");
                }

                if (resultado.RequiresTwoFactor)
                {
                    return (false, "Se requiere autenticación de dos factores.");
                }

                _logger.LogWarning($"Login fallido para {email}: contraseña incorrecta.");
                return (false, "Email o contraseña incorrectos.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado en login: {ex.Message}");
                return (false, "Error inesperado. Por favor, intenta más tarde.");
            }
        }

        /// <summary>
        /// Cierra sesión del usuario actual.
        /// </summary>
        public async Task<(bool Exitoso, string? Mensaje)> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("Usuario cerró sesión exitosamente.");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cerrar sesión: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario actual.
        /// </summary>
        public async Task<(bool Exitoso, string? Mensaje)> CambiarContraseñaAsync(
            ApplicationUser usuario,
            string contraseñaActual,
            string contraseñaNueva)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contraseñaActual))
                    return (false, "La contraseña actual es requerida.");

                if (string.IsNullOrWhiteSpace(contraseñaNueva))
                    return (false, "La contraseña nueva es requerida.");

                if (contraseñaNueva.Length < 8)
                    return (false, "La contraseña debe tener al menos 8 caracteres.");

                if (contraseñaActual == contraseñaNueva)
                    return (false, "La contraseña nueva debe ser diferente a la actual.");

                var resultado = await _userManager.ChangePasswordAsync(usuario, contraseñaActual, contraseñaNueva);

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    _logger.LogWarning($"Error al cambiar contraseña de {usuario.Email}: {errores}");
                    return (false, $"Error: {errores}");
                }

                _logger.LogInformation($"Usuario {usuario.Email} cambió su contraseña.");
                return (true, "Contraseña actualizada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cambiar contraseña: {ex.Message}");
                return (false, "Error inesperado. Por favor, intenta más tarde.");
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios activos (para administración).
        /// </summary>
        public async Task<List<ApplicationUser>> ObtenerUsuariosActivosAsync()
        {
            try
            {
                var usuarios = _userManager.Users.Where(u => u.Activo).ToList();
                return await Task.FromResult(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuarios: {ex.Message}");
                return new List<ApplicationUser>();
            }
        }

        /// <summary>
        /// Obtiene un usuario por email.
        /// </summary>
        public async Task<ApplicationUser?> ObtenerUsuarioAsync(string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuario {email}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene los roles de un usuario.
        /// </summary>
        public async Task<List<string>> ObtenerRolesUsuarioAsync(ApplicationUser usuario)
        {
            try
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                return new List<string>(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener roles del usuario: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Asigna un rol a un usuario (solo Admin).
        /// </summary>
        public async Task<(bool Exitoso, string? Mensaje)> AsignarRolAsync(
            ApplicationUser usuario,
            string rol)
        {
            try
            {
                if (!await _roleManager.RoleExistsAsync(rol))
                    return (false, $"El rol '{rol}' no existe.");

                var resultado = await _userManager.AddToRoleAsync(usuario, rol);

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    return (false, errores);
                }

                _logger.LogInformation($"Rol '{rol}' asignado a usuario {usuario.Email}.");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al asignar rol: {ex.Message}");
                return (false, "Error inesperado.");
            }
        }

        /// <summary>
        /// Remueve un rol de un usuario (solo Admin).
        /// </summary>
        public async Task<(bool Exitoso, string? Mensaje)> RemoverRolAsync(
            ApplicationUser usuario,
            string rol)
        {
            try
            {
                var resultado = await _userManager.RemoveFromRoleAsync(usuario, rol);

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    return (false, errores);
                }

                _logger.LogInformation($"Rol '{rol}' removido del usuario {usuario.Email}.");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al remover rol: {ex.Message}");
                return (false, "Error inesperado.");
            }
        }

        /// <summary>
        /// Desactiva una cuenta de usuario (solo Admin).
        /// </summary>
        public async Task<(bool Exitoso, string? Mensaje)> DesactivarUsuarioAsync(ApplicationUser usuario)
        {
            try
            {
                usuario.Activo = false;
                usuario.FechaActualizacion = DateTime.UtcNow;
                var resultado = await _userManager.UpdateAsync(usuario);

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    return (false, errores);
                }

                _logger.LogInformation($"Usuario {usuario.Email} desactivado.");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al desactivar usuario: {ex.Message}");
                return (false, "Error inesperado.");
            }
        }

        /// <summary>
        /// Reactiva una cuenta de usuario (solo Admin).
        /// </summary>
        public async Task<(bool Exitoso, string? Mensaje)> ReactivarUsuarioAsync(ApplicationUser usuario)
        {
            try
            {
                usuario.Activo = true;
                usuario.FechaActualizacion = DateTime.UtcNow;
                var resultado = await _userManager.UpdateAsync(usuario);

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    return (false, errores);
                }

                _logger.LogInformation($"Usuario {usuario.Email} reactivado.");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al reactivar usuario: {ex.Message}");
                return (false, "Error inesperado.");
            }
        }
    }
}
