using System.ComponentModel.DataAnnotations;

namespace QueueProducer.Api.Contracts;

public sealed record BulkCreateOrdersRequest(
    [property: Range(1, 500)] int Count,
    string? CustomerEmail,
    string? Action);
