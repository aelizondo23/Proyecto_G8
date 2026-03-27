using FieldTechWeb.Filters;
using FieldTechWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace FieldTechWeb.Controllers
{
    [ValidarSesion]
    public class OrdenController(IHttpClientFactory _http, IConfiguration _config) : Controller
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

        #region Dashboard

        [HttpGet]
        public IActionResult Dashboard()
        {
            using var client = CrearCliente();

            if (TipoUsuario == "TECH")
            {
                var url = UrlAPI + "Orden/MisAsignaciones";
                var result = client.GetAsync(url).Result;

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var datos = result.Content.ReadFromJsonAsync<List<Asignacion>>().Result ?? new();
                    return View("DashboardTecnico", datos);
                }
                else if (result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new Exception();
                }

                return View("DashboardTecnico", new List<Asignacion>());
            }
            else
            {
                var url = UrlAPI + "Orden/ListarOrdenes?tamano=50";
                var result = client.GetAsync(url).Result;

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var datos = result.Content.ReadFromJsonAsync<List<Orden>>().Result ?? new();
                    return View("DashboardCliente", datos);
                }
                else if (result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new Exception();
                }

                return View("DashboardCliente", new List<Orden>());
            }
        }

        #endregion

        #region Marketplace

        [HttpGet]
        public IActionResult Marketplace(string? categoria, string? urgencia, string? zona)
        {
            using var client = CrearCliente();
            var url = UrlAPI + $"Orden/ListarOrdenes?status=OPEN&soloDisponibles=true&categoria={categoria}&urgencia={urgencia}&zona={zona}";
            var result = client.GetAsync(url).Result;

            

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var datos = result.Content.ReadFromJsonAsync<List<Orden>>().Result ?? new();
                return View(datos);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            return View(new List<Orden>());
        }

        #endregion

        #region Mis Órdenes

        [HttpGet]
        public IActionResult MisOrdenes()
        {
            using var client = CrearCliente();
            var url = UrlAPI + "Orden/ListarOrdenes?tamano=100";
            var result = client.GetAsync(url).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var datos = result.Content.ReadFromJsonAsync<List<Orden>>().Result ?? new();
                return View(datos);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            return View(new List<Orden>());
        }

        #endregion

        #region Detalle Orden

        [HttpGet]
        public IActionResult Detalle(int id)
        {
            using var client = CrearCliente();

            var resultOrden = client.GetAsync(UrlAPI + $"Orden/ConsultarOrden?id={id}").Result;
            if (resultOrden.StatusCode != HttpStatusCode.OK) return RedirectToAction("Dashboard");

            var vm = new DetalleOrdenViewModel
            {
                Orden = resultOrden.Content.ReadFromJsonAsync<Orden>().Result!,
                Asignaciones = client.GetAsync(UrlAPI + $"Orden/ConsultarAsignaciones?ordenId={id}").Result
                    .Content.ReadFromJsonAsync<List<Asignacion>>().Result ?? new(),
                CheckIns = client.GetAsync(UrlAPI + $"Orden/ConsultarCheckIns?ordenId={id}").Result
                    .Content.ReadFromJsonAsync<List<CheckIn>>().Result ?? new(),
                Mensajes = client.GetAsync(UrlAPI + $"Orden/ConsultarMensajes?ordenId={id}").Result
                    .Content.ReadFromJsonAsync<List<Mensaje>>().Result ?? new()
            };

            return View(vm);
        }

        #endregion

        #region Crear Orden

        [HttpGet]
        public IActionResult CrearOrden()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CrearOrden(Orden modelo)
        {
            using var client = CrearCliente();
            var url = UrlAPI + "Orden/CrearOrden";
            var result = client.PostAsJsonAsync(url, modelo).Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("MisOrdenes");
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception();
            }

            ViewBag.Mensaje = result.Content.ReadAsStringAsync().Result;
            return View(modelo);
        }

        #endregion

        #region Cancelar Orden

        [HttpPost]
        public IActionResult CancelarOrden(int id)
        {
            using var client = CrearCliente();
            var url = UrlAPI + $"Orden/CancelarOrden?id={id}";
            var result = client.PutAsJsonAsync(url, new { }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("MisOrdenes");
        }

        #endregion

        #region Mensajes

        [HttpPost]
        public IActionResult EnviarMensaje(int ordenId, string cuerpo)
        {
            using var client = CrearCliente();
            var url = UrlAPI + $"Orden/EnviarMensaje?ordenId={ordenId}";
            var result = client.PostAsJsonAsync(url, new { Cuerpo = cuerpo }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Detalle", new { id = ordenId });
        }

        #endregion

        #region Check-In / Check-Out

        [HttpPost]
        public IActionResult CheckIn(int ordenId, string? notas)
        {
            using var client = CrearCliente();
            var url = UrlAPI + $"Orden/CheckIn?ordenId={ordenId}";
            var result = client.PostAsJsonAsync(url, new { Notas = notas }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Detalle", new { id = ordenId });
        }

        [HttpPost]
        public IActionResult CheckOut(int ordenId, int checkInId, string? notas)
        {
            using var client = CrearCliente();
            var url = UrlAPI + $"Orden/CheckOut?checkInId={checkInId}";
            var result = client.PutAsJsonAsync(url, new { Notas = notas }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Detalle", new { id = ordenId });
        }

        #endregion

        #region Asignaciones

        [HttpPost]
        public IActionResult AplicarOrden(int ordenId)
        {
            using var client = CrearCliente();
            var url = UrlAPI + $"Orden/AplicarOrden?ordenId={ordenId}";
            var result = client.PostAsJsonAsync(url, new { }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Dashboard");
        }


        [HttpPost]
        public IActionResult ResponderAsignacion(int asignacionId, bool aceptar, int ordenId)
        {
            using var client = CrearCliente();
            var url = UrlAPI + $"Orden/ResponderAsignacion?asignacionId={asignacionId}";
            var result = client.PutAsJsonAsync(url, new { Aceptar = aceptar }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Detalle", new { id = ordenId });
        }

        [HttpPost]
        public IActionResult CompletarAsignacion(int asignacionId, int ordenId)
        {
            using var client = CrearCliente();
            var url = UrlAPI + $"Orden/CompletarAsignacion?asignacionId={asignacionId}";
            var result = client.PutAsJsonAsync(url, new { }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Detalle", new { id = ordenId });
        }

        [HttpPost]
        public IActionResult AsignarTecnico(int ordenId, int tecnicoId, decimal? montoAcordado)
        {
            using var client = CrearCliente();
            var url = UrlAPI + $"Orden/AsignarTecnico?ordenId={ordenId}";
            var result = client.PostAsJsonAsync(url, new { TechnicianUserId = tecnicoId, MontoAcordado = montoAcordado }).Result;

            if (result.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception();

            return RedirectToAction("Detalle", new { id = ordenId });
        }

        #endregion
    }
}
