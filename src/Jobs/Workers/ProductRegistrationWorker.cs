using Application.Interfaces;
using Audit.Helpers;
using Audit.Services;

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
        var registrationService = scope.ServiceProvider.GetRequiredService<IProductRegistrationService>();
        var traceContext = scope.ServiceProvider.GetRequiredService<ITraceContext>();

        var message = await queueService.ReceiveMessageAsync();

        if (message == null)
            return;

        // Configura o TraceContext usando o TraceId da mensagem (enviado pela API)
        var ctx = (TraceContext)traceContext;
        ctx.TraceId = message.TraceId ?? TraceIdGenerator.NewTraceId();
        ctx.Category = Audit.Enums.AuditCategory.Job;
        ctx.Method = "ProcessProductRegistration";

        try
        {
            // O serviço com [AuditLog] vai fazer a auditoria automaticamente
            await registrationService.ProcessProductRegistrationAsync(message);
        }
        catch
        {
            // Exceções são auditadas automaticamente pelo proxy
        }
    }
}
