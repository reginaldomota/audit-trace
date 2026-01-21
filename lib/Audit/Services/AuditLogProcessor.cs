namespace Audit.Services;

using Audit.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class AuditLogProcessor : BackgroundService
{
    private readonly IAuditQueue _auditQueue;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogProcessor> _logger;

    public AuditLogProcessor(
        IAuditQueue auditQueue,
        IAuditLogService auditLogService,
        ILogger<AuditLogProcessor> logger)
    {
        _auditQueue = auditQueue;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit Log Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var auditLog = await _auditQueue.DequeueAsync(stoppingToken);
                
                if (auditLog != null)
                {
                    await _auditLogService.LogAsync(auditLog);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Audit Log Processor is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audit log from queue");
                
                // Aguarda um pouco antes de tentar novamente em caso de erro
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        _logger.LogInformation("Audit Log Processor stopped");
    }
}
