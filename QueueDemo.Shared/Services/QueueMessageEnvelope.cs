namespace QueueDemo.Shared.Services;

public sealed record QueueMessageEnvelope(
    string QueueMessageId,
    string PopReceipt,
    string Body,
    long DequeueCount);
