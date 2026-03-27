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
    public class SeguridadController(IConfiguration _config) : ControllerBase
    {
        private int    UserId    => int.Parse(User.FindFirst("userId")!.Value);
        private string UserType  => User.FindFirst("tipoUsuario")!.Value;

        private SqlConnection Conn() =>
            new(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

        #region Cambiar Contraseña

        [HttpPut("CambiarContrasenna")]
        public IActionResult CambiarContrasenna(CambiarContrasennaRequest modelo)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId",          UserId);
            parametros.Add("@NewPasswordHash", modelo.NuevaContrasenna);

            var result = context.Execute("sp_UpdateUserCredentials", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result <= 0)
                return BadRequest("Su información no se actualizó correctamente.");

            return Ok("Su información se actualizó correctamente.");
        }

        #endregion

        #region Consultar Perfil

        [HttpGet("ConsultarPerfil")]
        public IActionResult ConsultarPerfil()
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);

            if (UserType == "TECH")
            {
                var result = context.QueryFirstOrDefault<dynamic>("sp_GetTechnicianProfile", parametros,
                    commandType: System.Data.CommandType.StoredProcedure);

                if (result == null)
                    return BadRequest("No se encontró la información del perfil.");

                return Ok(result);
            }
            else
            {
                var result = context.QueryFirstOrDefault<dynamic>("sp_GetClientProfile", parametros,
                    commandType: System.Data.CommandType.StoredProcedure);

                if (result == null)
                    return BadRequest("No se encontró la información del perfil.");

                return Ok(result);
            }
        }

        #endregion

        #region Actualizar Perfil

        [HttpPut("ActualizarPerfil")]
        public IActionResult ActualizarPerfil(ActualizarPerfilRequest modelo)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);

            if (UserType == "TECH")
            {
                parametros.Add("@Bio",                modelo.Bio);
                parametros.Add("@HourlyRate",         modelo.HourlyRate);
                parametros.Add("@Zone",               modelo.Zone);
                parametros.Add("@AvailabilityStatus", modelo.AvailabilityStatus);
                parametros.Add("@PortfolioUrl",       modelo.PortfolioUrl);

                context.Execute("sp_UpdateTechnicianProfile", parametros,
                    commandType: System.Data.CommandType.StoredProcedure);
            }
            else
            {
                parametros.Add("@DisplayName",  modelo.DisplayName);
                parametros.Add("@ContactName",  modelo.ContactName);
                parametros.Add("@ContactPhone", modelo.ContactPhone);
                parametros.Add("@LocationText", modelo.LocationText);

                context.Execute("sp_UpdateClientProfile", parametros,
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            return Ok("Su información se actualizó correctamente.");
        }

        #endregion
    }
}
