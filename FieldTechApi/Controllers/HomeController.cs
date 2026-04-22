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
using System.Security.Cryptography;

namespace FieldTechApi.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController(IConfiguration _config, IUtilitario _util) : ControllerBase
    {
        #region Registrar Cuenta

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

        #endregion

        #region Iniciar Sesión

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

        #endregion

        #region Recuperar Acceso

        [HttpPut("RecuperarAcceso")]
        public IActionResult RecuperarAcceso(RecuperarAccesoRequest modelo)
        {
            using var context = new SqlConnection(_config.GetValue<string>("ConnectionStrings:DefaultConnection"));

            // Validar que el correo exista en la base de datos
            var parametrosValidar = new DynamicParameters();
            parametrosValidar.Add("@Email", modelo.Email);

            var result = context.QueryFirstOrDefault<UsuarioResponse>("sp_ValidarCorreo", parametrosValidar,
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
                return BadRequest("Su información no se validó correctamente.");

            // Generar nueva contraseña temporal
            var nuevaContrasenna = GenerarContrasenna();

            // Actualizar la contraseña en la base de datos
            var parametrosActualizar = new DynamicParameters();
            parametrosActualizar.Add("@UserId", result.UserId);
            parametrosActualizar.Add("@NewPasswordHash", _util.Encrypt(nuevaContrasenna));

            var resultActualizar = context.Execute("sp_UpdateUserCredentials", parametrosActualizar,
                commandType: System.Data.CommandType.StoredProcedure);

            if (resultActualizar <= 0)
                return BadRequest("Su información no se actualizó correctamente.");

            // Enviar correo con la nueva contraseña
            var contenido = CargarPlantilla("RecuperarAcceso.html")
                .Replace("{{Nombre}}", result.FirstName)
                .Replace("{{Contrasenna}}", nuevaContrasenna)
                .Replace("{{AnnioActual}}", DateTime.Now.Year.ToString());

            _util.EnviarCorreo(modelo.Email, "Recuperación de acceso - FieldTech", contenido);

            return Ok("Se envió una contraseña temporal a su correo electrónico.");
        }

        #endregion

        #region Métodos privados

        private static string GenerarContrasenna()
        {
            const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var r = new Random();
            return new string([.. Enumerable.Range(0, 8).Select(x => letras[r.Next(letras.Length)])]);
        }

        private static string CargarPlantilla(string nombreArchivo)
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "Templates", nombreArchivo);
            return System.IO.File.ReadAllText(ruta);
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

        #endregion
    }
}
