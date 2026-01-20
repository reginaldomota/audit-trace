using Amazon.SQS;
using Amazon.SQS.Model;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infra.Services;

public class SqsQueueService : IQueueService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;

    public SqsQueueService(IAmazonSQS sqsClient, IConfiguration configuration)
    {
        _sqsClient = sqsClient;
        _queueUrl = configuration["AWS:SQS:QueueUrl"] ?? string.Empty;
    }

    public async Task SendMessageAsync(string message)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = message
        };

        await _sqsClient.SendMessageAsync(request);
    }

    public async Task<string?> ReceiveMessageAsync()
    {
        var request = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 5
        };

        var response = await _sqsClient.ReceiveMessageAsync(request);

        if (response.Messages.Count == 0)
            return null;

        var message = response.Messages[0];
        
        // Deletar mensagem após receber
        await DeleteMessageAsync(message.ReceiptHandle);

        return message.Body;
    }

    public async Task DeleteMessageAsync(string receiptHandle)
    {
        var request = new DeleteMessageRequest
        {
            QueueUrl = _queueUrl,
            ReceiptHandle = receiptHandle
        };

        await _sqsClient.DeleteMessageAsync(request);
    }
}
