using System.ComponentModel.DataAnnotations;

namespace FieldTechApi.Models
{
    public class IniciarSesionRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Contrasenna { get; set; } = string.Empty;
    }
}
