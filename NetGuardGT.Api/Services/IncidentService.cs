using Microsoft.EntityFrameworkCore;
using NetGuardGT.Api.Data;
using NetGuardGT.Api.Models;

namespace NetGuardGT.Api.Services;

public class IncidentService
{
    private readonly NetGuardDbContext _db;

    public IncidentService(NetGuardDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Incident>> GetIncidentsAsync(CancellationToken ct = default)
    {
        await AutoEscalatePendingIncidentsAsync(ct);
        await _db.SaveChangesAsync(ct);

        return await _db.Incidents
            .Include(i => i.AssignedTechnician)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Incident?> GetIncidentAsync(int id, CancellationToken ct = default)
    {
        await AutoEscalatePendingIncidentsAsync(ct);
        await _db.SaveChangesAsync(ct);

        return await _db.Incidents
            .Include(i => i.AssignedTechnician)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<Incident> CreateIncidentAsync(Incident incident, CancellationToken ct = default)
    {
        incident.CreatedAt = DateTime.UtcNow;
        incident.Status = IncidentStatus.Registered;
        incident.ResolutionDeadline = CalculateResolutionDeadline(incident.Severity, incident.CreatedAt);

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync(ct);

        _db.IncidentStatusHistories.Add(new IncidentStatusHistory
        {
            IncidentId = incident.Id,
            PreviousStatus = IncidentStatus.Registered,
            NewStatus = IncidentStatus.Registered,
            Notes = "Incidente registrado en el sistema",
            ChangedAt = incident.CreatedAt
        });

        await _db.SaveChangesAsync(ct);
        return incident;
    }

    public async Task<Incident> AssignIncidentAsync(int incidentId, int technicianId, CancellationToken ct = default)
    {
        await AutoEscalatePendingIncidentsAsync(ct);

        var incident = await _db.Incidents.Include(i => i.AssignedTechnician).FirstOrDefaultAsync(i => i.Id == incidentId, ct)
            ?? throw new InvalidOperationException("Incidente no encontrado.");

        var technician = await _db.Technicians.FindAsync(new object?[] { technicianId }, ct)
            ?? throw new InvalidOperationException("Técnico no encontrado.");

        if (incident.Status == IncidentStatus.Closed || incident.Status == IncidentStatus.Resolved)
            throw new InvalidOperationException("No se puede asignar un incidente ya cerrado o resuelto.");

        if (!CanHandleIncident(technician, incident))
            throw new InvalidOperationException("El técnico no posee la especialidad requerida para este incidente.");

        var activeCount = await _db.Incidents.CountAsync(i =>
            i.AssignedTechnicianId == technicianId &&
            i.Status != IncidentStatus.Resolved &&
            i.Status != IncidentStatus.Closed, ct);
        if (activeCount >= 3)
            throw new InvalidOperationException("El técnico ya tiene 3 incidentes activos.");

        var previousStatus = incident.Status;
        if (incident.Status == IncidentStatus.Registered || incident.Status == IncidentStatus.Escalated)
            incident.Status = IncidentStatus.Assigned;

        incident.AssignedTechnicianId = technicianId;
        incident.UpdatedAt = DateTime.UtcNow;
        incident.LastStatusChangedAt = DateTime.UtcNow;

        _db.IncidentStatusHistories.Add(new IncidentStatusHistory
        {
            IncidentId = incident.Id,
            PreviousStatus = previousStatus,
            NewStatus = incident.Status,
            Notes = $"Asignado a {technician.Name}",
            ChangedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return incident;
    }

    public async Task<Incident> UpdateStatusAsync(int incidentId, IncidentStatus newStatus, CancellationToken ct = default)
    {
        await AutoEscalatePendingIncidentsAsync(ct);

        var incident = await _db.Incidents.FirstOrDefaultAsync(i => i.Id == incidentId, ct)
            ?? throw new InvalidOperationException("Incidente no encontrado.");

        if (!IsValidTransition(incident.Status, newStatus))
            throw new InvalidOperationException("La transición de estado no es válida.");

        var previousStatus = incident.Status;
        incident.Status = newStatus;
        incident.UpdatedAt = DateTime.UtcNow;
        incident.LastStatusChangedAt = DateTime.UtcNow;

        if (newStatus == IncidentStatus.Resolved)
            incident.Escalated = false;

        if (newStatus == IncidentStatus.Closed)
            incident.Escalated = false;

        _db.IncidentStatusHistories.Add(new IncidentStatusHistory
        {
            IncidentId = incident.Id,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Notes = "Cambio de estado aplicado por la API",
            ChangedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return incident;
    }

    public async Task<Incident> ReleaseIncidentAsync(int incidentId, CancellationToken ct = default)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(i => i.Id == incidentId, ct)
            ?? throw new InvalidOperationException("Incidente no encontrado.");

        var previousStatus = incident.Status;
        incident.AssignedTechnicianId = null;
        incident.Status = IncidentStatus.Registered;
        incident.UpdatedAt = DateTime.UtcNow;
        incident.LastStatusChangedAt = DateTime.UtcNow;

        _db.IncidentStatusHistories.Add(new IncidentStatusHistory
        {
            IncidentId = incident.Id,
            PreviousStatus = previousStatus,
            NewStatus = IncidentStatus.Registered,
            Notes = "Incidente liberado por el técnico anterior",
            ChangedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return incident;
    }

    public async Task<IReadOnlyList<IncidentStatusHistory>> GetHistoryAsync(int incidentId, CancellationToken ct = default)
    {
        return await _db.IncidentStatusHistories
            .Where(h => h.IncidentId == incidentId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(ct);
    }

    public async Task<object> GetReportsAsync(CancellationToken ct = default)
    {
        await AutoEscalatePendingIncidentsAsync(ct);
        await _db.SaveChangesAsync(ct);

        var incidents = await _db.Incidents.ToListAsync(ct);

        var summary = new
        {
            total = incidents.Count,
            open = incidents.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed),
            closed = incidents.Count(i => i.Status is IncidentStatus.Resolved or IncidentStatus.Closed),
            escalated = incidents.Count(i => i.Status == IncidentStatus.Escalated),
            bySeverity = incidents.GroupBy(i => i.Severity).Select(g => new { severity = g.Key.ToString(), count = g.Count() }),
            byStatus = incidents.GroupBy(i => i.Status).Select(g => new { status = g.Key.ToString(), count = g.Count() }),
            overdue = incidents.Count(i => i.ResolutionDeadline < DateTime.UtcNow && i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed)
        };

        return summary;
    }

    private static bool IsValidTransition(IncidentStatus current, IncidentStatus next)
    {
        return current switch
        {
            IncidentStatus.Registered => next is IncidentStatus.Assigned or IncidentStatus.InProgress or IncidentStatus.Escalated,
            IncidentStatus.Escalated => next is IncidentStatus.Assigned or IncidentStatus.InProgress,
            IncidentStatus.Assigned => next is IncidentStatus.InProgress,
            IncidentStatus.InProgress => next is IncidentStatus.Resolved,
            IncidentStatus.Resolved => next is IncidentStatus.Closed,
            _ => false
        };
    }

    private static DateTime CalculateResolutionDeadline(IncidentSeverity severity, DateTime createdAt)
    {
        var hours = severity switch
        {
            IncidentSeverity.Critical => 2,
            IncidentSeverity.Urgent => 4,
            IncidentSeverity.High => 8,
            IncidentSeverity.Moderate => 24,
            _ => 48
        };

        return createdAt.AddHours(hours);
    }

    private static bool CanHandleIncident(Technician technician, Incident incident)
    {
        if (incident.Type == IncidentType.General)
            return true;

        return incident.Type switch
        {
            IncidentType.FiberOptic => technician.Specialty == TechnicianSpecialty.FiberOptic || technician.Specialty == TechnicianSpecialty.General,
            IncidentType.Microwave => technician.Specialty == TechnicianSpecialty.Microwave || technician.Specialty == TechnicianSpecialty.General,
            IncidentType.Electrical => technician.Specialty == TechnicianSpecialty.Electrical || technician.Specialty == TechnicianSpecialty.General,
            _ => true
        };
    }

    private async Task AutoEscalatePendingIncidentsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var pendingIncidents = await _db.Incidents
            .Where(i => i.Status == IncidentStatus.Registered &&
                        (i.Severity == IncidentSeverity.Critical || i.Severity == IncidentSeverity.Urgent))
            .ToListAsync(ct);

        foreach (var incident in pendingIncidents)
        {
            var hoursSinceCreated = (now - incident.CreatedAt).TotalHours;
            if (hoursSinceCreated > 2)
            {
                var previousStatus = incident.Status;
                incident.Status = IncidentStatus.Escalated;
                incident.Escalated = true;
                incident.EscalatedAt = now;
                incident.UpdatedAt = now;
                incident.LastStatusChangedAt = now;

                _db.IncidentStatusHistories.Add(new IncidentStatusHistory
                {
                    IncidentId = incident.Id,
                    PreviousStatus = previousStatus,
                    NewStatus = IncidentStatus.Escalated,
                    Notes = "Incidente escalado automáticamente por tiempo de espera superior a 2 horas.",
                    ChangedAt = now
                });
            }
        }
    }
}
