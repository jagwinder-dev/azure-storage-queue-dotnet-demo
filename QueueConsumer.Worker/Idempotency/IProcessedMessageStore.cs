namespace QueueConsumer.Worker.Idempotency;

public interface IProcessedMessageStore
{
    Task<bool> IsProcessedAsync(string messageId, CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(string messageId, CancellationToken cancellationToken = default);
}
