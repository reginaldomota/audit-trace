using Audit.Data;
using Audit.Middleware;
using Audit.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Audit.Extensions;

public static class AuditMiddlewareExtensions
{
    public static IApplicationBuilder UseAudit(this IApplicationBuilder builder)
    {
        // Aplica migrations automaticamente apenas se houver pendentes
        using (var scope = builder.ApplicationServices.CreateScope())
        {
            var auditDbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuditDbContext>>();
            
            try
            {
                var pendingMigrations = auditDbContext.Database.GetPendingMigrations();
                
                if (pendingMigrations.Any())
                {
                    auditDbContext.Database.Migrate();
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "42P07") // 42P07 = relation already exists
            {
                // Tabela já existe, ignora o erro
                logger.LogWarning("Audit table already exists. Skipping migration.");
            }
        }

        return builder.UseMiddleware<AuditMiddleware>();
    }
}
