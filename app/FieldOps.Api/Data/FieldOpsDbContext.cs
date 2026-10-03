using FieldOps.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Data;

public class FieldOpsDbContext : DbContext
{
    public FieldOpsDbContext(DbContextOptions<FieldOpsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Technician> Technicians => Set<Technician>();

    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();

    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceRequest>()
            .HasOne(x => x.Customer)
            .WithMany(x => x.ServiceRequests)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assignment>()
            .HasOne(x => x.ServiceRequest)
            .WithMany()
            .HasForeignKey(x => x.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assignment>()
            .HasOne(x => x.Technician)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assignment>()
            .HasIndex(x => new
            {
                x.ServiceRequestId,
                x.IsActive
            });

        modelBuilder.Entity<Assignment>()
            .HasIndex(x => new
            {
                x.TechnicianId,
                x.IsActive
            });

        modelBuilder.Entity<StatusHistory>()
    .HasOne<ServiceRequest>()
    .WithMany()
    .HasForeignKey(x => x.ServiceRequestId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<StatusHistory>()
    .HasIndex(x => new
    {
        x.ServiceRequestId,
        x.ChangedAtUtc
    });

    modelBuilder.Entity<Attachment>()
    .HasOne(x => x.ServiceRequest)
    .WithMany()
    .HasForeignKey(x => x.ServiceRequestId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Attachment>()
    .HasIndex(x => x.ServiceRequestId);
    }
}