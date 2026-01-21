using Audit.Middleware;
using Audit.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Extensions;

public static class AuditMiddlewareExtensions
{
    public static IApplicationBuilder UseAudit(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuditMiddleware>();
    }
}
