using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByStatusAsync(ProductStatus status);
    Task<Product?> GetByNameAsync(string name);
    Task UpdateStatusAsync(Guid id, ProductStatus status);
}
