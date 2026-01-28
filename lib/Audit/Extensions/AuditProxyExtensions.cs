using Audit.Attributes;
using Audit.Interfaces;
using Audit.Proxies;
using Audit.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Audit.Extensions;

public static class AuditProxyExtensions
{
    /// <summary>
    /// Automaticamente envolve todos os serviços registrados que possuem métodos com [AuditLog] em proxies.
    /// Chame APÓS registrar todos os seus serviços.
    /// </summary>
    public static IServiceCollection EnableAuditProxies(this IServiceCollection services)
    {
        var descriptors = services.ToList();
        
        foreach (var descriptor in descriptors)
        {
            // Ignora serviços sem tipo de serviço (edge cases)
            if (descriptor.ServiceType == null)
                continue;
                
            // Ignora serviços do próprio Audit para evitar loops
            if (descriptor.ServiceType.Namespace?.StartsWith("Audit") == true)
                continue;
                
            // Ignora serviços de infraestrutura do ASP.NET
            if (descriptor.ServiceType.Namespace?.StartsWith("Microsoft") == true)
                continue;
            if (descriptor.ServiceType.Namespace?.StartsWith("System") == true)
                continue;
                
            // Só processa interfaces
            if (!descriptor.ServiceType.IsInterface)
                continue;

            // Verifica se a implementação existe
            var implementationType = descriptor.ImplementationType;
            if (implementationType == null)
                continue;

            // Verifica se a INTERFACE tem métodos com [AuditLog]
            var hasAuditLogMethods = descriptor.ServiceType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Any(m => m.GetCustomAttribute<AuditLogAttribute>() != null);

            if (!hasAuditLogMethods)
                continue;

            // Remove o descritor original
            services.Remove(descriptor);

            // Registra a implementação concreta
            services.Add(new ServiceDescriptor(
                implementationType,
                implementationType,
                descriptor.Lifetime));

            // Registra o proxy na interface
            services.Add(new ServiceDescriptor(
                descriptor.ServiceType,
                provider =>
                {
                    var implementation = provider.GetRequiredService(implementationType);
                    var auditQueue = provider.GetRequiredService<IAuditQueue>();
                    var auditLogService = provider.GetRequiredService<IAuditLogService>();
                    var traceContext = provider.GetRequiredService<ITraceContext>();
                    var httpContextAccessor = provider.GetService<IHttpContextAccessor>();

                    var proxyType = typeof(AuditProxy<>).MakeGenericType(descriptor.ServiceType);
                    var createMethod = proxyType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                    
                    return createMethod!.Invoke(null, new[] { implementation, auditQueue, auditLogService, traceContext, httpContextAccessor })!;
                },
                descriptor.Lifetime));
        }

        return services;
    }
}
