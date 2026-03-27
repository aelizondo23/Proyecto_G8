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
        private string UrlAPI     => _config.GetValue<string>("Valores:UrlAPI")!;
        private string Token      => HttpContext.Session.GetString("Token")!;
        private string TipoUsuario => HttpContext.Session.GetString("TipoUsuario")!;

        private HttpClient CrearCliente()
        {
            var client = _http.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            return client;
        }

        #region Perfil

        [HttpGet]
        public IActionResult Perfil()
        {
            using var client = CrearCliente();
            var url    = UrlAPI + "Seguridad/ConsultarPerfil";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var datos = result.Content.ReadFromJsonAsync<PerfilUsuarioViewModel>().Result;
                return View(datos);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View(new PerfilUsuarioViewModel());
        }

        [HttpPost]
        public IActionResult Perfil(PerfilUsuarioViewModel modelo)
        {
            using var client = CrearCliente();
            var url    = UrlAPI + "Seguridad/ActualizarPerfil";
            var result = client.PutAsJsonAsync(url, modelo).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                ViewBag.Exito = "Perfil actualizado correctamente.";
                return View(modelo);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View(modelo);
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
            modelo.NuevaContrasenna     = _util.Encrypt(modelo.NuevaContrasenna);
            modelo.ConfirmarContrasenna = _util.Encrypt(modelo.ConfirmarContrasenna);

            using var client = CrearCliente();
            var url    = UrlAPI + "Seguridad/CambiarContrasenna";
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
