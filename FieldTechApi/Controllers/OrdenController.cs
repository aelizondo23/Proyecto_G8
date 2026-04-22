using System.ComponentModel.DataAnnotations;
using Dapper;
using FieldTechApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FieldTechApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenController(IConfiguration _config) : ControllerBase
    {
        private int UserId => int.Parse(User.FindFirst("userId")!.Value);
        private string TipoUsuario => User.FindFirst("tipoUsuario")!.Value;

        #region Órdenes de Trabajo

        [HttpGet("ListarOrdenes")]
        public IActionResult ListarOrdenes(string? status, string? categoria, string? urgencia, string? zona,
    bool soloDisponibles = false, int pagina = 1, int tamano = 20)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@Status", status);
            parametros.Add("@Category", categoria);
            parametros.Add("@Urgency", urgencia);
            parametros.Add("@Zone", zona);
            parametros.Add("@SoloDisponibles", soloDisponibles);
            parametros.Add("@PageNum", pagina);
            parametros.Add("@PageSize", tamano);

            var result = context.Query<OrdenResponse>("sp_ListWorkOrders", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpGet("ConsultarOrden")]
        public IActionResult ConsultarOrden(int id)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", id);

            var result = context.QueryFirstOrDefault<OrdenResponse>("sp_GetWorkOrder", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("La orden no fue encontrada.");

            return Ok(result);
        }

        [HttpPost("CrearOrden")]
        public IActionResult CrearOrden(CrearOrdenRequest modelo)
        {
            if (TipoUsuario != "CLIENT")
                return BadRequest("Solo los clientes pueden crear órdenes.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@ClientUserId", UserId);
            parametros.Add("@Title", modelo.Title);
            parametros.Add("@Description", modelo.Description);
            parametros.Add("@Category", modelo.Category);
            parametros.Add("@LocationText", modelo.LocationText);
            parametros.Add("@BudgetAmount", modelo.BudgetAmount);
            parametros.Add("@Urgency", modelo.Urgency);

            var result = context.QueryFirstOrDefault<dynamic>("sp_CreateWorkOrder", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("Su información no se registró correctamente.");
            RegistrarHistorial(context, (int)result.WorkOrderId, "CREAR_ORDEN", "Orden creada por cliente.");
            return Ok(result);
        }

        [HttpPut("ActualizarOrden")]
        public IActionResult ActualizarOrden(int id, ActualizarOrdenRequest modelo)
        {
            if (TipoUsuario != "CLIENT")
                return BadRequest("Solo los clientes pueden actualizar órdenes.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", id);
            parametros.Add("@Title", modelo.Titulo);
            parametros.Add("@Description", modelo.Descripcion);
            parametros.Add("@Category", modelo.Categoria);
            parametros.Add("@LocationText", modelo.Ubicacion);
            parametros.Add("@BudgetAmount", modelo.Presupuesto);
            parametros.Add("@Urgency", modelo.Urgencia);

            context.Execute("sp_UpdateWorkOrder", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok("Su información se actualizó correctamente.");
        }

        [HttpPut("CancelarOrden")]
        public IActionResult CancelarOrden(int id)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", id);

            context.Execute("sp_CancelWorkOrder", parametros,
                commandType: System.Data.CommandType.StoredProcedure);
            RegistrarHistorial(context, id, "CANCELAR_ORDEN", "Orden cancelada correctamente.");
            return Ok("Orden cancelada correctamente.");
        }

        #endregion

        #region Asignaciones

        [HttpPost("AplicarOrden")]
        public IActionResult AplicarOrden(int ordenId)
        {
            if (TipoUsuario != "TECH")
                return BadRequest("Solo los técnicos pueden aplicar a órdenes.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);
            parametros.Add("@TechnicianUserId", UserId);

            var result = context.QueryFirstOrDefault<dynamic>("sp_AplicarOrden", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("No se pudo aplicar a la orden.");
            RegistrarHistorial(context, ordenId, "APLICAR_ORDEN", "Técnico aplicó a la orden.");
            return Ok("Te asignaste a la orden correctamente.");
        }


        [HttpGet("ConsultarAsignaciones")]
        public IActionResult ConsultarAsignaciones(int ordenId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);

            var result = context.Query<AsignacionResponse>("sp_GetAssignmentsByWorkOrder", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpGet("MisAsignaciones")]
        public IActionResult MisAsignaciones(string? status)
        {
            if (TipoUsuario != "TECH")
                return BadRequest("Solo los técnicos pueden consultar sus asignaciones.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@TechnicianUserId", UserId);
            parametros.Add("@Status", status);

            var result = context.Query<AsignacionResponse>("sp_GetAssignmentsByTechnician", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpPost("AsignarTecnico")]
        public IActionResult AsignarTecnico(int ordenId, AsignacionRequest modelo)
        {
            if (TipoUsuario != "CLIENT")
                return BadRequest("Solo los clientes pueden asignar técnicos.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);
            parametros.Add("@TechnicianUserId", modelo.TechnicianUserId);
            parametros.Add("@AgreedAmount", modelo.MontoAcordado);

            var result = context.QueryFirstOrDefault<dynamic>("sp_CreateAssignment", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("Su información no se registró correctamente.");

            RegistrarHistorial(context, ordenId, "ASIGNAR_TECNICO", $"Técnico asignado: {modelo.TechnicianUserId}.");
            return Ok(result);
        }

        [HttpPut("ResponderAsignacion")]
        public IActionResult ResponderAsignacion(int asignacionId, ResponderAsignacionRequest modelo)
        {
            if (TipoUsuario != "TECH")
                return BadRequest("Solo los técnicos pueden responder asignaciones.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@AssignmentId", asignacionId);
            parametros.Add("@Accept", modelo.Aceptar);

            context.Execute("sp_RespondAssignment", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            var workOrderId = ObtenerWorkOrderIdPorAsignacion(context, asignacionId);

            RegistrarHistorial(
                context,
                workOrderId,
                modelo.Aceptar ? "ACEPTAR_ASIGNACION" : "RECHAZAR_ASIGNACION",
                modelo.Aceptar ? "Asignación aceptada por técnico." : "Asignación rechazada por técnico."
            );

            return Ok(modelo.Aceptar ? "Asignación aceptada correctamente." : "Asignación rechazada.");
        }

        [HttpPut("CompletarAsignacion")]
        public IActionResult CompletarAsignacion(int asignacionId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@AssignmentId", asignacionId);

            context.Execute("sp_CompleteAssignment", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            var workOrderId = ObtenerWorkOrderIdPorAsignacion(context, asignacionId);
            RegistrarHistorial(context, workOrderId, "COMPLETAR_ASIGNACION", "Orden marcada como completada.");
            return Ok("Orden marcada como completada.");
        }

        #endregion

        #region Check-In / Check-Out

        [HttpPost("CheckIn")]
        public IActionResult CheckIn(int ordenId, CheckInRequest modelo)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);
            parametros.Add("@TechnicianUserId", UserId);
            parametros.Add("@Notes", modelo.Notas);

            var result = context.QueryFirstOrDefault<dynamic>("sp_CheckIn", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("Su información no se registró correctamente.");

            RegistrarHistorial(context, ordenId, "CHECK_IN", "El técnico inició trabajo.");
            return Ok(result);
        }

        [HttpPut("CheckOut")]
        public IActionResult CheckOut(int checkInId, CheckOutRequest modelo)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@CheckInId", checkInId);
            parametros.Add("@Notes", modelo.Notas);

            context.Execute("sp_CheckOut", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            var workOrderId = ObtenerWorkOrderIdPorCheckIn(context, checkInId);
            RegistrarHistorial(context, workOrderId, "CHECK_OUT", "El técnico finalizó trabajo.");
            return Ok("Check-out registrado correctamente.");
        }

        [HttpGet("ConsultarCheckIns")]
        public IActionResult ConsultarCheckIns(int ordenId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);

            var result = context.Query<CheckInResponse>("sp_GetCheckInsByWorkOrder", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        #endregion

        #region Mensajes

        [HttpGet("ConsultarMensajes")]
        public IActionResult ConsultarMensajes(int ordenId, int pagina = 1, int tamano = 50)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);
            parametros.Add("@PageNum", pagina);
            parametros.Add("@PageSize", tamano);

            var result = context.Query<MensajeResponse>("sp_GetMessagesByWorkOrder", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpPost("EnviarMensaje")]
        public IActionResult EnviarMensaje(int ordenId, MensajeRequest modelo)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);
            parametros.Add("@SenderUserId", UserId);
            parametros.Add("@Body", modelo.Cuerpo);

            var result = context.QueryFirstOrDefault<dynamic>("sp_SendMessage", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("Su información no se registró correctamente.");

            return Ok(result);
        }

        #endregion

        #region Técnicos

        [HttpGet("ListarTecnicos")]
        public IActionResult ListarTecnicos(string? zona, string? disponibilidad, decimal? tarifaMax,
            int pagina = 1, int tamano = 20)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@Zone", zona);
            parametros.Add("@AvailabilityStatus", disponibilidad);
            parametros.Add("@MaxHourlyRate", tarifaMax);
            parametros.Add("@PageNum", pagina);
            parametros.Add("@PageSize", tamano);

            var result = context.Query<TecnicoResponse>("sp_ListTechnicians", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        #endregion


        public class NotaOrdenRequest
        {
            [Required]
            public string Texto { get; set; } = string.Empty;
        }

        public class NotaOrdenResponse
        {
            public int NoteId { get; set; }
            public int WorkOrderId { get; set; }
            public int UserId { get; set; }
            public string NoteText { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public string AuthorName { get; set; } = string.Empty;
        }

        public class HistorialOrdenResponse
        {
            public int HistoryId { get; set; }
            public int WorkOrderId { get; set; }
            public int UserId { get; set; }
            public string ActionType { get; set; } = string.Empty;
            public string? ActionDetail { get; set; }
            public DateTime CreatedAt { get; set; }
            public string UserName { get; set; } = string.Empty;
        }

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

        private void RegistrarHistorial(SqlConnection context, int workOrderId, string actionType, string? detail = null)
        {
            var p = new DynamicParameters();
            p.Add("@WorkOrderId", workOrderId);
            p.Add("@UserId", UserId);
            p.Add("@ActionType", actionType);
            p.Add("@ActionDetail", detail);

            context.Execute("sp_AddWorkOrderHistory", p,
                commandType: System.Data.CommandType.StoredProcedure);
        }

        #region Notas / Historial / Calendario

        [HttpPost("AgregarNota")]
        public IActionResult AgregarNota(int ordenId, NotaOrdenRequest modelo)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);
            parametros.Add("@UserId", UserId);
            parametros.Add("@NoteText", modelo.Texto);

            try
            {
                var result = context.QueryFirstOrDefault<dynamic>(
                    "sp_AddWorkOrderNote",
                    parametros,
                    commandType: System.Data.CommandType.StoredProcedure);

                RegistrarHistorial(context, ordenId, "AGREGAR_NOTA", "Se agregó una observación.");
                return Ok(result);
            }
            catch
            {
                return BadRequest("La nota no puede ir vacía.");
            }
        }

        [HttpGet("ConsultarNotas")]
        public IActionResult ConsultarNotas(int ordenId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);

            var result = context.Query<NotaOrdenResponse>(
                "sp_GetWorkOrderNotes",
                parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpGet("ConsultarHistorial")]
        public IActionResult ConsultarHistorial(int ordenId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);

            var result = context.Query<HistorialOrdenResponse>(
                "sp_GetWorkOrderHistory",
                parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpPost("CrearEventoCalendario")]
        public IActionResult CrearEventoCalendario(CrearEventoCalendarioRequest modelo)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", modelo.WorkOrderId);
            parametros.Add("@CreatedByUserId", UserId);
            parametros.Add("@Title", modelo.Title);
            parametros.Add("@StartAt", modelo.StartAt);
            parametros.Add("@EndAt", modelo.EndAt);
            parametros.Add("@Description", modelo.Description);

            try
            {
                var result = context.QueryFirstOrDefault<dynamic>(
                    "sp_CreateCalendarEvent",
                    parametros,
                    commandType: System.Data.CommandType.StoredProcedure);

                if (modelo.WorkOrderId.HasValue)
                    RegistrarHistorial(context, modelo.WorkOrderId.Value, "CREAR_EVENTO", "Evento de calendario creado.");

                return Ok(result);
            }
            catch
            {
                return BadRequest("Rango de fechas inválido.");
            }
        }

        [HttpGet("ConsultarEventosCalendario")]
        public IActionResult ConsultarEventosCalendario(DateTime inicio, DateTime fin)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);
            parametros.Add("@StartDate", inicio);
            parametros.Add("@EndDate", fin);

            var result = context.Query<EventoCalendarioResponse>(
                "sp_GetCalendarEvents",
                parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        #endregion

        private int ObtenerWorkOrderIdPorAsignacion(SqlConnection context, int asignacionId)
        {
            var p = new DynamicParameters();
            p.Add("@AssignmentId", asignacionId);

            return context.QueryFirst<int>(
                "SELECT WorkOrderId FROM Assignment WHERE AssignmentId = @AssignmentId", p);
        }

        private int ObtenerWorkOrderIdPorCheckIn(SqlConnection context, int checkInId)
        {
            var p = new DynamicParameters();
            p.Add("@CheckInId", checkInId);

            return context.QueryFirst<int>(
                "SELECT WorkOrderId FROM CheckInLog WHERE CheckInId = @CheckInId", p);
        }

        private SqlConnection Conn() =>
            new(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));
    }
}
