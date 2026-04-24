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

    public class PerfilUsuarioViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;

        // Campos TECH
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Zone { get; set; }
        public string? AvailabilityStatus { get; set; }
        public string? PortfolioUrl { get; set; }

        // Campos CLIENT
        public string? DisplayName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? LocationText { get; set; }
        public string? ClientType { get; set; }

        // Listas TECH
        public List<EducacionViewModel> Educacion { get; set; } = new();
        public List<ExperienciaViewModel> Experiencia { get; set; } = new();
    }

    public class EducacionViewModel
    {
        public int EducationId { get; set; }
        public string Institution { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string? FieldOfStudy { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
    }

    public class ExperienciaViewModel
    {
        public int ExperienceId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public string? Description { get; set; }
    }
}