using AtelierPro.Data;
using AtelierPro.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AtelierPro.Services;

/// <summary>
/// Servicio para inicializar la base de datos con datos de ejemplo.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AtelierProDbContext context)
    {
        // Crear roles si no existen
        await SeedRolesAsync(context);

        // Crear usuarios por defecto si no existen
        await SeedUsersAsync(context);

        // Si ya hay datos de ERP, no hacer nada
        if (await context.Presupuestos.AnyAsync())
        {
            return;
        }

        // Crear tarifa base
        var tarifaBase = new Tarifa
        {
            PrecioManoObraHora = 45m,
            PrecioPinturaHora = 38m,
            TasaIva = 0.16m,
            FactorRecargo = 1.05m,
            FactorDescuento = 1m
        };
        context.Tarifas.Add(tarifaBase);

        // Crear vehículo
        var vehiculo = new Vehiculo
        {
            Vin = "3VWFE21C04M000001",
            Version = "Golf Highline",
            AntiguedadAnios = 6,
            ValorResidual = 8500m
        };
        context.Vehiculos.Add(vehiculo);

        // Crear items de presupuesto
        var item1 = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.Pieza,
            Codigo = "PUERT-DEL",
            Descripcion = "Puerta delantera",
            TiempoAsignadoHoras = 2.5,
            PrecioUnitario = 120m,
            PorcentajeAjuste = 0m,
            RequierePintura = true,
            RequiereDesmontajeDoble = true,
            RequiereAlineacion = false
        };

        var item2 = new ItemPresupuesto
        {
            Tipo = TipoItemPresupuesto.ManoObra,
            Codigo = "MO-01",
            Descripcion = "Mano de obra general",
            TiempoAsignadoHoras = 3,
            PrecioUnitario = 45m,
            PorcentajeAjuste = 0m
        };

        // Crear presupuesto
        var presupuesto = new Presupuesto
        {
            Vehiculo = vehiculo,
            Items = new List<ItemPresupuesto> { item1, item2 },
            // Subtotal es calculado automáticamente
            IvaAplicado = 69.6m, // Se calculará correctamente con PresupuestoService
            TotalFinal = 504.6m, // Se calculará correctamente con PresupuestoService
            Estado = EstadoPresupuesto.Aprobado
        };
        context.Presupuestos.Add(presupuesto);

        // Crear clientes
        var cliente = new Cliente
        {
            Nombre = "Ana Pérez",
            Historial = "Cliente VIP con 5 servicios.",
            Preferencias = "Comunicación por WhatsApp.",
            Nps = 75,
            TasaRetencion = 0.92,
            Interacciones = new List<Interaccion>
            {
                new()
                {
                    Fecha = DateTime.UtcNow.AddDays(-2),
                    Tipo = "Seguimiento",
                    Resultado = "Acepta cotización"
                }
            }
        };
        context.Clientes.Add(cliente);

        // Crear refacciones
        context.Refacciones.AddRange(new[]
        {
            new Refaccion { Sku = "PUERT-DEL", StockActual = 0, StockMinimo = 1, CostoPromedio = 95m },
            new Refaccion { Sku = "PARAG-GTI", StockActual = 3, StockMinimo = 2, CostoPromedio = 70m }
        });

        // Crear orden de compra
        var ordenCompra = new OrdenCompra
        {
            Proveedor = "Autopartes Express",
            Items = new List<ItemPresupuesto> { item1 },
            Estado = "Enviado"
        };
        context.OrdenesCompra.Add(ordenCompra);

        // Crear orden de reparación
        var ordenReparacion = new OrdenReparacion
        {
            TecnicoAsignado = "Luis Gómez",
            Inicio = DateTime.UtcNow.AddHours(-5),
            HorasReales = 4.2
        };
        context.OrdenesReparacion.Add(ordenReparacion);

        // Crear activo
        var activo = new Activo
        {
            Nombre = "Cabina de pintura PPG",
            Tipo = "Cabina",
            UltimaCalibracion = DateTime.UtcNow.AddDays(-200),
            FrecuenciaCalibracion = TimeSpan.FromDays(180)
        };
        context.Activos.Add(activo);

        // Crear plan de mantenimiento
        var plan = new PlanMantenimiento
        {
            ActivoNombre = "Elevador 2 columnas",
            Frecuencia = "Trimestral",
            ProximaFecha = DateTime.UtcNow.AddDays(20)
        };
        context.PlanesMantenimiento.Add(plan);

        // Crear transacción
        var transaccion = new Transaccion
        {
            Tipo = "Ingreso",
            Concepto = "Cobro siniestro",
            Fecha = DateTime.UtcNow.AddDays(-1),
            Monto = 5200m
        };
        context.Transacciones.Add(transaccion);

        // Crear factura
        var factura = new FacturaCliente
        {
            PresupuestoId = presupuesto.Id,
            Importe = presupuesto.TotalFinal,
            FechaEmision = DateTime.UtcNow,
            Pagada = false
        };
        context.FacturasClientes.Add(factura);

        // Guardar todos los cambios
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(AtelierProDbContext context)
    {
        var roles = new[]
        {
            new ApplicationRole { Name = "Admin", NormalizedName = "ADMIN", Descripcion = "Administrador del sistema" },
            new ApplicationRole { Name = "Finanzas", NormalizedName = "FINANZAS", Descripcion = "Encargado de finanzas" },
            new ApplicationRole { Name = "Taller", NormalizedName = "TALLER", Descripcion = "Técnico de taller" },
            new ApplicationRole { Name = "Cliente", NormalizedName = "CLIENTE", Descripcion = "Cliente del sistema" }
        };

        foreach (var role in roles)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == role.Name))
            {
                context.Roles.Add(role);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AtelierProDbContext context)
    {
        var passwordHasher = new PasswordHasher<ApplicationUser>();

        var admin = new ApplicationUser
        {
            UserName = "admin@atelierpro.com",
            Email = "admin@atelierpro.com",
            EmailConfirmed = true,
            NombreCompleto = "Administrador Sistema",
            Activo = true,
            NormalizedUserName = "ADMIN@ATELIERPRO.COM",
            NormalizedEmail = "ADMIN@ATELIERPRO.COM"
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123456");

        var finanzas = new ApplicationUser
        {
            UserName = "finanzas@atelierpro.com",
            Email = "finanzas@atelierpro.com",
            EmailConfirmed = true,
            NombreCompleto = "Juan Pérez Finanzas",
            Activo = true,
            NormalizedUserName = "FINANZAS@ATELIERPRO.COM",
            NormalizedEmail = "FINANZAS@ATELIERPRO.COM"
        };
        finanzas.PasswordHash = passwordHasher.HashPassword(finanzas, "Finanzas123456");

        var taller = new ApplicationUser
        {
            UserName = "taller@atelierpro.com",
            Email = "taller@atelierpro.com",
            EmailConfirmed = true,
            NombreCompleto = "Carlos García Taller",
            Activo = true,
            NormalizedUserName = "TALLER@ATELIERPRO.COM",
            NormalizedEmail = "TALLER@ATELIERPRO.COM"
        };
        taller.PasswordHash = passwordHasher.HashPassword(taller, "Taller123456");

        var cliente = new ApplicationUser
        {
            UserName = "cliente@example.com",
            Email = "cliente@example.com",
            EmailConfirmed = true,
            NombreCompleto = "María López Cliente",
            Activo = true,
            NormalizedUserName = "CLIENTE@EXAMPLE.COM",
            NormalizedEmail = "CLIENTE@EXAMPLE.COM"
        };
        cliente.PasswordHash = passwordHasher.HashPassword(cliente, "Cliente123456");

        // Agregar usuarios si no existen
        if (!await context.Users.AnyAsync(u => u.Email == "admin@atelierpro.com"))
        {
            context.Users.Add(admin);
            await context.SaveChangesAsync();

            // Agregar usuario a roles
            await context.UserRoles.AddAsync(new IdentityUserRole<string>
            {
                UserId = admin.Id,
                RoleId = context.Roles.First(r => r.Name == "Admin").Id
            });
        }

        if (!await context.Users.AnyAsync(u => u.Email == "finanzas@atelierpro.com"))
        {
            context.Users.Add(finanzas);
            await context.SaveChangesAsync();

            await context.UserRoles.AddAsync(new IdentityUserRole<string>
            {
                UserId = finanzas.Id,
                RoleId = context.Roles.First(r => r.Name == "Finanzas").Id
            });
        }

        if (!await context.Users.AnyAsync(u => u.Email == "taller@atelierpro.com"))
        {
            context.Users.Add(taller);
            await context.SaveChangesAsync();

            await context.UserRoles.AddAsync(new IdentityUserRole<string>
            {
                UserId = taller.Id,
                RoleId = context.Roles.First(r => r.Name == "Taller").Id
            });
        }

        if (!await context.Users.AnyAsync(u => u.Email == "cliente@example.com"))
        {
            context.Users.Add(cliente);
            await context.SaveChangesAsync();

            await context.UserRoles.AddAsync(new IdentityUserRole<string>
            {
                UserId = cliente.Id,
                RoleId = context.Roles.First(r => r.Name == "Cliente").Id
            });
        }

        await context.SaveChangesAsync();
    }
}
