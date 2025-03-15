using Microsoft.AspNetCore.Mvc;

namespace SolarflowClient.Controllers
{
    public class SuggestionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
