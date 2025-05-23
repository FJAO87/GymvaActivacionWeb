using Microsoft.EntityFrameworkCore;
using GymvaActivacionWeb.Data;
using GymvaActivacionWeb.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GymvaDbContext>(options =>
    options.UseMySql(
        "Server=sql7.freesqldatabase.com;Port=3306;Database=sql7780578;Uid=sql7780578;Pwd=keRF8Iadnc;Connection Timeout=30;",
        new MySqlServerVersion(new Version(8, 0, 36)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure()
    )
);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/activar", async (HttpContext http) =>
{
    string token = http.Request.Query["token"];

    if (string.IsNullOrWhiteSpace(token))
        return Results.BadRequest("Token inválido");

    return Results.Content($@"...", "text/html"); // Aquí va tu HTML de activación
});

app.MapPost("/activar", async (HttpContext http, GymvaDbContext db) =>
{
    var form = await http.Request.ReadFormAsync();
    string token = form["token"];
    string password = form["password"];

    if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(password))
        return Results.BadRequest("Datos incompletos");

    try
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.token_activacion == token && u.activo == false);
        if (usuario == null)
            return Results.BadRequest("Token inválido o ya utilizado.");

        if (usuario.fecha_expiracion < DateTime.UtcNow)
            return Results.Content("...", "text/html"); // HTML token expirado

        usuario.contrasena_hash = BCrypt.Net.BCrypt.HashPassword(password);
        usuario.activo = true;
        usuario.token_activacion = null;
        usuario.fecha_expiracion = null;

        var gimnasio = await db.Gimnasios.FirstOrDefaultAsync(g => g.id_usuario == usuario.id_usuario);
        if (gimnasio != null)
            gimnasio.activo = true;

        await db.SaveChangesAsync();

        return Results.Content("...", "text/html"); // HTML éxito
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error durante la activación: " + ex.Message);
        return Results.Problem("Error interno al activar la cuenta.");
    }
});

app.Run();

