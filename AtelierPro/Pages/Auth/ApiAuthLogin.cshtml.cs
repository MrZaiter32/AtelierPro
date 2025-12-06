using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using AtelierPro.Data;

namespace AtelierPro.Pages.Auth;

public class ApiAuthLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ApiAuthLoginModel> _logger;

    public ApiAuthLoginModel(SignInManager<ApplicationUser> signInManager, ILogger<ApiAuthLoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? email, string? password, bool rememberMe = false, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorMessage = "Email y contraseña son requeridos.";
            ReturnUrl = returnUrl ?? "/";
            return;
        }

        _logger.LogInformation("Intento de login para: {email}", email);

        try
        {
            var result = await _signInManager.PasswordSignInAsync(
                email,
                password,
                rememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Login exitoso para: {email}", email);
                ReturnUrl = returnUrl ?? "/";
                RedirectToPage(ReturnUrl);
                return;
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Cuenta bloqueada: {email}", email);
                ErrorMessage = "La cuenta ha sido bloqueada por demasiados intentos fallidos.";
            }
            else if (result.RequiresTwoFactor)
            {
                ErrorMessage = "Se requiere autenticación de dos factores.";
            }
            else
            {
                _logger.LogWarning("Login fallido para: {email}", email);
                ErrorMessage = "Correo o contraseña inválidos.";
            }

            ReturnUrl = returnUrl ?? "/";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante login");
            ErrorMessage = "Error interno del servidor.";
            ReturnUrl = returnUrl ?? "/";
        }
    }
}
