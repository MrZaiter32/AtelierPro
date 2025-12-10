using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AtelierPro.Data;
using AtelierPro.Services;
using AtelierPro.Services.Catalogos;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers(); // Agregar soporte para API Controllers
builder.Services.AddMudServices();
builder.Services.AddSingleton<BusyService>();

// Configurar DbContext con SQLite
builder.Services.AddDbContext<AtelierProDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=atelierpro.db"));

// Configurar ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AtelierProDbContext>()
    .AddDefaultTokenProviders();

// Configurar opciones de Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

// Repositorios (Scoped - una instancia por request)
builder.Services.AddScoped<PresupuestoRepository>();
builder.Services.AddScoped<ClienteRepository>();

// Servicios de dominio (cambiar a Scoped para trabajar con EF Core)
builder.Services.AddScoped<ReglaService>();
builder.Services.AddScoped<PresupuestoService>();
builder.Services.AddScoped<WorkflowService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<TallerService>(); // FASE 1
builder.Services.AddScoped<AlmacenService>(); // FASE 1
builder.Services.AddScoped<ComprasService>(); // FASE 1
builder.Services.AddScoped<AuthService>(); // Autenticación y gestión de usuarios

// Servicios de catálogos
builder.Services.AddHttpClient("FinditParts", client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:5000");
    client.Timeout = TimeSpan.FromSeconds(60);
}).ConfigureHttpClient(client => client.DefaultRequestHeaders.Add("User-Agent", "AtelierPro/1.0"));

// HttpClient tipado para la búsqueda detallada (página Almacén)
builder.Services.AddHttpClient<IFinditPartsService, FinditPartsService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddScoped<CatalogosManager>();

// CORS para API Python
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPythonAPI", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://127.0.0.1:5000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Mantener ErpDataService como Singleton solo para demo/seed inicial
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<ErpDataService>();

var app = builder.Build();

// Asegurar que la base de datos esté creada y con seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AtelierProDbContext>();
    context.Database.EnsureCreated();
    await DbSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseWebSockets();
app.UseCors("AllowPythonAPI");
app.UseStaticFiles();

app.UseRouting();

// IMPORTANTE: Authentication y Authorization DEBEN estar antes de endpoints
app.UseAuthentication();
app.UseAuthorization();

// Mapear API controllers primero
app.MapControllers();
// Luego Blazor
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();