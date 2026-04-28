This project demonstrates how to use Azure Storage Queue to implement background processing using the Producer-Consumer pattern in a .NET application.

## Problem

In a typical application, handling tasks like sending emails or updating inventory inside an API request increases response time and reduces scalability.

## Solution

This demo shows how to:
- Offload non-critical tasks to Azure Storage Queue
- Process them asynchronously using a background worker
- Improve performance and scalability

---

## Architecture

- **API (Producer)**  
  Accepts requests and sends messages to Azure Storage Queue  

- **Worker (Consumer)**  
  Reads messages from the queue and processes them  

- **Azure Storage Queue**  
  Acts as a buffer between API and background processing  

---

## Flow

1. Client sends request to API  
2. API validates and saves required data  
3. API sends a message to Azure Storage Queue  
4. Worker reads the message  
5. Worker processes the task (email, inventory, etc.)  
6. Message is deleted after successful processing  

---

## Tech Stack

- .NET 8 / ASP.NET Core  
- Azure Storage Queue  
- Worker Service  
- Azurite (local storage emulator)  

---

## Run Locally

### 1. Start Azurite (Azure Storage Emulator)

<<<<<<< HEAD
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
=======
```bash
docker-compose up -d
>>>>>>> 77a2621 (Update ReadMe with project details)
