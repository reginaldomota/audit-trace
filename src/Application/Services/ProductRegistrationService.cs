using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class ProductRegistrationService : IProductRegistrationService
{
    private readonly IProductRepository _productRepository;

    public ProductRegistrationService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<string> ProcessProductRegistrationAsync(QueueMessage message)
    {
        if (Guid.TryParse(message.Body, out var productId))
        {
            var product = await _productRepository.GetByIdAsync(productId);
            
            if (product != null && product.Status == ProductStatus.Created)
            {
                await _productRepository.UpdateStatusAsync(productId, ProductStatus.Registered);
                return $"Product {productId} registered successfully";
            }
            
            return $"Product {productId} not found or invalid status";
        }
        
        throw new ArgumentException($"Invalid product ID: {message.Body}");
    }
}
