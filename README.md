# AzureStorageQueueDemo

A .NET 8 learning project that demonstrates Azure Storage Queue with a producer Web API and a consumer Worker Service.

## Projects

- `QueueProducer.Api` exposes endpoints that create JSON queue messages.
- `QueueConsumer.Worker` polls the queue, processes messages, and deletes them only after successful processing.
- `QueueDemo.Shared` contains the queue message model, queue constants, configuration, and queue service abstraction.

## What Azure Storage Queue Is

Azure Storage Queue is a simple, durable message queue in an Azure Storage account. It is useful when one part of a system needs to hand work to another part asynchronously. The producer can enqueue work quickly, and the consumer can process it later at its own pace.

This demo uses the queue `order-processing-queue` and stores each message as JSON.

## Producer vs Consumer

The producer is the API. It accepts order requests and sends `OrderProcessingMessage` records to Azure Storage Queue.

The consumer is the worker. It receives one visible message at a time, simulates processing, records the message as completed for idempotency, and then deletes the queue message.

## Message Contract

```json
{
  "messageId": "unique-message-id",
  "orderId": "ORD-1001",
  "customerEmail": "customer@example.com",
  "action": "CreateOrder",
  "createdAt": "2026-04-25T00:00:00+00:00"
}
```

## Configuration

Both runnable projects use `appsettings.json`:

```json
{
  "QueueSettings": {
    "StorageConnectionString": "UseDevelopmentStorage=true",
    "QueueName": "order-processing-queue",
    "VisibilityTimeoutSeconds": 15,
    "PollingIntervalSeconds": 3,
    "ProcessingDelaySeconds": 3,
    "SimulateFailure": false,
    "ProcessedMessagesFilePath": "processed-messages.json"
  }
}
```

Use `UseDevelopmentStorage=true` with Azurite. For Azure, replace it with a real Storage Account connection string.

## Run Azurite

From the solution folder:

```powershell
docker compose up -d
```

Azurite exposes queues on port `10001`.

## Run The Producer API

```powershell
dotnet run --project QueueProducer.Api
```

Open Swagger at:

```text
http://localhost:5264/swagger
```

Health check:

```powershell
curl http://localhost:5264/api/health
```

Create one queue message:

```powershell
curl -X POST http://localhost:5264/api/orders `
  -H "Content-Type: application/json" `
  -d '{ "orderId": "ORD-1001", "customerEmail": "customer@example.com", "action": "CreateOrder" }'
```

Create multiple queue messages:

```powershell
curl -X POST http://localhost:5264/api/orders/bulk `
  -H "Content-Type: application/json" `
  -d '{ "count": 5, "customerEmail": "bulk.customer@example.com", "action": "CreateOrder" }'
```

## Run The Consumer Worker

In a second terminal:

```powershell
dotnet run --project QueueConsumer.Worker
```

Watch the structured console logs. You should see:

- message received
- processing started
- processing completed
- message deleted

## Visibility Timeout

When the worker receives a message, Azure Storage Queue hides it for `VisibilityTimeoutSeconds`. During that hidden window, other consumers cannot receive the same message.

If the worker deletes the message before the timeout expires, the message is gone permanently. If the worker fails or does not delete it, the message becomes visible again and can be retried.

To demonstrate this, set:

```json
"VisibilityTimeoutSeconds": 5,
"ProcessingDelaySeconds": 10
```

With multiple workers running, a second worker can receive the same message after the visibility timeout expires.

## Worker Failure

Set this in `QueueConsumer.Worker/appsettings.json`:

```json
"SimulateFailure": true
```

The worker will receive messages and fail before deleting them. After the visibility timeout expires, the same messages will become visible again and retry.

Set it back to `false` to let the worker complete and delete messages.

## Idempotency

Queues usually provide at-least-once delivery. That means your application must be safe if the same message is delivered more than once.

This demo tracks completed `MessageId` values in a local JSON file. Before processing, the worker checks whether the message was already completed:

- already completed: skip processing and delete the duplicate queue message
- not completed: process, mark as completed, then delete the queue message

This is intentionally simple for learning. In production, use durable storage such as a database table with a unique key on `MessageId`.

## Azure Storage Queue vs Azure Service Bus

Azure Storage Queue is best for simple, high-volume background work where you need durability and basic retry behavior.

Azure Service Bus is a richer messaging broker. It supports features such as topics and subscriptions, sessions, duplicate detection, dead-letter queues, scheduled messages, transactions, and advanced delivery semantics.

Interview summary:

- Storage Queue is simpler and storage-account based.
- Service Bus is more feature-rich and messaging-platform based.
- Choose Storage Queue for straightforward async background work.
- Choose Service Bus when messaging behavior is central to the system design.

## Interview Talking Points

- The API and worker are decoupled through a queue.
- The producer returns quickly after enqueueing work.
- The worker deletes only after successful processing.
- Visibility timeout controls when failed or slow work can retry.
- At-least-once delivery means duplicate processing is possible.
- Idempotency prevents duplicate side effects.
- `SimulateFailure` demonstrates retry behavior without changing code.
- `ProcessingDelaySeconds` and `VisibilityTimeoutSeconds` make timing behavior visible.
- Azurite gives a local Azure Storage-compatible development loop.
