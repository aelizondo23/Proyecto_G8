using Dapper;
using FieldTechApi.Models;
using FieldTechApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FieldTechApi.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController(IConfiguration _config, IUtilitario _util) : ControllerBase
    {
        [HttpPost("RegistrarCuenta")]
        public IActionResult RegistrarCuenta(RegistroUsuarioRequest modelo)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));
            var parametros = new DynamicParameters();
            parametros.Add("@Email", modelo.Email);
            parametros.Add("@PasswordHash", modelo.Contrasenna);
            parametros.Add("@UserType", modelo.TipoUsuario);
            parametros.Add("@FirstName", modelo.Nombre);
            parametros.Add("@LastName", modelo.Apellido);

            var result = context.QueryFirstOrDefault<dynamic>("sp_RegisterUser", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("Su información no se registró correctamente.");

            return Ok("Su información se registró correctamente.");
        }

        [HttpPost("IniciarSesion")]
        public IActionResult IniciarSesion(IniciarSesionRequest modelo)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));
            var parametros = new DynamicParameters();
            parametros.Add("@Email", modelo.Email);
            parametros.Add("@PasswordHash", modelo.Contrasenna);

            var result = context.QueryFirstOrDefault<UsuarioResponse>("sp_LoginUser", parametros,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("Su información no se autenticó correctamente.");

            result.Token = GenerarToken(result.UserId, result.UserType);
            return Ok(result);
        }

        private string GenerarToken(int userId, string userType)
        {
            var key = Encoding.UTF8.GetBytes(_config.GetValue<string>("Jwt:Key")!);

            var claims = new[]
            {
                new Claim("userId", userId.ToString()),
                new Claim("tipoUsuario", userType)
            };

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            );

            var tokenDescriptor = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}