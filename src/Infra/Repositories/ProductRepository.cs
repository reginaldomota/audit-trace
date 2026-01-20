using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByStatusAsync(ProductStatus status)
    {
        return await _dbSet.Where(p => p.Status == status).ToListAsync();
    }

    public async Task<Product?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task UpdateStatusAsync(Guid id, ProductStatus status)
    {
        var product = await _dbSet.FindAsync(id);
        if (product != null)
        {
            product.Status = status;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
