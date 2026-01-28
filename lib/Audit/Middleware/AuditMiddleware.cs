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
using System.Text.Json;

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
        var ctx = (TraceContext)traceContext;
        ctx.TraceId = traceId;
        ctx.Category = AuditCategory.HTTP;
        ctx.Method = context.Request.Method;
        
        // Adiciona o traceId no response header
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Trace-Id"] = traceId;
            return Task.CompletedTask;
        });

        var loggedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        
        // Captura o request body
        var requestBody = await ReadRequestBodyAsync(context.Request);
        
        // Captura o response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        Exception? exception = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            
            // Se ocorreu uma exceção e o response ainda não foi iniciado,
            // configura o status code para 500
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                
                var errorResponse = JsonSerializer.Serialize(new
                {
                    error = "Internal Server Error",
                    message = ex.Message,
                    traceId = traceId
                });
                
                var errorBytes = Encoding.UTF8.GetBytes(errorResponse);
                await responseBody.WriteAsync(errorBytes, 0, errorBytes.Length);
            }
        }
        finally
        {
            stopwatch.Stop();
            
            var responseBodyText = await ReadResponseBodyAsync(context.Response);
            
            // Cria objeto completo com Request e Response para Metadata
            var metadata = new
            {
                Request = new
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path.ToString(),
                    QueryString = context.Request.QueryString.ToString(),
                    Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                    Body = requestBody,
                    ContentType = context.Request.ContentType,
                    ContentLength = context.Request.ContentLength,
                    Protocol = context.Request.Protocol,
                    Scheme = context.Request.Scheme,
                    Host = context.Request.Host.ToString()
                },
                Response = new
                {
                    StatusCode = context.Response.StatusCode,
                    StatusDescription = HttpStatusCodes.GetDescription(context.Response.StatusCode),
                    Headers = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                    Body = responseBodyText,
                    ContentType = context.Response.ContentType,
                    ContentLength = context.Response.ContentLength
                },
                Exception = exception != null ? new
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    Type = exception.GetType().FullName
                } : null
            };
            
            var auditLog = new AuditLog
            {
                TraceId = traceId,
                LoggedAt = loggedAt,
                Category = AuditCategory.HTTP,
                Operation = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                StatusCodeDescription = HttpStatusCodes.GetDescription(context.Response.StatusCode),
                HasError = HttpStatusCodes.IsError(context.Response.StatusCode),
                DurationMs = stopwatch.ElapsedMilliseconds,
                InputData = SerializeToJson(requestBody),
                OutputData = SerializeToJson(responseBodyText),
                Metadata = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = false }),
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

    private string SerializeToJson(string? data)
    {
        if (string.IsNullOrEmpty(data))
            return JsonSerializer.Serialize(new { });

        // Tenta verificar se já é um JSON válido
        try
        {
            using var doc = JsonDocument.Parse(data);
            // Se parseou com sucesso, já é JSON válido
            return data;
        }
        catch
        {
            // Se não é JSON, encapsula em um objeto
            return JsonSerializer.Serialize(new { raw = data });
        }
    }
}