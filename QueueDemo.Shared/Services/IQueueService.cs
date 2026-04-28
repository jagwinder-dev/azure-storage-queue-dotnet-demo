using QueueDemo.Shared.Messages;

namespace QueueDemo.Shared.Services;

public interface IQueueService
{
    Task CreateQueueIfNotExistsAsync(CancellationToken cancellationToken = default);

    Task SendMessageAsync(OrderProcessingMessage message, CancellationToken cancellationToken = default);

    Task<QueueMessageEnvelope?> ReceiveMessageAsync(
        TimeSpan visibilityTimeout,
        CancellationToken cancellationToken = default);

    Task DeleteMessageAsync(
        string queueMessageId,
        string popReceipt,
        CancellationToken cancellationToken = default);
}
