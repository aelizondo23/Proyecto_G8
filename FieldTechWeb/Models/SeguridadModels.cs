using System.ComponentModel.DataAnnotations;

namespace FieldTechWeb.Models
{
    public class CambiarContrasennaViewModel
    {
        [Required]
        [MinLength(8)]
        public string NuevaContrasenna { get; set; } = string.Empty;
        [Required]
        [Compare("NuevaContrasenna")]
        public string ConfirmarContrasenna { get; set; } = string.Empty;
    }
}
