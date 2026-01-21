using Audit.Data;
using Audit.Middleware;
using Audit.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Extensions;

public static class AuditMiddlewareExtensions
{
    public static IApplicationBuilder UseAudit(this IApplicationBuilder builder)
    {
        // Aplica migrations automaticamente
        using (var scope = builder.ApplicationServices.CreateScope())
        {
            var auditDbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
            auditDbContext.Database.Migrate();
        }

        return builder.UseMiddleware<AuditMiddleware>();
    }
}
