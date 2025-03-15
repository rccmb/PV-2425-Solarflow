using Microsoft.AspNetCore.Mvc;

namespace SolarflowClient.Controllers
{
    public class BatteryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
