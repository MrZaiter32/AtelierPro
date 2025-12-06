using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using AtelierPro.Data;

namespace AtelierPro.Pages.Auth;

public class ApiAuthLogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ApiAuthLogoutModel> _logger;

    public ApiAuthLogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<ApiAuthLogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Usuario desconectado");
        RedirectToPage("/Auth/Login");
    }
}
