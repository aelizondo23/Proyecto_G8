using System.ComponentModel.DataAnnotations;
using Dapper;
using FieldTechApi.Models;
using FieldTechApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FieldTechApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenController(IConfiguration _config, ICorreoService _correo) : ControllerBase
    {
        private int UserId => int.Parse(User.FindFirst("userId")!.Value);
        private string TipoUsuario => User.FindFirst("tipoUsuario")!.Value;

        #region Órdenes de Trabajo

        [HttpGet("MisOrdenes")]
        public IActionResult MisOrdenes(int pagina = 1, int tamano = 50)
        {
            if (TipoUsuario != "CLIENT")
                return BadRequest("Solo los clientes pueden consultar sus órdenes.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@ClientUserId", UserId);
            parametros.Add("@PageNum", pagina);
            parametros.Add("@PageSize", tamano);

            var result = context.Query<OrdenResponse>("sp_ListWorkOrdersByClient", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }


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
        public async Task<IActionResult> CrearOrden(CrearOrdenRequest modelo)
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
            parametros.Add("@ContactName", modelo.ContactName);
            parametros.Add("@ContactPhone", modelo.ContactPhone);

            var result = context.QueryFirstOrDefault<dynamic>("sp_CreateWorkOrder", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("Su información no se registró correctamente.");

            RegistrarHistorial(context, (int)result.WorkOrderId, "CREAR_ORDEN", "Orden creada por cliente.");

            // Correo al cliente
            try
            {
                var cliente = ObtenerUsuario(context, UserId);
                if (cliente != null)
                    await _correo.EnviarAsync(
                        cliente.Email,
                        "Tu orden fue publicada en FieldTech",
                        CorreoTemplates.OrdenCreada(
                            cliente.FirstName + " " + cliente.LastName,
                            modelo.Title,
                            modelo.Category,
                            modelo.LocationText,
                            modelo.BudgetAmount));
            }
            catch { /* No bloquear si falla el correo */ }

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
            parametros.Add("@ContactName", modelo.ContactName);
            parametros.Add("@ContactPhone", modelo.ContactPhone);

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

        [HttpGet("ConsultarEventosOrden")]
        public IActionResult ConsultarEventosOrden(int ordenId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);
            var result = context.Query<EventoCalendarioResponse>("sp_GetEventosPorOrden", parametros,
                commandType: System.Data.CommandType.StoredProcedure);
            return Ok(result);
        }

        [HttpGet("ConsultarArchivos")]
        public IActionResult ConsultarArchivos(int ordenId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);
            var result = context.Query<ArchivoOrdenResponse>("sp_GetArchivosPorOrden", parametros,
                commandType: System.Data.CommandType.StoredProcedure);
            return Ok(result);
        }

        [HttpPost("SubirArchivo")]
        public async Task<IActionResult> SubirArchivo(int ordenId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se recibió ningún archivo.");

            if (archivo.Length > 10 * 1024 * 1024)
                return BadRequest("El archivo no puede superar 10MB.");

            using var ms = new MemoryStream();
            await archivo.CopyToAsync(ms);
            var bytes = ms.ToArray();

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);
            parametros.Add("@UploadedByUserId", UserId);
            parametros.Add("@FileName", archivo.FileName);
            parametros.Add("@MimeType", archivo.ContentType);
            parametros.Add("@FileData", bytes);

            var result = context.QueryFirstOrDefault<dynamic>("sp_SubirArchivoOrden", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            RegistrarHistorial(context, ordenId, "SUBIR_ARCHIVO", $"Archivo subido: {archivo.FileName}");
            return Ok(result);
        }

        [HttpGet("DescargarArchivo")]
        public IActionResult DescargarArchivo(int fileId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@FileId", fileId);
            var result = context.QueryFirstOrDefault<dynamic>("sp_GetArchivo", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null) return NotFound();

            byte[] data = result.FileData;
            return File(data, (string)result.MimeType, (string)result.FileName);
        }

        #endregion

        #region Asignaciones

        [HttpPost("AplicarOrden")]
        public async Task<IActionResult> AplicarOrden(int ordenId)
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

            // Correo al cliente
            try
            {
                var orden = context.QueryFirstOrDefault<dynamic>(
                    "SELECT wo.ClientUserId, wo.Title, u.Email, u.FirstName, u.LastName " +
                    "FROM WorkOrder wo INNER JOIN Users u ON u.UserId = wo.ClientUserId " +
                    "WHERE wo.WorkOrderId = @Id", new { Id = ordenId });

                var tecnico = ObtenerUsuario(context, UserId);

                if (orden != null && tecnico != null)
                    await _correo.EnviarAsync(
                        (string)orden.Email,
                        "Un técnico aplicó a tu orden en FieldTech",
                        CorreoTemplates.TecnicoAplicoOrden(
                            orden.FirstName + " " + orden.LastName,
                            (string)orden.Title,
                            tecnico.FirstName + " " + tecnico.LastName));
            }
            catch { }

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
        public async Task<IActionResult> AsignarTecnico(int ordenId, AsignacionRequest modelo)
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

            // Correo al técnico
            try
            {
                var orden = context.QueryFirstOrDefault<dynamic>(
                    "SELECT wo.Title, wo.LocationText, u.FirstName, u.LastName " +
                    "FROM WorkOrder wo INNER JOIN Users u ON u.UserId = wo.ClientUserId " +
                    "WHERE wo.WorkOrderId = @Id", new { Id = ordenId });

                var tecnico = ObtenerUsuario(context, modelo.TechnicianUserId);

                if (orden != null && tecnico != null)
                    await _correo.EnviarAsync(
                        tecnico.Email,
                        "¡Te han asignado una orden en FieldTech!",
                        CorreoTemplates.OrdenAsignada(
                            tecnico.FirstName + " " + tecnico.LastName,
                            (string)orden.Title,
                            orden.FirstName + " " + orden.LastName,
                            (string)(orden.LocationText ?? "—")));
            }
            catch { }

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
                context, workOrderId,
                modelo.Aceptar ? "ACEPTAR_ASIGNACION" : "RECHAZAR_ASIGNACION",
                modelo.Aceptar ? "Asignación aceptada por técnico." : "Asignación rechazada por técnico.");

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
        public async Task<IActionResult> EnviarMensaje(int ordenId, MensajeRequest modelo)
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

            // Correo al destinatario
            try
            {
                var orden = context.QueryFirstOrDefault<dynamic>(
                    "SELECT wo.ClientUserId, wo.Title FROM WorkOrder wo WHERE wo.WorkOrderId = @Id",
                    new { Id = ordenId });

                var remitente = ObtenerUsuario(context, UserId);

                if (orden != null && remitente != null)
                {
                    // Si el remitente es el cliente, notificar al técnico asignado
                    // Si el remitente es el técnico, notificar al cliente
                    int destinatarioId;
                    if (TipoUsuario == "CLIENT")
                    {
                        var asignacion = context.QueryFirstOrDefault<dynamic>(
                            "SELECT TechnicianUserId FROM WorkOrderAssignment " +
                            "WHERE WorkOrderId = @Id AND Status = 'ACCEPTED'",
                            new { Id = ordenId });
                        if (asignacion == null) goto SkipCorreo;
                        destinatarioId = (int)asignacion.TechnicianUserId;
                    }
                    else
                    {
                        destinatarioId = (int)orden.ClientUserId;
                    }

                    var destinatario = ObtenerUsuario(context, destinatarioId);
                    if (destinatario != null)
                    {
                        var preview = modelo.Cuerpo.Length > 80
                            ? modelo.Cuerpo[..80] + "..."
                            : modelo.Cuerpo;

                        await _correo.EnviarAsync(
                            destinatario.Email,
                            $"Nuevo mensaje en tu orden — FieldTech",
                            CorreoTemplates.NuevoMensaje(
                                destinatario.FirstName + " " + destinatario.LastName,
                                remitente.FirstName + " " + remitente.LastName,
                                (string)orden.Title,
                                preview));
                    }
                }
            SkipCorreo:;
            }
            catch { }

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

        #region Notas / Historial / Calendario

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
                var result = context.QueryFirstOrDefault<dynamic>("sp_AddWorkOrderNote", parametros,
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

            var result = context.Query<NotaOrdenResponse>("sp_GetWorkOrderNotes", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpGet("ConsultarHistorial")]
        public IActionResult ConsultarHistorial(int ordenId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@WorkOrderId", ordenId);

            var result = context.Query<HistorialOrdenResponse>("sp_GetWorkOrderHistory", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpPost("CrearEventoCalendario")]
        public async Task<IActionResult> CrearEventoCalendario(CrearEventoCalendarioRequest modelo)
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
                var result = context.QueryFirstOrDefault<dynamic>("sp_CreateCalendarEvent", parametros,
                    commandType: System.Data.CommandType.StoredProcedure);

                if (modelo.WorkOrderId.HasValue)
                {
                    RegistrarHistorial(context, modelo.WorkOrderId.Value, "CREAR_EVENTO", "Evento de calendario creado.");

                    // Correos a cliente y técnico asignado
                    try
                    {
                        var orden = context.QueryFirstOrDefault<dynamic>(
                            "SELECT wo.Title, wo.ClientUserId FROM WorkOrder wo WHERE wo.WorkOrderId = @Id",
                            new { Id = modelo.WorkOrderId.Value });

                        if (orden != null)
                        {
                            var cliente = ObtenerUsuario(context, (int)orden.ClientUserId);
                            if (cliente != null)
                                await _correo.EnviarAsync(
                                    cliente.Email,
                                    "Fecha agendada para tu orden — FieldTech",
                                    CorreoTemplates.FechaAgendada(
                                        cliente.FirstName + " " + cliente.LastName,
                                        (string)orden.Title,
                                        modelo.StartAt, modelo.EndAt));

                            var asignacion = context.QueryFirstOrDefault<dynamic>(
                                "SELECT TechnicianUserId FROM WorkOrderAssignment " +
                                "WHERE WorkOrderId = @Id AND Status = 'ACCEPTED'",
                                new { Id = modelo.WorkOrderId.Value });

                            if (asignacion != null)
                            {
                                var tecnico = ObtenerUsuario(context, (int)asignacion.TechnicianUserId);
                                if (tecnico != null)
                                    await _correo.EnviarAsync(
                                        tecnico.Email,
                                        "Fecha agendada para tu orden — FieldTech",
                                        CorreoTemplates.FechaAgendada(
                                            tecnico.FirstName + " " + tecnico.LastName,
                                            (string)orden.Title,
                                            modelo.StartAt, modelo.EndAt));
                            }
                        }
                    }
                    catch { }
                }

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

            var result = context.Query<EventoCalendarioResponse>("sp_GetCalendarEvents", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        #endregion

        #region Helpers

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

        private dynamic? ObtenerUsuario(SqlConnection context, int userId)
        {
            return context.QueryFirstOrDefault<dynamic>(
                "SELECT UserId, Email, FirstName, LastName FROM Users WHERE UserId = @UserId",
                new { UserId = userId });
        }

        private int ObtenerWorkOrderIdPorAsignacion(SqlConnection context, int asignacionId)
        {
            var p = new DynamicParameters();
            p.Add("@AssignmentId", asignacionId);
            return context.QueryFirst<int>(
                "SELECT WorkOrderId FROM WorkOrderAssignment WHERE AssignmentId = @AssignmentId", p);
        }

        private int ObtenerWorkOrderIdPorCheckIn(SqlConnection context, int checkInId)
        {
            var p = new DynamicParameters();
            p.Add("@CheckInId", checkInId);
            return context.QueryFirst<int>(
                "SELECT WorkOrderId FROM WorkOrderCheckIn WHERE CheckInId = @CheckInId", p);
        }

        private SqlConnection Conn() =>
            new(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

        #endregion
    }
}