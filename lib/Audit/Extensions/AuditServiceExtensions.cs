namespace Audit.Extensions;

using Audit.Interfaces;
using Audit.Services;
using Microsoft.Extensions.DependencyInjection;

public static class AuditServiceExtensions
{
    /// <summary>
    /// Adiciona os serviços de auditoria para projetos API
    /// Inclui TraceContext e AuditLogService
    /// </summary>
    public static IServiceCollection AddAuditForApi(this IServiceCollection services)
    {
        services.AddScoped<ITraceContext, TraceContext>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        
        return services;
    }

    /// <summary>
    /// Adiciona os serviços de auditoria para projetos Jobs/Workers
    /// Inclui TraceContext e AuditLogService
    /// </summary>
    public static IServiceCollection AddAuditForJobs(this IServiceCollection services)
    {
        services.AddScoped<ITraceContext, TraceContext>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        
        return services;
    }
}
