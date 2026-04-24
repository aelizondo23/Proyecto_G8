namespace FieldTechApi.Models
{
    public class UsuarioResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Nombre => FirstName;
        public string Apellido => LastName;
        public string UserType { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class ActualizarPerfilRequest
    {
        // TECH
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Zone { get; set; }
        public string? AvailabilityStatus { get; set; }
        public string? PortfolioUrl { get; set; }

        // CLIENT
        public string? DisplayName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? LocationText { get; set; }
        public string? ClientType { get; set; }
    }
}