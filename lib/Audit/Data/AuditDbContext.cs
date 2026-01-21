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
                .ValueGeneratedOnAdd()
                .HasColumnOrder(1);

            entity.Property(e => e.ApplicationName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnOrder(2);

            entity.Property(e => e.TraceId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnOrder(3);

            entity.Property(e => e.LoggedAt)
                .IsRequired()
                .HasColumnOrder(4);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasColumnOrder(5);

            entity.Property(e => e.Method)
                .HasMaxLength(50)
                .HasColumnOrder(6);

            entity.Property(e => e.Operation)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnOrder(7);

            entity.Property(e => e.StatusCodeDescription)
                .HasMaxLength(100)
                .HasColumnOrder(8);

            entity.Property(e => e.StatusCode)
                .HasColumnOrder(9);

            entity.Property(e => e.DurationMs)
                .IsRequired()
                .HasColumnOrder(10);

            entity.Property(e => e.InputData)
                .HasColumnType("text")
                .HasColumnOrder(11);

            entity.Property(e => e.OutputData)
                .HasColumnType("text")
                .HasColumnOrder(12);

            entity.Property(e => e.Metadata)
                .HasColumnType("text")
                .HasColumnOrder(13);

            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .HasColumnOrder(14);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnOrder(15);

            // Índices para melhorar performance de consultas
            entity.HasIndex(e => e.TraceId);
            entity.HasIndex(e => e.ApplicationName);
            entity.HasIndex(e => e.LoggedAt);
            entity.HasIndex(e => e.Category);
        });
    }
}
