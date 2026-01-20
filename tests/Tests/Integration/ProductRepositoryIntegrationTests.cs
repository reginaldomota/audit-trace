using Domain.Entities;
using Domain.Enums;
using Infra.Repositories;
using Xunit;

namespace Tests.Integration;

public class ProductRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ProductRepository _repository;

    public ProductRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.CleanDatabase();
        _repository = new ProductRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistProductToDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "Produto Teste Integração",
            Description = "Descrição do produto teste",
            Price = 99.99m,
            Stock = 50,
            Status = ProductStatus.Registered
        };

        // Act
        await _repository.AddAsync(product);

        // Assert
        var savedProduct = await _repository.GetByIdAsync(product.Id);
        Assert.NotNull(savedProduct);
        Assert.Equal("Produto Teste Integração", savedProduct.Name);
        Assert.Equal(99.99m, savedProduct.Price);
        Assert.Equal(50, savedProduct.Stock);
        Assert.Equal(ProductStatus.Registered, savedProduct.Status);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnProductFromDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "Produto Busca Por Nome",
            Description = "Teste de busca",
            Price = 49.99m,
            Stock = 25,
            Status = ProductStatus.Registered
        };
        await _repository.AddAsync(product);

        // Act
        var foundProduct = await _repository.GetByNameAsync("Produto Busca Por Nome");

        // Assert
        Assert.NotNull(foundProduct);
        Assert.Equal(product.Id, foundProduct.Id);
        Assert.Equal("Produto Busca Por Nome", foundProduct.Name);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnOnlyProductsWithSpecifiedStatus()
    {
        // Arrange
        var activeProduct1 = new Product
        {
            Name = "Produto Ativo 1",
            Description = "Ativo",
            Price = 10.00m,
            Stock = 10,
            Status = ProductStatus.Registered
        };
        
        var activeProduct2 = new Product
        {
            Name = "Produto Ativo 2",
            Description = "Ativo",
            Price = 20.00m,
            Stock = 20,
            Status = ProductStatus.Registered
        };
        
        var inactiveProduct = new Product
        {
            Name = "Produto Inativo",
            Description = "Inativo",
            Price = 30.00m,
            Stock = 30,
            Status = ProductStatus.Inactive
        };

        await _repository.AddAsync(activeProduct1);
        await _repository.AddAsync(activeProduct2);
        await _repository.AddAsync(inactiveProduct);

        // Act
        var activeProducts = await _repository.GetByStatusAsync(ProductStatus.Registered);

        // Assert
        var productList = activeProducts.ToList();
        Assert.Equal(2, productList.Count);
        Assert.All(productList, p => Assert.Equal(ProductStatus.Registered, p.Status));
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "Produto Original",
            Description = "Descrição Original",
            Price = 100.00m,
            Stock = 10,
            Status = ProductStatus.Registered
        };
        await _repository.AddAsync(product);

        // Act
        product.Name = "Produto Atualizado";
        product.Price = 150.00m;
        product.Stock = 20;
        await _repository.UpdateAsync(product);

        // Assert
        var updatedProduct = await _repository.GetByIdAsync(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal("Produto Atualizado", updatedProduct.Name);
        Assert.Equal(150.00m, updatedProduct.Price);
        Assert.Equal(20, updatedProduct.Stock);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldChangeProductStatusInDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "Produto Para Atualizar Status",
            Description = "Teste",
            Price = 50.00m,
            Stock = 5,
            Status = ProductStatus.Registered
        };
        await _repository.AddAsync(product);

        // Act
        await _repository.UpdateStatusAsync(product.Id, ProductStatus.Inactive);

        // Assert
        var updatedProduct = await _repository.GetByIdAsync(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal(ProductStatus.Inactive, updatedProduct.Status);
        Assert.NotNull(updatedProduct.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProductFromDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "Produto Para Deletar",
            Description = "Será removido",
            Price = 25.00m,
            Stock = 15,
            Status = ProductStatus.Registered
        };
        await _repository.AddAsync(product);

        // Act
        await _repository.DeleteAsync(product.Id);

        // Assert
        var deletedProduct = await _repository.GetByIdAsync(product.Id);
        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProductsFromDatabase()
    {
        // Arrange
        var product1 = new Product
        {
            Name = "Produto 1",
            Description = "Descrição 1",
            Price = 10.00m,
            Stock = 10,
            Status = ProductStatus.Registered
        };
        
        var product2 = new Product
        {
            Name = "Produto 2",
            Description = "Descrição 2",
            Price = 20.00m,
            Stock = 20,
            Status = ProductStatus.Inactive
        };
        
        var product3 = new Product
        {
            Name = "Produto 3",
            Description = "Descrição 3",
            Price = 30.00m,
            Stock = 30,
            Status = ProductStatus.Validated
        };

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);

        // Act
        var allProducts = await _repository.GetAllAsync();

        // Assert
        var productList = allProducts.ToList();
        Assert.Equal(3, productList.Count);
        Assert.Contains(productList, p => p.Name == "Produto 1");
        Assert.Contains(productList, p => p.Name == "Produto 2");
        Assert.Contains(productList, p => p.Name == "Produto 3");
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var product = new Product
        {
            Name = "Produto Concorrente",
            Description = "Teste de concorrência",
            Price = 100.00m,
            Stock = 100,
            Status = ProductStatus.Registered
        };
        await _repository.AddAsync(product);
        var productId = product.Id;

        // Act - Simula operações sequenciais (DbContext não é thread-safe)
        for (int i = 0; i < 5; i++)
        {
            var prod = await _repository.GetByIdAsync(productId);
            if (prod != null)
            {
                prod.Stock -= 1;
                await _repository.UpdateAsync(prod);
            }
        }

        // Assert
        var finalProduct = await _repository.GetByIdAsync(productId);
        Assert.NotNull(finalProduct);
        Assert.Equal(95, finalProduct.Stock); // 100 - 5 = 95
    }
}
