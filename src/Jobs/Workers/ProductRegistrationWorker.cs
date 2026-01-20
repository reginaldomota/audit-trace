using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces;

namespace Jobs.Workers;

public class ProductRegistrationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _intervalMilliseconds;

    public ProductRegistrationWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _intervalMilliseconds = configuration.GetValue<int>("WorkerSettings:ProductRegistrationInterval", 5000);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueueMessagesAsync();
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

    private async Task ProcessQueueMessagesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var message = await queueService.ReceiveMessageAsync();

        if (message == null)
            return;

        if (Guid.TryParse(message, out var productId))
        {
            var product = await productRepository.GetByIdAsync(productId);
            
            if (product != null && product.Status == ProductStatus.Created)
            {
                await productRepository.UpdateStatusAsync(productId, ProductStatus.Registered);
            }
        }
    }
}
