using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecetasAPI.Data;
using RecetasAPI.Models;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Add Security
builder.Services.AddAuthorization(options =>
{
    // Política basada en roles: solo el Administrador puede crear, modificar o eliminar
    options.AddPolicy(Politicas.SoloAdministrador, policy =>
        policy.RequireRole(Roles.Administrador));
});

builder.Services.AddAuthentication()
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        // Al ser una API devolvemos 401/403 en lugar de redirigir a una página de login
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddIdentityCore<Usuario>()
    .AddRoles<IdentityRole>()   // Habilita roles (RoleManager y tabla AspNetRoles)
    .AddEntityFrameworkStores<RecetasDbContext>()
    .AddApiEndpoints();

// EF Core - SQL Server
builder.Services.AddDbContext<RecetasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Crea los roles "Administrador" y "Usuario" y los usuarios de prueba
// (ejecutar antes Update-Database para que existan las tablas)
await DbInitializer.SembrarRolesYUsuariosAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Scalar UI (reemplaza a Swagger UI) disponible en /scalar/v1
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Página web (wwwroot/index.html) con el visor de reportes de SSRS
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGroup("/api/auth")
    .WithTags("Auth")
    .MapIdentityApi<Usuario>();

app.Run();
