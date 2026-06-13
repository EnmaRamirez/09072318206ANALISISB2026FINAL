using Microsoft.AspNetCore.Mvc;

namespace NetGuardGT.Api.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Redirect("/swagger");
    }
}
