namespace Audit.Services;

using Audit.Interfaces;
using Audit.Models;
using Microsoft.Extensions.Logging;

public class AuditLogService : IAuditLogService
{
    private readonly ILogger<AuditLogService> _logger;
    private static readonly List<AuditLog> _auditLogs = new(); // Em memória para exemplo

    public AuditLogService(ILogger<AuditLogService> logger)
    {
        _logger = logger;
    }

    public Task LogAsync(AuditLog auditLog)
    {
        _logger.LogInformation(
            "[AUDIT] TraceId: {TraceId} | {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms - IP: {IpAddress}",
            auditLog.TraceId,
            auditLog.Method,
            auditLog.Path,
            auditLog.StatusCode,
            auditLog.Duration,
            auditLog.IpAddress);

        // Armazena em memória (pode ser substituído por persistência em banco)
        _auditLogs.Add(auditLog);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<AuditLog>> GetByTraceIdAsync(string traceId)
    {
        var logs = _auditLogs.Where(x => x.TraceId == traceId).ToList();
        return Task.FromResult<IEnumerable<AuditLog>>(logs);
    }
}
