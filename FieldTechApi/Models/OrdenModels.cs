using System.ComponentModel.DataAnnotations;

namespace FieldTechApi.Models
{
    public class CrearOrdenRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? LocationText { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string Urgency { get; set; } = "NORMAL";
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class ActualizarOrdenRequest
    {
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public string? Ubicacion { get; set; }
        public decimal? Presupuesto { get; set; }
        public string? Urgencia { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class OrdenResponse
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
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ClientName { get; set; }
        public string? ClientDisplayName { get; set; }
        public string? ClientPhone { get; set; }
    }

    public class AsignacionRequest
    {
        [Required]
        public int TechnicianUserId { get; set; }
        public decimal? MontoAcordado { get; set; }
    }

    public class ResponderAsignacionRequest
    {
        [Required]
        public bool Aceptar { get; set; }
    }

    public class AsignacionResponse
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

    public class CheckInRequest
    {
        public string? Notas { get; set; }
    }

    public class CheckOutRequest
    {
        public string? Notas { get; set; }
    }

    public class CheckInResponse
    {
        public int CheckInId { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Notes { get; set; }
        public string? TechnicianName { get; set; }
    }

    public class MensajeRequest
    {
        [Required]
        public string Cuerpo { get; set; } = string.Empty;
    }

    public class MensajeResponse
    {
        public int MessageId { get; set; }
        public string? Body { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int SenderUserId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderType { get; set; }
    }

    public class TecnicoResponse
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

    public class ArchivoOrdenResponse
    {
        public int FileId { get; set; }
        public int WorkOrderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UploaderName { get; set; } = string.Empty;
    }
}