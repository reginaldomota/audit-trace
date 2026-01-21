using Audit.Constants;
using Audit.Enums;
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

    public async Task InvokeAsync(HttpContext context, ITraceContext traceContext, IAuditQueue auditQueue)
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
                LoggedAt = DateTime.UtcNow,
                Category = AuditCategory.HTTP,
                Operation = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                StatusCodeDescription = GetHttpStatusDescription(context.Response.StatusCode),
                DurationMs = stopwatch.ElapsedMilliseconds,
                InputData = requestBody,
                OutputData = await ReadResponseBodyAsync(context.Response),
                UserId = context.User?.Identity?.Name,
                IpAddress = context.Connection.RemoteIpAddress?.ToString()
            };

            // Fire-and-forget: enfileira sem bloquear a resposta
            _ = auditQueue.EnqueueAsync(auditLog);

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

    private string GetHttpStatusDescription(int statusCode)
    {
        return statusCode switch
        {
            200 => "OK",
            201 => "Created",
            202 => "Accepted",
            204 => "No Content",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => statusCode.ToString()
        };
    }

}