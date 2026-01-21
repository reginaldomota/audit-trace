namespace Audit.Interfaces;

using Audit.Models;

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetByTraceIdAsync(string traceId);
    Task<IEnumerable<AuditLog>> GetByApplicationNameAsync(string applicationName, int pageNumber = 1, int pageSize = 50);
}
