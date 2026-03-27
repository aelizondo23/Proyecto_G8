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

        private SqlConnection Conn() =>
            new(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));
    }
}
