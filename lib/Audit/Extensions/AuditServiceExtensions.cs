namespace Audit.Extensions;

using Audit.Interfaces;
using Audit.Services;
using Microsoft.Extensions.DependencyInjection;

public static class AuditServiceExtensions
{
    /// <summary>
    /// Adiciona os serviços de auditoria para projetos API
    /// Inclui TraceContext, AuditQueue (fila em memória), AuditLogService e AuditLogProcessor (BackgroundService)
    /// </summary>
    public static IServiceCollection AddAuditForApi(this IServiceCollection services)
    {
        services.AddScoped<ITraceContext, TraceContext>();
        services.AddSingleton<IAuditQueue, AuditQueue>();
        services.AddSingleton<IAuditLogService, AuditLogService>();
        services.AddHostedService<AuditLogProcessor>();
        
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
