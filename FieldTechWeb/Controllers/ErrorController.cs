using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FieldTechWeb.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult CapturarError()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            return View("Error");
        }
    }
}
