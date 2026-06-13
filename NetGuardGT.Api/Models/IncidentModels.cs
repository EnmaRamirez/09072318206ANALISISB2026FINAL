namespace NetGuardGT.Api.Models;

public enum IncidentSeverity
{
    Low,
    Moderate,
    High,
    Urgent,
    Critical
}

public enum IncidentType
{
    FiberOptic,
    Microwave,
    Electrical,
    General
}

public enum IncidentStatus
{
    Registered,
    Assigned,
    InProgress,
    Resolved,
    Closed,
    Escalated
}

public enum TechnicianSpecialty
{
    FiberOptic,
    Microwave,
    Electrical,
    General
}

public class Technician
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TechnicianSpecialty Specialty { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Incident
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentType Type { get; set; }
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Registered;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastStatusChangedAt { get; set; }
    public DateTime? ResolutionDeadline { get; set; }
    public int? AssignedTechnicianId { get; set; }
    public Technician? AssignedTechnician { get; set; }
    public bool Escalated { get; set; }
    public DateTime? EscalatedAt { get; set; }
}

public class IncidentStatusHistory
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident? Incident { get; set; }
    public IncidentStatus PreviousStatus { get; set; }
    public IncidentStatus NewStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}

public class IncidentReportItem
{
    public int Total { get; set; }
    public int Open { get; set; }
    public int Closed { get; set; }
    public int Overdue { get; set; }
}
