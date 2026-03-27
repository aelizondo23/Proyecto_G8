namespace FieldTechWeb.Models
{
    public class Usuario
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Contrasenna { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class Orden
    {
        public int WorkOrderId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? LocationText { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string? Urgency { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ClientName { get; set; }
        public string? ClientDisplayName { get; set; }
        public string? ClientPhone { get; set; }
    }

    public class Asignacion
    {
        public int AssignmentId { get; set; }
        public string? Status { get; set; }
        public decimal? AgreedAmount { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public int TechnicianUserId { get; set; }
        public string? TechnicianName { get; set; }
        public string? Zone { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? AvailabilityStatus { get; set; }
        public int WorkOrderId { get; set; }
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? LocationText { get; set; }
        public string? Urgency { get; set; }
        public decimal? BudgetAmount { get; set; }
    }

    public class CheckIn
    {
        public int CheckInId { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Notes { get; set; }
        public string? TechnicianName { get; set; }
    }

    public class Mensaje
    {
        public int MessageId { get; set; }
        public string? Body { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int SenderUserId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderType { get; set; }
    }

    public class Tecnico
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Zone { get; set; }
        public string? AvailabilityStatus { get; set; }
        public string? PortfolioUrl { get; set; }
        public int CompletedJobs { get; set; }
    }

    public class DetalleOrdenViewModel
    {
        public Orden Orden { get; set; } = new();
        public List<Asignacion> Asignaciones { get; set; } = new();
        public List<CheckIn> CheckIns { get; set; } = new();
        public List<Mensaje> Mensajes { get; set; } = new();
    }
}
