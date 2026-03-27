using FieldTechWeb.Filters;
using FieldTechWeb.Models;
using FieldTechWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace FieldTechWeb.Controllers
{
    [ValidarSesion]
    public class SeguridadController(IHttpClientFactory _http, IConfiguration _config, IUtilitario _util) : Controller
    {
        private string UrlAPI => _config.GetValue<string>("Valores:UrlAPI")!;
        private string Token => HttpContext.Session.GetString("Token")!;

        #region Perfil

        [HttpGet]
        public IActionResult Perfil()
        {
            return View();
        }

        #endregion

        #region Cambiar Contraseña

        [HttpGet]
        public IActionResult CambiarContrasenna()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CambiarContrasenna(CambiarContrasennaViewModel modelo)
        {
            modelo.NuevaContrasenna    = _util.Encrypt(modelo.NuevaContrasenna);
            modelo.ConfirmarContrasenna = _util.Encrypt(modelo.ConfirmarContrasenna);

            using var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            var url = UrlAPI + "Seguridad/CambiarContrasenna";
            var result = client.PutAsJsonAsync(url, modelo).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View();
        }

        #endregion
    }
}
