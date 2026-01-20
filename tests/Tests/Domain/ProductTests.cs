using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Product_ShouldInitialize_WithCorrectDefaults()
    {
        // Act
        var product = new Product();

        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(ProductStatus.Created, product.Status);
        Assert.True(product.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Product_ShouldSetProperties_Correctly()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 50
        };

        // Assert
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("Test Description", product.Description);
        Assert.Equal(99.99m, product.Price);
        Assert.Equal(50, product.Stock);
    }

    [Fact]
    public void Product_ShouldGenerateUniqueIds()
    {
        // Act
        var product1 = new Product();
        var product2 = new Product();

        // Assert
        Assert.NotEqual(product1.Id, product2.Id);
    }
}
