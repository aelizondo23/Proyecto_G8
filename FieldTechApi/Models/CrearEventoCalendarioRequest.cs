using System.ComponentModel.DataAnnotations;

namespace FieldTechApi.Models
{
    public class CrearEventoCalendarioRequest
    {
        public int? WorkOrderId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }

    public class EventoCalendarioResponse
    {
        public int EventId { get; set; }
        public int? WorkOrderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? Description { get; set; }
    }
}
