namespace Application.DTOs;

public class QueueMessage
{
    public string Body { get; set; } = string.Empty;
    public string ReceiptHandle { get; set; } = string.Empty;
}
