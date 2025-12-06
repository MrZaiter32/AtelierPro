using Microsoft.AspNetCore.Identity;

namespace AtelierPro.Data;

/// <summary>
/// Usuario de aplicación extendido con datos personalizados para AtelierPro.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string? NombreCompleto { get; set; }

    /// <summary>
    /// Teléfono del usuario.
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Dirección del usuario.
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Indica si el usuario está activo.
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Fecha de creación del usuario.
    /// </summary>
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Última fecha de modificación.
    /// </summary>
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
