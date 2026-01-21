using Audit.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Audit.Factories;

public class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuditDbContext>();
        
        // Connection string padrão para migrations
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=audittrace;Username=postgres;Password=postgres");

        return new AuditDbContext(optionsBuilder.Options);
    }
}
