using Application.DTOs;
using Audit.Attributes;
using Domain.Enums;

namespace Application.Interfaces;

public interface IProductService
{

    Task<ProductDto?> GetByIdAsync(Guid id);

    [AuditLog]
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<IEnumerable<ProductDto>> GetByStatusAsync(ProductStatus status);

    [AuditLog]
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto);
    Task<bool> DeleteAsync(Guid id);
}
