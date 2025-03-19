using Microsoft.AspNetCore.Mvc;

namespace SolarflowClient.Controllers
{
    public class NotificationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
