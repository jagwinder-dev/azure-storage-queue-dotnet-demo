using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueDemo.Shared.Configuration;
using QueueDemo.Shared.Messages;
using QueueDemo.Shared.Serialization;

namespace QueueDemo.Shared.Services;

public sealed class QueueService : IQueueService
{
    private readonly QueueClient _queueClient;
    private readonly QueueSettings _settings;
    private readonly ILogger<QueueService> _logger;

    public QueueService(IOptions<QueueSettings> options, ILogger<QueueService> logger)
    {
        _settings = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.StorageConnectionString))
        {
            throw new InvalidOperationException("QueueSettings:StorageConnectionString is required.");
        }

        if (string.IsNullOrWhiteSpace(_settings.QueueName))
        {
            throw new InvalidOperationException("QueueSettings:QueueName is required.");
        }

        var queueClientOptions = new QueueClientOptions
        {
            // Base64 keeps JSON safe for Azure Storage Queue message transport.
            MessageEncoding = QueueMessageEncoding.Base64
        };

        _queueClient = new QueueClient(
            _settings.StorageConnectionString,
            _settings.QueueName,
            queueClientOptions);
    }

    public async Task CreateQueueIfNotExistsAsync(CancellationToken cancellationToken = default)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        _logger.LogInformation(
            "Azure Storage Queue ready. QueueName {QueueName}",
            _settings.QueueName);
    }

    public async Task SendMessageAsync(
        OrderProcessingMessage message,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message, QueueJson.SerializerOptions);

        await _queueClient.SendMessageAsync(json, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Message sent by producer. MessageId {MessageId}, OrderId {OrderId}, Action {Action}, QueueName {QueueName}",
            message.MessageId,
            message.OrderId,
            message.Action,
            _settings.QueueName);
    }

    public async Task<QueueMessageEnvelope?> ReceiveMessageAsync(
        TimeSpan visibilityTimeout,
        CancellationToken cancellationToken = default)
    {
        QueueMessage? queueMessage = await _queueClient.ReceiveMessageAsync(
            visibilityTimeout: visibilityTimeout,
            cancellationToken: cancellationToken);

        if (queueMessage is null)
        {
            return null;
        }

        return new QueueMessageEnvelope(
            queueMessage.MessageId,
            queueMessage.PopReceipt,
            queueMessage.MessageText,
            queueMessage.DequeueCount);
    }

    public async Task DeleteMessageAsync(
        string queueMessageId,
        string popReceipt,
        CancellationToken cancellationToken = default)
    {
        await _queueClient.DeleteMessageAsync(
            queueMessageId,
            popReceipt,
            cancellationToken);
    }
}
