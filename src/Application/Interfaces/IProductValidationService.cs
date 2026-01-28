using Audit.Attributes;

namespace Application.Interfaces;

public interface IProductValidationService
{
    [AuditLog(Category = "Job")]
    Task<string> ValidateProductAsync(Guid productId);
}
