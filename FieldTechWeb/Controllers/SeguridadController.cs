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
        private string TipoUsuario => HttpContext.Session.GetString("TipoUsuario")!;
        private int UserId => HttpContext.Session.GetInt32("UserId") ?? 0;

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

            var perfilResp = client.GetAsync(UrlAPI + "Seguridad/ConsultarPerfil").Result;
            if (perfilResp.StatusCode == HttpStatusCode.InternalServerError) throw new Exception();

            var vm = perfilResp.StatusCode == HttpStatusCode.OK
                ? perfilResp.Content.ReadFromJsonAsync<PerfilUsuarioViewModel>().Result ?? new()
                : new PerfilUsuarioViewModel();

            if (TipoUsuario == "TECH")
            {
                var eduResp = client.GetAsync(UrlAPI + "Seguridad/GetEducacion").Result;
                vm.Educacion = eduResp.StatusCode == HttpStatusCode.OK
                    ? eduResp.Content.ReadFromJsonAsync<List<EducacionViewModel>>().Result ?? new()
                    : new();

                var expResp = client.GetAsync(UrlAPI + "Seguridad/GetExperiencia").Result;
                vm.Experiencia = expResp.StatusCode == HttpStatusCode.OK
                    ? expResp.Content.ReadFromJsonAsync<List<ExperienciaViewModel>>().Result ?? new()
                    : new();
            }

            return View(vm);
        }

        [HttpPost]
        public IActionResult Perfil(PerfilUsuarioViewModel modelo)
        {
            using var client = CrearCliente();
            var result = client.PutAsJsonAsync(UrlAPI + "Seguridad/ActualizarPerfil", modelo).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                ViewBag.Exito = "Perfil actualizado correctamente.";
                return RedirectToAction("Perfil");
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
            modelo.NuevaContrasenna = _util.Encrypt(modelo.NuevaContrasenna);
            modelo.ConfirmarContrasenna = _util.Encrypt(modelo.ConfirmarContrasenna);

            using var client = CrearCliente();
            var result = client.PutAsJsonAsync(UrlAPI + "Seguridad/CambiarContrasenna", modelo).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                HttpContext.Session.Clear();
                TempData["ContrasennaActualizada"] = true;
                return View();
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View();
        }

        #endregion

        #region Foto de Perfil

        [HttpPost]
        public async Task<IActionResult> SubirFotoPerfil(IFormFile foto)
        {
            using var client = CrearCliente();
            using var content = new MultipartFormDataContent();
            using var stream = foto.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(foto.ContentType);
            content.Add(fileContent, "foto", foto.FileName);

            var result = await client.PostAsync(UrlAPI + "Seguridad/SubirFotoPerfil", content);

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Perfil");
        }

        [HttpGet]
        public async Task<IActionResult> VerFotoPerfil(int? userId = null)
        {
            using var client = CrearCliente();
            var targetId = userId ?? UserId;
            var response = await client.GetAsync(UrlAPI + $"Seguridad/GetFotoPerfil?userId={targetId}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
            return File(bytes, contentType);
        }

        #endregion

        #region Educación

        [HttpPost]
        public IActionResult AddEducacion(EducacionViewModel modelo)
        {
            using var client = CrearCliente();
            var result = client.PostAsJsonAsync(UrlAPI + "Seguridad/AddEducacion", modelo).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Perfil");
        }

        [HttpPost]
        public IActionResult DeleteEducacion(int educacionId)
        {
            using var client = CrearCliente();
            var result = client.PostAsJsonAsync(UrlAPI + $"Seguridad/DeleteEducacion?educacionId={educacionId}", new { }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Perfil");
        }

        #endregion

        #region Experiencia

        [HttpPost]
        public IActionResult AddExperiencia(ExperienciaViewModel modelo)
        {
            using var client = CrearCliente();
            var result = client.PostAsJsonAsync(UrlAPI + "Seguridad/AddExperiencia", modelo).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Perfil");
        }

        [HttpPost]
        public IActionResult DeleteExperiencia(int experienciaId)
        {
            using var client = CrearCliente();
            var result = client.PostAsJsonAsync(UrlAPI + $"Seguridad/DeleteExperiencia?experienciaId={experienciaId}", new { }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Perfil");
        }

        #endregion
    }
}