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

    return Results.Content($@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Activar cuenta - Gymva</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Segoe UI', sans-serif;
            background: url('https://images.unsplash.com/photo-1571019613578-2b58f16451be?auto=format&fit=crop&w=1280&q=80') no-repeat center center fixed;
            background-size: cover;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            color: white;
        }}
        .container {{
            background-color: rgba(0, 0, 0, 0.75);
            padding: 40px;
            border-radius: 15px;
            width: 90%;
            max-width: 400px;
            text-align: center;
        }}
        img {{
            width: 80px;
            margin-bottom: 20px;
        }}
        h2 {{
            font-size: 26px;
            margin-bottom: 10px;
        }}
        input {{
            width: 100%;
            padding: 12px;
            margin-top: 20px;
            margin-bottom: 20px;
            border-radius: 8px;
            border: none;
            font-size: 16px;
        }}
        button {{
            width: 100%;
            padding: 12px;
            font-size: 16px;
            font-weight: bold;
            color: white;
            background-color: #d6452c;
            border: none;
            border-radius: 8px;
            cursor: pointer;
        }}
        button:hover {{
            background-color: #b5381f;
        }}
        p {{
            margin-top: 20px;
            font-size: 14px;
            color: #ccc;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <img src='https://raw.githubusercontent.com/FJAO87/GymvaActivacionWeb/main/logo.png' alt='Gymva Logo'>
        <h2>Activar cuenta de gimnasio</h2>
        <form method='post' action='/activar'>
            <input type='hidden' name='token' value='{token}' />
            <input type='password' name='password' placeholder='Nueva contraseña' required/>
            <button type='submit'>Activar cuenta</button>
        </form>
        <p>Este formulario está vinculado a tu activación Gymva.</p>
    </div>
</body>
</html>
", "text/html");
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
            return Results.BadRequest("Token expirado. Solicita uno nuevo.");

        usuario.contrasena_hash = BCrypt.Net.BCrypt.HashPassword(password);
        usuario.activo = true;
        usuario.token_activacion = null;
        usuario.fecha_expiracion = null;

        var gimnasio = await db.Gimnasios.FirstOrDefaultAsync(g => g.id_usuario == usuario.id_usuario);
        if (gimnasio != null)
            gimnasio.activo = true;

        await db.SaveChangesAsync();

        return Results.Content("<h2>✅ Cuenta activada correctamente. Ya puedes iniciar sesión en el programa Gymva.</h2>", "text/html");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error durante la activación: " + ex.Message);
        return Results.Problem("Error interno al activar la cuenta.");
    }
});

app.Run();
