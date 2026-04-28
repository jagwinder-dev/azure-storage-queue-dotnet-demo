using System.ComponentModel.DataAnnotations;

namespace QueueProducer.Api.Contracts;

public sealed record CreateOrderRequest(
    [property: Required] string OrderId,
    [property: Required, EmailAddress] string CustomerEmail,
    [property: Required] string Action);
