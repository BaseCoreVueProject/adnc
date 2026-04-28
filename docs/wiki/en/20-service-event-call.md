# ADNC Inter-service Communication: Events (CAP)

[GitHub Repository](https://github.com/alphayu/adnc)

In microservices, Event-driven communication is often preferred over synchronous calls (HTTP/gRPC) for cross-service collaboration. A service publishes an event ("something happened"), and other services subscribe to and process it. This reduces coupling and facilitates eventual consistency.

This guide uses `CustomerRechargedEventSubscriber.cs` as an example of event publishing and subscription using **CAP (DotNetCore.CAP)**.

---

## 0. When to Use Events vs. HTTP/gRPC?

- **Use Events**: For cross-service write operations and eventual consistency (e.g., notifying inventory/marketing after a successful payment).
- **Use HTTP/gRPC**: For queries or validations where an immediate result is required (e.g., fetching a dictionary or checking a status).

Rule of thumb: If the caller **must wait** for the result to continue, use HTTP/gRPC. If the caller just needs to **inform others** what happened, use Events.

## 1. Quick Start (4 Steps)

1. **Define Event DTO**: Place it in `Remote.Event` (e.g., `CustomerRechargedEvent`).
2. **Publish**: Inject `IEventPublisher` and call `PublishAsync(...)` in your business service.
3. **Subscribe**: Create a class implementing `ICapSubscribe` and mark the handler with `[CapSubscribe("TopicName")]`.
4. **Configure CAP**: Register the subscriber in the dependency registrar using `AddCapEventBus([...])`.

## 2. Event DTO (Shared Contract)

Event DTOs should inherit from `BaseEvent`, which provides standard fields like `Id` (unique event identifier) and `OccurredDate`.

```csharp
public class CustomerRechargedEvent : BaseEvent
{
    public long CustomerId { get; set; }
    public decimal Amount { get; set; }
}
```

The Topic name defaults to the type name (e.g., `"CustomerRechargedEvent"`).

## 3. Publishing Events

Typically, you persist a business record (e.g., a transaction log) and then publish the event. Even if the subscriber fails initially, the business record allows for tracking and retries.

```csharp
await eventPublisher.PublishAsync(customerRechargedEvent);
```

## 4. Subscribing to Events

A subscriber implements `ICapSubscribe`. CAP invokes the marked method when a message is received.

```csharp
[CapSubscribe(nameof(CustomerRechargedEvent))]
public async Task HandleAsync(CustomerRechargedEvent @event) { ... }
```

## 5. Subscription Requirements: Idempotency & Transactions

### 5.1 Idempotency
Message systems may deliver the same message multiple times due to retries or network issues. **Subscribers must ensure that processing the same event twice has no side effects.**

ADNC uses a message tracker (`IMessageTracker`):
- Before processing, check: `HasProcessedAsync(eventId, handlerName)`.
- After success, mark: `MarkAsProcessedAsync(eventId, handlerName)`.

### 5.2 Transactions
Subscribers often perform multiple writes (e.g., updating balance + updating log status). These should be wrapped in an `IUnitOfWork` to ensure atomicity.

## 6. CAP Configuration

CAP is configured to use:
- **RabbitMQ**: As the message broker.
- **MySQL/MariaDB**: For message persistence (table prefix `cap`).
- **Retries**: Built-in retry logic for failed consumption.

---
*If this helps, please Star & Fork.*
