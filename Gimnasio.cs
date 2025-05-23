using System.ComponentModel.DataAnnotations;

namespace GymvaActivacionWeb.Models
{
    public class Gimnasio
    {
        [Key]
        public int id_gimnasio { get; set; }
        public string? nombre { get; set; }
        public bool activo { get; set; }
        public int id_usuario { get; set; }
    }
}
