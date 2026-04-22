using System.ComponentModel.DataAnnotations;

namespace FieldTechWeb.Models
{
    public class NotaOrden
    {
        public int NoteId { get; set; }
        public int WorkOrderId { get; set; }
        public int UserId { get; set; }
        public string NoteText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }

    public class HistorialOrden
    {
        public int HistoryId { get; set; }
        public int WorkOrderId { get; set; }
        public int UserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? ActionDetail { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class EventoCalendario
    {
        public int EventId { get; set; }
        public int? WorkOrderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? Description { get; set; }
    }
}