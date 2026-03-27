using System.ComponentModel.DataAnnotations;

namespace FieldTechWeb.Models
{
    public class CambiarContrasennaViewModel
    {
        [Required]
        [MinLength(8, ErrorMessage = "Mínimo 8 caracteres.")]
        public string NuevaContrasenna { get; set; } = string.Empty;

        [Required]
        [Compare("NuevaContrasenna", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasenna { get; set; } = string.Empty;
    }

    // Perfil compartido entre TECH y CLIENT (datos base de Users)
    public class PerfilUsuarioViewModel
    {
        public int    UserId    { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string Email     { get; set; } = string.Empty;
        public string Phone     { get; set; } = string.Empty;
        public string UserType  { get; set; } = string.Empty;

        // Campos extra para TECH
        public string?  Bio                { get; set; }
        public decimal? HourlyRate         { get; set; }
        public string?  Zone               { get; set; }
        public string?  AvailabilityStatus { get; set; }
        public string?  PortfolioUrl       { get; set; }

        // Campos extra para CLIENT
        public string? DisplayName  { get; set; }
        public string? ContactName  { get; set; }
        public string? ContactPhone { get; set; }
        public string? LocationText { get; set; }
    }
}
