using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SolarflowClient.Controllers
{
    public class SuggestionsController : Controller
    {
        public IActionResult Index()
        {
            var token = Request.Cookies["AuthToken"];

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Authentication");
            }

            return View();
        }
    }
}
