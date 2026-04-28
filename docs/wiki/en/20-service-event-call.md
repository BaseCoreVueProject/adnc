# How ADNC Services Communicate Through Events (CAP)

[GitHub repository](https://github.com/alphayu/adnc)

In microservices, cross-service collaboration should often use event-driven communication in addition to synchronous calls (HTTP/gRPC): one service publishes what happened, and other services subscribe and process it as needed. This reduces coupling, shortens call chains, and makes eventual consistency easier to implement.

This article uses `src/Demo/Cust/Api/Application/Subscribers/CustomerRechargedEventSubscriber.cs` as an example to introduce event publishing and subscription in ADNC based on CAP (DotNetCore.CAP), and contrasts it with synchronous calls such as `docs/wiki/service-http-call-zh.md`.

---

## 0. When to Use Events and When to Use HTTP/gRPC

- Suitable for events: cross-service write collaboration and eventual consistency workflows, such as notifying inventory, points, or marketing after a successful payment.
- Suitable for HTTP/gRPC: queries and validations that require immediate results, such as retrieving dictionaries, retrieving configurations, or verifying a status.

Simple rule: if the caller must wait for the downstream result before continuing the current request, use HTTP/gRPC. If the caller only needs to tell others what happened and does not require an immediate result, use events.

## 1. Quick Start

1. Define the shared event DTO under `src/Demo/Shared/Remote.Event/`, for example `CustomerRechargedEvent`.
2. Publish the event by injecting `IEventPublisher` in the business service and calling `PublishAsync(...)`.
3. Subscribe to events by writing an `ICapSubscribe` subscriber class and marking the handler method with `[CapSubscribe("TopicName")]`. `TopicName` can be understood as the event name.
4. Register subscribers in dependency registration with `AddCapEventBus([...])`.

## 2. Define an Event DTO (Shared Contract)

Example: `src/Demo/Shared/Remote.Event/CustomerRechargedEvent.cs`

The event DTO inherits `BaseEvent` (`src/Infrastructures/EventBus/BaseEvent.cs`) and includes shared fields:

- `Id`: Unique event identifier. A globally unique value is strongly recommended for deduplication.
- `OccurredDate`: Event occurrence time.
- `EventSource`: Method/source that triggered the event, useful for troubleshooting.

The event body should contain only the data really needed by downstream services. Avoid putting too much context into the event.

> Convention: By default, the event topic name uses the type name (`typeof(T).Name`). For example, publishing `CustomerRechargedEvent` uses the topic name `"CustomerRechargedEvent"`; see `src/Infrastructures/EventBus/Cap/CapPublisher.cs`.

## 3. Publish an Event

Example: `RechargeAsync` in `src/Demo/Cust/Api/Application/Services/CustomerService.cs`

The core flow is:

1. First write a business record to the database, such as `TransactionLog` with status `Processing`.
2. Publish the event, such as `CustomerRechargedEvent`, carrying the required primary key, amount, and other fields.

This way, even if downstream processing fails or is delayed, the system can still track processing status and compensation logic through business records.

Example publishing code:

```csharp
await eventPublisher.PublishAsync(customerRechargedEvent);
```

## 4. Subscribe to Events

Example: `src/Demo/Cust/Api/Application/Subscribers/CustomerRechargedEventSubscriber.cs`

The subscriber class implements `ICapSubscribe` and uses `[CapSubscribe(nameof(CustomerRechargedEvent))]` to specify the subscription topic. After the message is received, CAP calls the corresponding handler method.

## 5. Two Things Subscribers Must Handle: Idempotency and Transactions

### 5.1 Why Idempotency Is Required

Message systems naturally have retries and repeated deliveries. For example, consumption failures, network jitter, and service restarts may cause the same event to be delivered again. Subscribers must guarantee that repeated consumption has no side effects.

The demo uses message deduplication records:

- `CustomerRechargedEventSubscriber` uses `MessageTrackerFactory` to create `IMessageTracker`; see `src/ServiceShared/Application/Services/Trackers/MessageTrackerFactory.cs`.
- Check whether the message has already been processed with `HasProcessedAsync(eventId, handlerName)`.
- Mark it as processed after success with `MarkAsProcessedAsync(eventId, handlerName)`.

Database implementation:

- `src/ServiceShared/Application/Services/Trackers/DbMessageTrackerService.cs`
- `src/ServiceShared/Repository/EfCoreEntities/EventTracker.cs`

It is recommended to build a unique index on `EventId + TrackerName` to prevent repeated concurrent writes.

### 5.2 Why Transactions Are Required

Subscribers usually perform multiple write operations, such as updating balance and updating transaction-flow status. These operations must either all succeed or all roll back; otherwise data inconsistency can occur.

In the demo, `IUnitOfWork` is used to explicitly start a transaction:

- `BeginTransaction()` -> multiple updates -> `CommitAsync()`
- On exception, call `RollbackAsync()` and rethrow so CAP can trigger retry.

## 6. Access CAP (Registration and Configuration)

Call `AddCapEventBus([...])` in application-layer dependency registration of the publisher/subscriber service to register subscribers.

Example in `src/Demo/Cust/Api/DependencyRegistrar.cs`:

- `AddCapEventBus([typeof(CustomerRechargedEventSubscriber), ...])`

CAP's shared registration logic is located at:

- `src/ServiceShared/Application/Registrar/AbstractApplicationDependencyRegistrar.EventBus.cs`

Key points:

- RabbitMQ is used as the message middleware, read from the configured `RabbitMq` node.
- MySQL is used as CAP's persistent storage, with table prefix `cap`.
- The default consumer thread count is `ConsumerThreadCount = 1`, which keeps consumption order more stable within the same group. Increasing it improves throughput but does not guarantee order.
- Failures are retried; the maximum retry count and interval are configured during registration.

## 7. Complete Business Example: Recharge Event

Taking Demo recharge as an example, the flow can be understood as:

1. The user initiates a recharge request, and the Cust service creates `TransactionLog(Processing)`.
2. The Cust service publishes `CustomerRechargedEvent`, carrying `CustomerId`, `TransactionLogId`, and `Amount`.
3. The Cust service, or another service, subscribes to the event and:
   - Updates balance (`Finance`).
   - Updates the transaction flow to `Finished` and records the before/after amount.
   - Writes a deduplication record to prevent repeated consumption.
4. If any step fails, roll back the transaction and throw an exception. CAP retries according to the policy until it succeeds or reaches the retry limit.

## 8. FAQ

- The event handler runs twice: Check whether idempotency (`MessageTracker`) is implemented; check whether a unique index exists in the deduplication table; confirm that `handlerName` is stable. `nameof(Method)` is recommended.
- The event is published successfully but the subscriber does not respond: Check whether `AddCapEventBus` registered the subscriber; check RabbitMQ and MySQL configuration; check whether CAP `groupName` (consumer group) is separated by environment.
- Which fields should be included in the event: Include only data required by downstream services, usually the business primary key plus key values. When more information is needed, downstream services can query the database. Do not put the entire object into the event.
