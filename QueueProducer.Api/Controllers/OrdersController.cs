using Microsoft.AspNetCore.Mvc;
using QueueDemo.Shared.Constants;
using QueueDemo.Shared.Messages;
using QueueDemo.Shared.Services;
using QueueProducer.Api.Contracts;

namespace QueueProducer.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IQueueService _queueService;

    public OrdersController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderProcessingMessage>> CreateOrder(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var message = new OrderProcessingMessage
        {
            MessageId = Guid.NewGuid().ToString("N"),
            OrderId = request.OrderId,
            CustomerEmail = request.CustomerEmail,
            Action = request.Action,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _queueService.SendMessageAsync(message, cancellationToken);

        return Accepted(message);
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkCreateOrdersResponse>> CreateBulkOrders(
        BulkCreateOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var customerEmail = string.IsNullOrWhiteSpace(request.CustomerEmail)
            ? "bulk.customer@example.com"
            : request.CustomerEmail;

        var action = string.IsNullOrWhiteSpace(request.Action)
            ? OrderActions.CreateOrder
            : request.Action;

        var messages = new List<OrderProcessingMessage>(request.Count);

        for (var i = 1; i <= request.Count; i++)
        {
            var message = new OrderProcessingMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                OrderId = $"BULK-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{i:D4}",
                CustomerEmail = customerEmail,
                Action = action,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _queueService.SendMessageAsync(message, cancellationToken);
            messages.Add(message);
        }

        return Accepted(new BulkCreateOrdersResponse(messages.Count, messages));
    }
}
