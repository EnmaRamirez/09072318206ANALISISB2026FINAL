using Microsoft.AspNetCore.Mvc;
using NetGuardGT.Api.Services;

namespace NetGuardGT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IncidentService _service;

    public ReportsController(IncidentService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> Summary(CancellationToken ct)
    {
        return Ok(await _service.GetReportsAsync(ct));
    }
}
