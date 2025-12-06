using Microsoft.AspNetCore.Identity;

namespace AtelierPro.Data;

/// <summary>
/// Rol de la aplicación para control de acceso basado en roles.
/// </summary>
public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// Descripción del rol.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Fecha de creación del rol.
    /// </summary>
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
