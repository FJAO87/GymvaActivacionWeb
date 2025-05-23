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
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Activate Gymva</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Segoe UI', sans-serif;
            background: url('https://images.unsplash.com/photo-1571019613578-2b58f16451be?auto=format&fit=crop&w=1280&q=80') no-repeat center center fixed;
            background-size: cover;
            color: white;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
        }}
        .container {{
            background-color: rgba(0, 0, 0, 0.7);
            padding: 40px;
            border-radius: 15px;
            width: 90%;
            max-width: 400px;
            text-align: center;
        }}
        img {{
            width: 70px;
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
        <img src='https://raw.githubusercontent.com/FJAO87/GymvaActivacionWeb/main/logo.png' alt='Gymva Logo' />
        <h2>Activate Your Account</h2>
        <p>Set your new password below</p>
        <form method='post' action='/activar'>
            <input type='hidden' name='token' value='{token}' />
            <input type='password' name='password' placeholder='New Password' required/>
            <button type='submit'>Activate Account</button>
        </form>
        <p>Didn’t receive a code? Check spam or register again</p>
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

        return Results.Content(@"
<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Cuenta activada</title>
  <style>
    body {
      margin: 0;
      padding: 0;
      font-family: 'Segoe UI', sans-serif;
      background: url('https://raw.githubusercontent.com/FJAO87/GymvaActivacionWeb/main/fondo_gym.jpg') no-repeat center center fixed;
      background-size: cover;
      display: flex;
      justify-content: center;
      align-items: center;
      height: 100vh;
      color: white;
    }
    .container {
      background-color: rgba(0, 0, 0, 0.75);
      padding: 40px;
      border-radius: 15px;
      width: 90%;
      max-width: 500px;
      text-align: center;
    }
    img {
      width: 80px;
      margin-bottom: 10px;
    }
    .brand {
      font-size: 22px;
      font-weight: bold;
      letter-spacing: 2px;
      color: #00CFFF;
      margin-bottom: 20px;
    }
    .success-icon {
      font-size: 60px;
      color: #00CFFF;
      margin-bottom: 20px;
    }
    h2 {
      font-size: 26px;
      margin-bottom: 10px;
    }
    p {
      font-size: 16px;
      color: #ddd;
      margin-bottom: 30px;
    }
    a.button {
      background-color: #00CFFF;
      color: white;
      text-decoration: none;
      padding: 12px 24px;
      font-weight: bold;
      border-radius: 8px;
      font-size: 16px;
    }
    a.button:hover {
      background-color: #00b5e5;
    }
  </style>
</head>
<body>
  <div class='container'>
    <img src='https://raw.githubusercontent.com/FJAO87/GymvaActivacionWeb/main/logo.png' alt='Gymva Logo'>
    <div class='brand'>GYMVA</div>
    <div class='success-icon'>✅</div>
    <h2>Cuenta activada correctamente</h2>
    <p>Ya puedes iniciar sesión en el programa Gymva con tu nueva contraseña.</p>
    <a href='#' class='button'>Abrir Gymva</a>
  </div>
</body>
</html>
", "text/html");

    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error durante la activación: " + ex.Message);
        return Results.Problem("Error interno al activar la cuenta.");
    }
});

app.Run();
