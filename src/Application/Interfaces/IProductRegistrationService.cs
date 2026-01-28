using Application.DTOs;
using Audit.Attributes;

namespace Application.Interfaces;

public interface IProductRegistrationService
{
    [AuditLog(Category = "Job")]
    Task<string> ProcessProductRegistrationAsync(QueueMessage message);
}
