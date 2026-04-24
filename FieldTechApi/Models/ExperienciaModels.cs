namespace FieldTechApi.Models
{
    public class ExperienciaRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public string? Description { get; set; }
    }

    public class ExperienciaResponse
    {
        public int ExperienceId { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public string? Description { get; set; }
    }
}