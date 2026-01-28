namespace Audit.Extensions;

using Audit.Configuration;
using Audit.Data;
using Audit.Interfaces;
using Audit.Repositories;
using Audit.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class AuditServiceExtensions
{
    /// <summary>
    /// Adiciona os serviços de auditoria para projetos API
    /// Inclui TraceContext, AuditQueue (fila em memória), AuditLogService e AuditLogProcessor (BackgroundService)
    /// </summary>
    public static IServiceCollection AddAuditForApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurações
        services.Configure<AuditOptions>(configuration.GetSection("Audit"));

        // Database Context
        var connectionString = configuration.GetConnectionString("AuditConnection") 
            ?? configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(connectionString));

        // HttpContextAccessor para capturar IpAddress e UserId
        services.AddHttpContextAccessor();

        // Serviços
        services.AddScoped<ITraceContext, TraceContext>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddSingleton<IAuditQueue, AuditQueue>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddHostedService<AuditLogProcessor>();
        
        return services;
    }

    /// <summary>
    /// Adiciona os serviços de auditoria para projetos Jobs/Workers
    /// Inclui TraceContext e AuditLogService
    /// </summary>
    public static IServiceCollection AddAuditForJobs(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurações
        services.Configure<AuditOptions>(configuration.GetSection("Audit"));

        // Database Context
        var connectionString = configuration.GetConnectionString("AuditConnection") 
            ?? configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(connectionString));

        // HttpContextAccessor (opcional em Jobs, mas útil se houver)
        services.AddHttpContextAccessor();

        // Serviços
        services.AddScoped<ITraceContext, TraceContext>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddSingleton<IAuditQueue, AuditQueue>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddHostedService<AuditLogProcessor>();
        
        return services;
    }
}
