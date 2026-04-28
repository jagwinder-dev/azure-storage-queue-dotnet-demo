namespace QueueDemo.Shared.Messages;

public sealed class OrderProcessingMessage
{
    public string MessageId { get; init; } = string.Empty;

    public string OrderId { get; init; } = string.Empty;

    public string CustomerEmail { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }
}
