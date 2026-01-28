using Application.DTOs;

namespace Application.Interfaces;

public interface IQueueService
{
    Task SendMessageAsync(string message, string? traceId = null);
    Task<QueueMessage?> ReceiveMessageAsync();
    Task DeleteMessageAsync(string receiptHandle);
}
