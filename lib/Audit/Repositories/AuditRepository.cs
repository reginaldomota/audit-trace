namespace Audit.Repositories;

using Audit.Data;
using Audit.Interfaces;
using Audit.Models;
using Microsoft.EntityFrameworkCore;

public class AuditRepository : IAuditRepository
{
    private readonly AuditDbContext _context;

    public AuditRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByTraceIdAsync(string traceId)
    {
        return await _context.AuditLogs
            .Where(a => a.TraceId == traceId)
            .OrderBy(a => a.LoggedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByApplicationNameAsync(string applicationName, int pageNumber = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(a => a.ApplicationName == applicationName)
            .OrderByDescending(a => a.LoggedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
