using System.ComponentModel.DataAnnotations;

namespace GymvaActivacionWeb/Models/Usuario.cs
{
    public class Usuario
    {
        [Key]
        public int id_usuario { get; set; }
        public string? nombre_usuario { get; set; }
        public string? contrasena_hash { get; set; }
        public bool activo { get; set; }
        public string? token_activacion { get; set; }
        public DateTime? fecha_expiracion { get; set; }
    }
}
