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
      background: url('https://raw.githubusercontent.com/FJAO87/GymvaActivacionWeb/main/fondo_gym.jpg') no-repeat center center fixed;
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
      margin-bottom: 10px;
    }}
    .brand {{
      font-size: 22px;
      font-weight: bold;
      letter-spacing: 2px;
      color: white;
      margin-bottom: 20px;
    }}
    h2 {{
      font-size: 24px;
      margin-bottom: 10px;
    }}
    input {{
      width: 100%;
      padding: 12px;
      margin-top: 15px;
      margin-bottom: 15px;
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
      background-color: #00CFFF;
      border: none;
      border-radius: 8px;
      cursor: pointer;
    }}
    button:hover {{
      background-color: #00b5e5;
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
    <div class='brand'>GYMVA</div>
    <h2>Activar cuenta de gimnasio</h2>
    <form method='post' action='/activar' onsubmit='return validarContrasenas()'>
      <input type='hidden' name='token' value='{token}' />
      <input type='password' id='password' name='password' placeholder='Nueva contraseña' required/>
      <input type='password' id='confirmPassword' placeholder='Confirmar contraseña' required/>
      <button type='submit'>Activar cuenta</button>
    </form>
    <p>Este formulario está vinculado a tu activación Gymva.</p>
  </div>
  <script>
    function validarContrasenas() {{
      const pass = document.getElementById('password').value;
      const confirm = document.getElementById('confirmPassword').value;
      if (pass !== confirm) {{
        alert('Las contraseñas no coinciden.');
        return false;
      }}
      return true;
    }}
  </script>
</body>
</html>
", "text/html");
});

// El resto del POST '/activar' y demás lógica se mantendrá intacta hasta que lo editemos a continuación si es necesario.

app.Run();
