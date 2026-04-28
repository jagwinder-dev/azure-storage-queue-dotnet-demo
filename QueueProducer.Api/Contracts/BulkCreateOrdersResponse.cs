using QueueDemo.Shared.Messages;

namespace QueueProducer.Api.Contracts;

public sealed record BulkCreateOrdersResponse(
    int Count,
    IReadOnlyCollection<OrderProcessingMessage> Messages);
