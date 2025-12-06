using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AtelierPro.Data;
using System.ComponentModel.DataAnnotations;

namespace AtelierPro.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(SignInManager<ApplicationUser> signInManager, ILogger<AuthController> logger)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Endpoint para login (maneja cookies HTTP, no UI Blazor)
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new LoginResponse 
            { 
                Success = false, 
                Message = "Datos de entrada inválidos" 
            });
        }

        try
        {
            _logger.LogInformation("Intento de login para: {email}", request.Email);

            var result = await _signInManager.PasswordSignInAsync(
                request.Email,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Login exitoso para: {email}", request.Email);
                return Ok(new LoginResponse 
                { 
                    Success = true, 
                    Message = "Login exitoso" 
                });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Cuenta bloqueada: {email}", request.Email);
                return Unauthorized(new LoginResponse 
                { 
                    Success = false, 
                    Message = "La cuenta ha sido bloqueada por demasiados intentos fallidos. Intenta más tarde." 
                });
            }

            if (result.RequiresTwoFactor)
            {
                return BadRequest(new LoginResponse 
                { 
                    Success = false, 
                    Message = "Se requiere autenticación de dos factores" 
                });
            }

            _logger.LogWarning("Login fallido para: {email}", request.Email);
            return Unauthorized(new LoginResponse 
            { 
                Success = false, 
                Message = "Correo o contraseña inválidos" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante login");
            return StatusCode(500, new LoginResponse 
            { 
                Success = false, 
                Message = "Error interno del servidor" 
            });
        }
    }

    /// <summary>
    /// Endpoint para logout
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Logout exitoso");
            return Ok(new LoginResponse 
            { 
                Success = true, 
                Message = "Logout exitoso" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante logout");
            return StatusCode(500, new LoginResponse 
            { 
                Success = false, 
                Message = "Error durante logout" 
            });
        }
    }
}
