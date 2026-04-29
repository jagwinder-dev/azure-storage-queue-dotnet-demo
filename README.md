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

