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
        private int UserId => int.Parse(User.FindFirst("userId")!.Value);
        private string UserType => User.FindFirst("tipoUsuario")!.Value;

        private SqlConnection Conn() =>
            new(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

        #region Cambiar Contraseña

        [HttpPut("CambiarContrasenna")]
        public IActionResult CambiarContrasenna(CambiarContrasennaRequest modelo)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);
            parametros.Add("@NewPasswordHash", modelo.NuevaContrasenna);

            context.Execute("sp_UpdateUserCredentials", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

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
                parametros.Add("@Bio", modelo.Bio);
                parametros.Add("@HourlyRate", modelo.HourlyRate);
                parametros.Add("@Zone", modelo.Zone);
                parametros.Add("@AvailabilityStatus", modelo.AvailabilityStatus);
                parametros.Add("@PortfolioUrl", modelo.PortfolioUrl);

                context.Execute("sp_UpdateTechnicianProfile", parametros,
                    commandType: System.Data.CommandType.StoredProcedure);
            }
            else
            {
                parametros.Add("@DisplayName", modelo.DisplayName);
                parametros.Add("@ContactName", modelo.ContactName);
                parametros.Add("@ContactPhone", modelo.ContactPhone);
                parametros.Add("@LocationText", modelo.LocationText);
                parametros.Add("@ClientType", modelo.ClientType);

                context.Execute("sp_UpdateClientProfile", parametros,
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            return Ok("Su información se actualizó correctamente.");
        }

        #endregion

        #region Foto de Perfil

        [HttpPost("SubirFotoPerfil")]
        public async Task<IActionResult> SubirFotoPerfil(IFormFile foto)
        {
            if (foto == null || foto.Length == 0)
                return BadRequest("No se recibió ninguna imagen.");

            if (foto.Length > 5 * 1024 * 1024)
                return BadRequest("La imagen no puede superar 5MB.");

            if (!foto.ContentType.StartsWith("image/"))
                return BadRequest("Solo se permiten imágenes.");

            using var ms = new MemoryStream();
            await foto.CopyToAsync(ms);
            var bytes = ms.ToArray();

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);
            parametros.Add("@MimeType", foto.ContentType);
            parametros.Add("@PhotoData", bytes);

            context.Execute("sp_SubirFotoPerfil", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok("Foto de perfil actualizada.");
        }

        [HttpGet("GetFotoPerfil")]
        public IActionResult GetFotoPerfil(int? userId = null)
        {
            var targetId = userId ?? UserId;

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", targetId);

            var result = context.QueryFirstOrDefault<dynamic>("sp_GetFotoPerfil", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null) return NotFound();

            byte[] data = result.PhotoData;
            return File(data, (string)result.MimeType);
        }

        #endregion

        #region Educación

        [HttpGet("GetEducacion")]
        public IActionResult GetEducacion()
        {
            if (UserType != "TECH")
                return BadRequest("Solo los técnicos tienen educación registrada.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);

            var result = context.Query<EducacionResponse>("sp_GetEducacion", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpPost("AddEducacion")]
        public IActionResult AddEducacion(EducacionRequest modelo)
        {
            if (UserType != "TECH")
                return BadRequest("Solo los técnicos pueden agregar educación.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);
            parametros.Add("@Institution", modelo.Institution);
            parametros.Add("@Degree", modelo.Degree);
            parametros.Add("@FieldOfStudy", modelo.FieldOfStudy);
            parametros.Add("@StartYear", modelo.StartYear);
            parametros.Add("@EndYear", modelo.EndYear);

            var result = context.QueryFirstOrDefault<dynamic>("sp_AddEducacion", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpPost("DeleteEducacion")]
        public IActionResult DeleteEducacion(int educacionId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@EducationId", educacionId);
            parametros.Add("@UserId", UserId);

            context.Execute("sp_DeleteEducacion", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok("Registro eliminado.");
        }

        #endregion

        #region Experiencia

        [HttpGet("GetExperiencia")]
        public IActionResult GetExperiencia()
        {
            if (UserType != "TECH")
                return BadRequest("Solo los técnicos tienen experiencia registrada.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);

            var result = context.Query<ExperienciaResponse>("sp_GetExperiencia", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpPost("AddExperiencia")]
        public IActionResult AddExperiencia(ExperienciaRequest modelo)
        {
            if (UserType != "TECH")
                return BadRequest("Solo los técnicos pueden agregar experiencia.");

            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);
            parametros.Add("@CompanyName", modelo.CompanyName);
            parametros.Add("@RoleName", modelo.RoleName);
            parametros.Add("@StartYear", modelo.StartYear);
            parametros.Add("@EndYear", modelo.EndYear);
            parametros.Add("@Description", modelo.Description);

            var result = context.QueryFirstOrDefault<dynamic>("sp_AddExperiencia", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(result);
        }

        [HttpPost("DeleteExperiencia")]
        public IActionResult DeleteExperiencia(int experienciaId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@ExperienceId", experienciaId);
            parametros.Add("@UserId", UserId);

            context.Execute("sp_DeleteExperiencia", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok("Registro eliminado.");
        }

        #endregion

        #region Perfil Público

        [HttpGet("GetPerfilPublico")]
        public IActionResult GetPerfilPublico(int userId)
        {
            using var context = Conn();
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", userId);

            var perfil = context.QueryFirstOrDefault<dynamic>("sp_GetTechnicianProfilePublico", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (perfil == null) return NotFound();

            var educacion = context.Query<EducacionResponse>("sp_GetEducacion", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            var experiencia = context.Query<ExperienciaResponse>("sp_GetExperiencia", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            return Ok(new
            {
                perfil.UserId,
                perfil.FullName,
                perfil.Email,
                perfil.Bio,
                perfil.HourlyRate,
                perfil.Zone,
                perfil.AvailabilityStatus,
                perfil.PortfolioUrl,
                Educacion = educacion,
                Experiencia = experiencia
            });
        }

        #endregion
    }
}