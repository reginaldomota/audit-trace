using Audit.Attributes;
using Audit.Constants;
using Audit.Enums;
using Audit.Helpers;
using Audit.Interfaces;
using Audit.Models;
using Audit.Services;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Audit.Proxies;

public class AuditProxy<T> : DispatchProxy where T : class
{
    private T? _target;
    private IAuditQueue? _auditQueue;
    private IAuditLogService? _auditLogService;
    private ITraceContext? _traceContext;
    private IHttpContextAccessor? _httpContextAccessor;

    public static T Create(T target, IAuditQueue auditQueue, IAuditLogService auditLogService, ITraceContext traceContext, IHttpContextAccessor? httpContextAccessor = null)
    {
        var proxy = Create<T, AuditProxy<T>>() as AuditProxy<T>;
        if (proxy == null)
            throw new InvalidOperationException("Failed to create proxy");
            
        proxy._target = target;
        proxy._auditQueue = auditQueue;
        proxy._auditLogService = auditLogService;
        proxy._traceContext = traceContext;
        proxy._httpContextAccessor = httpContextAccessor;
        
        return (proxy as T)!;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null || _target == null)
            return null;

        // Verifica se o método tem o atributo [AuditLog]
        var auditAttribute = targetMethod.GetCustomAttribute<AuditLogAttribute>();
        
        if (auditAttribute == null)
        {
            // Se não tem atributo, apenas executa o método
            return targetMethod.Invoke(_target, args);
        }

        // Captura informações antes da execução
        var stopwatch = Stopwatch.StartNew();
        var inputData = args != null && args.Length > 0 
            ? JsonSerializer.Serialize(new { arguments = args }) 
            : null;

        object? result = null;
        Exception? exception = null;

        try
        {
            // Executa o método
            result = targetMethod.Invoke(_target, args);
            
            // Se é Task ou Task<T>, intercepta a conclusão para auditoria
            if (result is Task task)
            {
                var taskType = task.GetType();
                
                // Se é Task<T>, usa método genérico
                if (taskType.IsGenericType)
                {
                    var resultType = taskType.GetGenericArguments()[0];
                    var method = typeof(AuditProxy<T>).GetMethod(nameof(InterceptTaskWithResultAsync), BindingFlags.NonPublic | BindingFlags.Instance);
                    var genericMethod = method!.MakeGenericMethod(resultType);
                    return (genericMethod.Invoke(this, new object[] { task, targetMethod, args!, inputData!, stopwatch }) ?? Task.CompletedTask)!;
                }
                
                // Se é Task (sem resultado), usa método sem genérico
                return InterceptTaskAsync(task, targetMethod, args, inputData, stopwatch);
            }
            
            // Para métodos síncronos, faz auditoria normal
            LogAudit(targetMethod, args, inputData, result, null, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            exception = ex;
            LogAudit(targetMethod, args, inputData, null, ex, stopwatch.ElapsedMilliseconds);
            throw;
        }

        return result;
    }

    private async Task InterceptTaskAsync(Task task, MethodInfo targetMethod, object?[]? args, string? inputData, Stopwatch stopwatch)
    {
        try
        {
            await task.ConfigureAwait(false);
            LogAudit(targetMethod, args, inputData, null, null, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            LogAudit(targetMethod, args, inputData, null, ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<TResult> InterceptTaskWithResultAsync<TResult>(Task<TResult> task, MethodInfo targetMethod, object?[]? args, string? inputData, Stopwatch stopwatch)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            LogAudit(targetMethod, args, inputData, result, null, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            LogAudit(targetMethod, args, inputData, null, ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void LogAudit(MethodInfo targetMethod, object?[]? args, string? inputData, object? result, Exception? exception, long durationMs)
    {
        var auditAttribute = targetMethod.GetCustomAttribute<AuditLogAttribute>();
        if (auditAttribute == null) return;

        int statusCode = exception == null ? JobStatusCodes.Success : JobStatusCodes.Failed;
        string statusDescription = JobStatusCodes.GetDescription(statusCode);

        // Captura informações do HttpContext se disponível
        var httpContext = _httpContextAccessor?.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userId = httpContext?.User?.Identity?.Name;
        
        // Se não há HttpContext (Jobs), tenta obter o IP local do servidor
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = GetLocalIpAddress();
        }

        // Cria o log de auditoria (ApplicationName será definido pelo AuditLogService)
        var auditLog = new AuditLog
        {
            TraceId = !string.IsNullOrEmpty(_traceContext?.TraceId) ? _traceContext.TraceId : TraceIdGenerator.NewTraceId(),
            LoggedAt = DateTime.UtcNow,
            Category = _traceContext?.Category ?? 
                (!string.IsNullOrEmpty(auditAttribute?.Category) 
                    ? Enum.Parse<AuditCategory>(auditAttribute.Category) 
                    : AuditCategory.Job),
            Operation = auditAttribute?.Operation ?? $"{_target?.GetType().FullName}.{targetMethod?.Name}",
            Method = _traceContext?.Method ?? JobMethods.Triggered,
            StatusCode = statusCode,
            StatusCodeDescription = statusDescription,
            HasError = JobStatusCodes.IsError(statusCode),
            DurationMs = durationMs,
            InputData = inputData ?? JsonSerializer.Serialize(new { }),
            OutputData = SerializeResultToJson(result),
            Metadata = exception != null 
                ? JsonSerializer.Serialize(new 
                { 
                    HasError = true,
                    Error = new
                    {
                        Message = exception.Message,
                        StackTrace = exception.StackTrace,
                        Type = exception.GetType().FullName
                    }
                }) 
                : JsonSerializer.Serialize(new { HasError = false }),
            IpAddress = ipAddress,
            UserId = userId
        };

        // Enfileira o log de forma assíncrona
        _ = _auditQueue?.EnqueueAsync(auditLog);
    }

    private string SerializeResultToJson(object? result)
    {
        if (result == null)
            return JsonSerializer.Serialize(new { });

        // Se for string primitiva, encapsula em um objeto
        if (result is string str)
            return JsonSerializer.Serialize(new { result = str });

        // Se for tipo primitivo (int, bool, etc), encapsula em um objeto
        var type = result.GetType();
        if (type.IsPrimitive || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid))
            return JsonSerializer.Serialize(new { result });

        // Para objetos complexos, serializa normalmente
        return JsonSerializer.Serialize(result);
    }

    private string? GetLocalIpAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var ipAddress = host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            return ipAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }
}
