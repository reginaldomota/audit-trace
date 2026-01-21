namespace Audit.Interfaces;

using Audit.Models;

public interface IAuditLogService
{
    Task LogAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetByTraceIdAsync(string traceId);
}
