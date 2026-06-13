using Microsoft.AspNetCore.Mvc;
using NetGuardGT.Api.Models;
using NetGuardGT.Api.Services;

namespace NetGuardGT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly IncidentService _service;

    public IncidentsController(IncidentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Incident>>> GetAll(CancellationToken ct)
    {
        return Ok(await _service.GetIncidentsAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Incident>> GetById(int id, CancellationToken ct)
    {
        var incident = await _service.GetIncidentAsync(id, ct);
        return incident is null ? NotFound() : Ok(incident);
    }

    [HttpPost]
    public async Task<ActionResult<Incident>> Create([FromBody] Incident request, CancellationToken ct)
    {
        try
        {
            var incident = await _service.CreateIncidentAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/assign")]
    public async Task<ActionResult<Incident>> Assign(int id, [FromQuery] int technicianId, CancellationToken ct)
    {
        try
        {
            return Ok(await _service.AssignIncidentAsync(id, technicianId, ct));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<Incident>> ChangeStatus(int id, [FromQuery] IncidentStatus status, CancellationToken ct)
    {
        try
        {
            return Ok(await _service.UpdateStatusAsync(id, status, ct));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/release")]
    public async Task<ActionResult<Incident>> Release(int id, CancellationToken ct)
    {
        try
        {
            return Ok(await _service.ReleaseIncidentAsync(id, ct));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<IEnumerable<IncidentStatusHistory>>> History(int id, CancellationToken ct)
    {
        return Ok(await _service.GetHistoryAsync(id, ct));
    }
}
