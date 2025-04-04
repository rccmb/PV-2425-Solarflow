using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SolarflowServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HubController(ApplicationDbContext context) : ControllerBase
{
}