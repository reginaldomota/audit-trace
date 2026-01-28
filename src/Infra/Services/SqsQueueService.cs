using Amazon.SQS;
using Amazon.SQS.Model;
using Application.DTOs;
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

    public async Task SendMessageAsync(string message, string? traceId = null)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = message
        };
        
        // Adiciona traceId como atributo da mensagem se fornecido
        if (!string.IsNullOrEmpty(traceId))
        {
            request.MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                { "TraceId", new MessageAttributeValue { DataType = "String", StringValue = traceId } }
            };
        }

        await _sqsClient.SendMessageAsync(request);
    }

    public async Task<QueueMessage?> ReceiveMessageAsync()
    {
        var request = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 5,
            MessageAttributeNames = new List<string> { "All" } // Recebe todos os atributos
        };

        var response = await _sqsClient.ReceiveMessageAsync(request);

        if (response.Messages.Count == 0)
            return null;

        var message = response.Messages[0];
        
        // Extrai traceId dos atributos, se existir
        string? traceId = null;
        if (message.MessageAttributes.TryGetValue("TraceId", out var traceIdAttribute))
        {
            traceId = traceIdAttribute.StringValue;
        }
        
        var queueMessage = new QueueMessage
        {
            Body = message.Body,
            ReceiptHandle = message.ReceiptHandle,
            TraceId = traceId
        };
        
        // Deletar mensagem após receber
        await DeleteMessageAsync(message.ReceiptHandle);

        return queueMessage;
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
