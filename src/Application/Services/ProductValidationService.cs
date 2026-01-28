using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class ProductValidationService : IProductValidationService
{
    private readonly IProductRepository _productRepository;

    public ProductValidationService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<string> ValidateProductAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        
        if (product == null)
            throw new ArgumentException($"Product {productId} not found");
        
        if (product.Status != ProductStatus.Registered)
            throw new InvalidOperationException($"Product {productId} is not in Registered status");
        
        await _productRepository.UpdateStatusAsync(productId, ProductStatus.Validated);
        
        return $"Product {productId} validated successfully";
    }
}
