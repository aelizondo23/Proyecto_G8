using FieldTechWeb.Filters;
using FieldTechWeb.Models;
using FieldTechWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FieldTechWeb.Controllers
{
    public class HomeController(IHttpClientFactory _http, IConfiguration _config, IUtilitario _util) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var nombre = HttpContext.Session.GetString("NombreUsuario");
            if (!string.IsNullOrEmpty(nombre))
                return RedirectToAction("Dashboard", "Orden");
            return RedirectToAction("Login", "Home");
        }

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Usuario modelo)
        {
            modelo.Contrasenna = _util.Encrypt(modelo.Contrasenna);

            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Home/IniciarSesion";
            var result = client.PostAsJsonAsync(url, modelo).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var datos = result.Content.ReadFromJsonAsync<Usuario>().Result;
                HttpContext.Session.SetString("NombreUsuario", datos!.Nombre + " " + datos.Apellido);
                HttpContext.Session.SetInt32("UserId", datos!.UserId);
                HttpContext.Session.SetString("Token", datos!.Token);
                HttpContext.Session.SetString("TipoUsuario", datos!.UserType);
                return RedirectToAction("Dashboard", "Orden");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View();
        }

        #endregion

        #region Registro

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario modelo)
        {
            modelo.Contrasenna = _util.Encrypt(modelo.Contrasenna);

            using var client = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "Home/RegistrarCuenta";
            var result = client.PostAsJsonAsync(url, modelo).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
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

        #region Cerrar Sesión

        [HttpGet]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        #endregion
    }
}