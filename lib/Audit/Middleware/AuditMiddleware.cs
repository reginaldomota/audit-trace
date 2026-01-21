using Audit.Helpers;
using Audit.Interfaces;
using Audit.Models;
using Audit.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Audit.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITraceContext traceContext, IAuditLogService auditLogService)
    {
        // Verifica se o traceId foi passado no header
        var traceId = context.Request.Headers["traceId"].FirstOrDefault();
        
        // Se não foi passado, gera um novo GUID v7
        if (string.IsNullOrEmpty(traceId))
            traceId = TraceIdGenerator.NewTraceId();
        
        // Armazena no contexto para ser acessível em toda a aplicação
        ((TraceContext)traceContext).TraceId = traceId;
        
        // Adiciona o traceId no response header
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Trace-Id"] = traceId;
            return Task.CompletedTask;
        });

        var stopwatch = Stopwatch.StartNew();
        
        // Captura o request body
        var requestBody = await ReadRequestBodyAsync(context.Request);
        
        // Captura o response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            var auditLog = new AuditLog
            {
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                Method = context.Request.Method,
                Path = context.Request.Path,
                StatusCode = context.Response.StatusCode,
                Duration = stopwatch.ElapsedMilliseconds,
                RequestBody = requestBody,
                ResponseBody = await ReadResponseBodyAsync(context.Response),
                UserId = context.User?.Identity?.Name,
                IpAddress = context.Connection.RemoteIpAddress?.ToString()
            };

            await auditLogService.LogAsync(auditLog);

            // Copia o response de volta para o stream original
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.ContentLength.HasValue || request.ContentLength == 0)
            return null;

        request.EnableBuffering();
        
        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }

    private async Task<string?> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        
        return string.IsNullOrEmpty(text) ? null : text;
    }

}
