using Audit.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ITraceContext? _traceContext;

    public ProductRepository(ApplicationDbContext context, ITraceContext? traceContext = null) : base(context)
    {
        _traceContext = traceContext;
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
            
            // Atualiza o TraceId automaticamente se disponível
            if (_traceContext != null && !string.IsNullOrEmpty(_traceContext.TraceId))
                product.TraceId = _traceContext.TraceId;
            
            await _context.SaveChangesAsync();
        }
    }
}
