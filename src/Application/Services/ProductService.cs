using Application.DTOs;
using Application.Interfaces;
using Audit.Attributes;
using Audit.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ITraceContext _traceContext;

    public ProductService(IProductRepository productRepository, ITraceContext traceContext)
    {
        _productRepository = productRepository;
        _traceContext = traceContext;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetByStatusAsync(ProductStatus status)
    {
        var products = await _productRepository.GetByStatusAsync(status);
        return products.Select(MapToDto);
    }
    
    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        // Garante que o TraceId não está vazio
        var traceId = !string.IsNullOrEmpty(_traceContext.TraceId) 
            ? _traceContext.TraceId 
            : null;
            
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            TraceId = traceId // Salva o TraceId do contexto atual
        };

        var createdProduct = await _productRepository.AddAsync(product);
        return MapToDto(createdProduct);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        if (!string.IsNullOrEmpty(dto.Name))
            product.Name = dto.Name;
        
        if (!string.IsNullOrEmpty(dto.Description))
            product.Description = dto.Description;
        
        if (dto.Price.HasValue)
            product.Price = dto.Price.Value;
        
        if (dto.Stock.HasValue)
            product.Stock = dto.Stock.Value;
        
        if (dto.Status.HasValue)
            product.Status = dto.Status.Value;

        // Atualiza o TraceId com o contexto atual se disponível
        if (!string.IsNullOrEmpty(_traceContext.TraceId))
            product.TraceId = _traceContext.TraceId;

        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return false;

        await _productRepository.DeleteAsync(id);
        return true;
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Status = product.Status,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
