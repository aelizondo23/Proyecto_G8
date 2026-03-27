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

        [HttpPut("CambiarContrasenna")]
        public IActionResult CambiarContrasenna(CambiarContrasennaRequest modelo)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));
            var parametros = new DynamicParameters();
            parametros.Add("@UserId", UserId);
            parametros.Add("@NewPasswordHash", modelo.NuevaContrasenna);

            var result = context.Execute("sp_UpdateUserCredentials", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result <= 0)
                return BadRequest("Su información no se actualizó correctamente.");

            return Ok("Su información se actualizó correctamente.");
        }
    }
}
