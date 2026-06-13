using Microsoft.EntityFrameworkCore;
using NetGuardGT.Api.Models;

namespace NetGuardGT.Api.Data;

public class NetGuardDbContext : DbContext
{
    public NetGuardDbContext(DbContextOptions<NetGuardDbContext> options) : base(options)
    {
    }

    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentStatusHistory> IncidentStatusHistories => Set<IncidentStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Technician>().HasData(
            new Technician { Id = 1, Name = "Carlos Mendez", Specialty = TechnicianSpecialty.FiberOptic, IsActive = true },
            new Technician { Id = 2, Name = "Ana Ruiz", Specialty = TechnicianSpecialty.Microwave, IsActive = true },
            new Technician { Id = 3, Name = "Luis Torres", Specialty = TechnicianSpecialty.Electrical, IsActive = true },
            new Technician { Id = 4, Name = "Sofía López", Specialty = TechnicianSpecialty.General, IsActive = true }
        );

        modelBuilder.Entity<Incident>()
            .HasOne(i => i.AssignedTechnician)
            .WithMany()
            .HasForeignKey(i => i.AssignedTechnicianId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<IncidentStatusHistory>()
            .HasOne(h => h.Incident)
            .WithMany()
            .HasForeignKey(h => h.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
