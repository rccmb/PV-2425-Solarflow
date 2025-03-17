using Microsoft.AspNetCore.Mvc;

namespace SolarflowClient.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
