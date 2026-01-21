namespace Audit.Interfaces;

using Audit.Models;

public interface IAuditQueue
{
    ValueTask EnqueueAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    ValueTask<AuditLog?> DequeueAsync(CancellationToken cancellationToken = default);
}
