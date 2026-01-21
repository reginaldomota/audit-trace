namespace Audit.Services;

using Audit.Configuration;
using Audit.Interfaces;
using Audit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

public class AuditLogService : IAuditLogService
{
    private readonly ILogger<AuditLogService> _logger;
    private readonly IAuditRepository _auditRepository;
    private readonly string _applicationName;

    public AuditLogService(
        ILogger<AuditLogService> logger,
        IAuditRepository auditRepository,
        IOptions<AuditOptions> options)
    {
        _logger = logger;
        _auditRepository = auditRepository;
        
        // Se não configurado, usa o nome da assembly principal
        _applicationName = string.IsNullOrWhiteSpace(options.Value.ApplicationName) || options.Value.ApplicationName == "UnknownApp"
            ? Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp"
            : options.Value.ApplicationName;
    }

    public async Task LogAsync(AuditLog auditLog)
    {
        auditLog.ApplicationName = _applicationName;

        _logger.LogInformation(
            "[AUDIT] App: {ApplicationName} | TraceId: {TraceId} | Category: {Category} | {Method} {Operation} - Status: {StatusCode} - Duration: {Duration}ms - IP: {IpAddress}",
            auditLog.ApplicationName,
            auditLog.TraceId,
            auditLog.Category,
            auditLog.Method,
            auditLog.Operation,
            auditLog.StatusCode,
            auditLog.DurationMs,
            auditLog.IpAddress);

        await _auditRepository.AddAsync(auditLog);
    }

    public async Task<IEnumerable<AuditLog>> GetByTraceIdAsync(string traceId)
    {
        return await _auditRepository.GetByTraceIdAsync(traceId);
    }
}
