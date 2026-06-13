using Microsoft.EntityFrameworkCore;
using NetGuardGT.Api.Data;
using NetGuardGT.Api.Models;
using NetGuardGT.Api.Services;

namespace NetGuardGT.Tests;

public class IncidentServiceTests
{
    private static NetGuardDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NetGuardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NetGuardDbContext(options);
    }

    [Fact]
    public async Task Assigning_MoreThanThreeActiveIncidents_ShouldFail()
    {
        using var context = CreateContext();
        context.Technicians.Add(new Technician { Id = 10, Name = "T1", Specialty = TechnicianSpecialty.General });
        context.Incidents.AddRange(
            new Incident { Id = 1, Title = "A", Description = "", Type = IncidentType.General, Severity = IncidentSeverity.Low, Status = IncidentStatus.Assigned, CreatedAt = DateTime.UtcNow.AddHours(-1), AssignedTechnicianId = 10 },
            new Incident { Id = 2, Title = "B", Description = "", Type = IncidentType.General, Severity = IncidentSeverity.Low, Status = IncidentStatus.InProgress, CreatedAt = DateTime.UtcNow.AddHours(-1), AssignedTechnicianId = 10 },
            new Incident { Id = 3, Title = "C", Description = "", Type = IncidentType.General, Severity = IncidentSeverity.Low, Status = IncidentStatus.Assigned, CreatedAt = DateTime.UtcNow.AddHours(-1), AssignedTechnicianId = 10 }
        );
        await context.SaveChangesAsync();

        var service = new IncidentService(context);
        var incident = new Incident { Id = 4, Title = "D", Description = "", Type = IncidentType.General, Severity = IncidentSeverity.Low, Status = IncidentStatus.Registered, CreatedAt = DateTime.UtcNow };
        await service.CreateIncidentAsync(incident);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AssignIncidentAsync(4, 10));
    }

    [Fact]
    public async Task InvalidTransition_ShouldBeRejected()
    {
        using var context = CreateContext();
        var incident = new Incident { Id = 5, Title = "Estado", Description = "", Type = IncidentType.General, Severity = IncidentSeverity.Low, Status = IncidentStatus.Registered, CreatedAt = DateTime.UtcNow.AddHours(-2) };
        context.Incidents.Add(incident);
        await context.SaveChangesAsync();

        var service = new IncidentService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateStatusAsync(5, IncidentStatus.Closed));
    }

    [Fact]
    public async Task CriticalIncident_OverTwoHoursWithoutAttention_ShouldEscalateAutomatically()
    {
        using var context = CreateContext();
        var incident = new Incident
        {
            Id = 6,
            Title = "Crítico",
            Description = "",
            Type = IncidentType.FiberOptic,
            Severity = IncidentSeverity.Critical,
            Status = IncidentStatus.Registered,
            CreatedAt = DateTime.UtcNow.AddHours(-3)
        };
        context.Incidents.Add(incident);
        await context.SaveChangesAsync();

        var service = new IncidentService(context);
        await service.GetIncidentsAsync();

        var escalated = await context.Incidents.FindAsync(6);
        Assert.Equal(IncidentStatus.Escalated, escalated!.Status);
        Assert.True(escalated.Escalated);
    }

    [Fact]
    public async Task TechnicianWithoutMatchingSpecialty_ShouldNotBeAssigned()
    {
        using var context = CreateContext();
        context.Technicians.Add(new Technician { Id = 11, Name = "Fiber Tech", Specialty = TechnicianSpecialty.FiberOptic });
        context.Incidents.Add(new Incident { Id = 7, Title = "Fibra", Description = "", Type = IncidentType.Microwave, Severity = IncidentSeverity.High, Status = IncidentStatus.Registered, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new IncidentService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AssignIncidentAsync(7, 11));
    }
}
