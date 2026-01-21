namespace Audit.Data;

using Audit.Models;
using Microsoft.EntityFrameworkCore;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.ApplicationName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TraceId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.LoggedAt)
                .IsRequired();

            entity.Property(e => e.Category)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.Method)
                .HasMaxLength(50);

            entity.Property(e => e.Operation)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.StatusCodeDescription)
                .HasMaxLength(100);

            entity.Property(e => e.StatusCode);

            entity.Property(e => e.DurationMs)
                .IsRequired();

            entity.Property(e => e.InputData)
                .HasColumnType("text");

            entity.Property(e => e.OutputData)
                .HasColumnType("text");

            entity.Property(e => e.Metadata)
                .HasColumnType("text");

            entity.Property(e => e.UserId)
                .HasMaxLength(100);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(50);

            // Índices para melhorar performance de consultas
            entity.HasIndex(e => e.TraceId);
            entity.HasIndex(e => e.ApplicationName);
            entity.HasIndex(e => e.LoggedAt);
            entity.HasIndex(e => e.Category);
        });
    }
}
