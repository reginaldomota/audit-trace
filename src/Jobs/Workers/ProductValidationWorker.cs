using Domain.Enums;
using Domain.Interfaces;

namespace Jobs.Workers;

public class ProductValidationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _intervalMilliseconds;

    public ProductValidationWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _intervalMilliseconds = configuration.GetValue<int>("WorkerSettings:ProductValidationInterval", 10000);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRegisteredProductsAsync();
                await Task.Delay(_intervalMilliseconds, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessRegisteredProductsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var registeredProducts = await productRepository.GetByStatusAsync(ProductStatus.Registered);

        foreach (var product in registeredProducts)
        {
            await productRepository.UpdateStatusAsync(product.Id, ProductStatus.Validated);
        }
    }
}
