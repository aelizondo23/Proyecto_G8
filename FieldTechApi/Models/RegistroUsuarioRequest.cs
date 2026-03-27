using System.ComponentModel.DataAnnotations;

namespace FieldTechApi.Models
{
    public class RegistroUsuarioRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Contrasenna { get; set; } = string.Empty;
        [Required]
        public int TipoUsuario { get; set; }  // 0 = TECH, 1 = CLIENT
        [Required]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        public string Apellido { get; set; } = string.Empty;
    }
}
