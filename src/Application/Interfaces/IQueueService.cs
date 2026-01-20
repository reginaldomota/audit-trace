namespace Application.Interfaces;

public interface IQueueService
{
    Task SendMessageAsync(string message);
    Task<string?> ReceiveMessageAsync();
    Task DeleteMessageAsync(string receiptHandle);
}
