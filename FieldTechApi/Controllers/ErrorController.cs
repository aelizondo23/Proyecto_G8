using Dapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FieldTechApi.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController(IConfiguration _config) : ControllerBase
    {
        [Route("CapturarError")]
        public IActionResult CapturarError()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var usuario = User.FindFirst("userId")?.Value ?? "0";

            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));
            var parametros = new DynamicParameters();
            parametros.Add("@Error", exception?.Error.Message);
            parametros.Add("@Fecha", DateTime.Now);
            parametros.Add("@Origen", exception?.Path);
            parametros.Add("@Usuario", usuario);

            // context.Execute("sp_RegistrarError", parametros);

            return StatusCode(500, "Se presentó un error en el servicio. Por favor intenta nuevamente más tarde.");
        }
    }
}
