namespace QueueConsumer.Worker;

using System.Text.Json;
using Microsoft.Extensions.Options;
using QueueConsumer.Worker.Idempotency;
using QueueDemo.Shared.Configuration;
using QueueDemo.Shared.Messages;
using QueueDemo.Shared.Serialization;
using QueueDemo.Shared.Services;

public class Worker : BackgroundService
{
    private readonly IProcessedMessageStore _processedMessageStore;
    private readonly IQueueService _queueService;
    private readonly QueueSettings _settings;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IQueueService queueService,
        IProcessedMessageStore processedMessageStore,
        IOptions<QueueSettings> options,
        ILogger<Worker> logger)
    {
        _queueService = queueService;
        _processedMessageStore = processedMessageStore;
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _queueService.CreateQueueIfNotExistsAsync(stoppingToken);

        var pollingInterval = TimeSpan.FromSeconds(Math.Max(1, _settings.PollingIntervalSeconds));
        var visibilityTimeout = TimeSpan.FromSeconds(Math.Max(1, _settings.VisibilityTimeoutSeconds));
        var processingDelay = TimeSpan.FromSeconds(Math.Max(0, _settings.ProcessingDelaySeconds));

        _logger.LogInformation(
            "Consumer started. PollingIntervalSeconds {PollingIntervalSeconds}, VisibilityTimeoutSeconds {VisibilityTimeoutSeconds}, ProcessingDelaySeconds {ProcessingDelaySeconds}, SimulateFailure {SimulateFailure}",
            pollingInterval.TotalSeconds,
            visibilityTimeout.TotalSeconds,
            processingDelay.TotalSeconds,
            _settings.SimulateFailure);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var envelope = await _queueService.ReceiveMessageAsync(visibilityTimeout, stoppingToken);

                if (envelope is null)
                {
                    await Task.Delay(pollingInterval, stoppingToken);
                    continue;
                }

                await HandleMessageAsync(envelope, processingDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Processing failed. The message was not deleted and will become visible again after the visibility timeout.");

                await Task.Delay(pollingInterval, stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(
        QueueMessageEnvelope envelope,
        TimeSpan processingDelay,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Message received by consumer. QueueMessageId {QueueMessageId}, DequeueCount {DequeueCount}",
            envelope.QueueMessageId,
            envelope.DequeueCount);

        var message = JsonSerializer.Deserialize<OrderProcessingMessage>(
            envelope.Body,
            QueueJson.SerializerOptions);

        if (message is null || string.IsNullOrWhiteSpace(message.MessageId))
        {
            throw new InvalidOperationException("Queue message body could not be deserialized into an order message.");
        }

        if (await _processedMessageStore.IsProcessedAsync(message.MessageId, cancellationToken))
        {
            _logger.LogWarning(
                "Duplicate message skipped. MessageId {MessageId}, OrderId {OrderId}, QueueMessageId {QueueMessageId}",
                message.MessageId,
                message.OrderId,
                envelope.QueueMessageId);

            await _queueService.DeleteMessageAsync(
                envelope.QueueMessageId,
                envelope.PopReceipt,
                cancellationToken);

            _logger.LogInformation(
                "Duplicate message deleted. MessageId {MessageId}, QueueMessageId {QueueMessageId}",
                message.MessageId,
                envelope.QueueMessageId);

            return;
        }

        _logger.LogInformation(
            "Message processing started. MessageId {MessageId}, OrderId {OrderId}, CustomerEmail {CustomerEmail}, Action {Action}",
            message.MessageId,
            message.OrderId,
            message.CustomerEmail,
            message.Action);

        // Keep the delay visible in logs so the visibility timeout demo is easy to explain.
        await Task.Delay(processingDelay, cancellationToken);

        if (_settings.SimulateFailure)
        {
            throw new InvalidOperationException("Simulated worker failure. Disable QueueSettings:SimulateFailure to process messages successfully.");
        }

        // Mark completed before deleting. If delete fails, a later retry can skip the duplicate safely.
        await _processedMessageStore.MarkProcessedAsync(message.MessageId, cancellationToken);

        _logger.LogInformation(
            "Message processing completed. MessageId {MessageId}, OrderId {OrderId}",
            message.MessageId,
            message.OrderId);

        await _queueService.DeleteMessageAsync(
            envelope.QueueMessageId,
            envelope.PopReceipt,
            cancellationToken);

        _logger.LogInformation(
            "Message deleted. MessageId {MessageId}, QueueMessageId {QueueMessageId}",
            message.MessageId,
            envelope.QueueMessageId);
    }
}
