# ADNC Project Tour: A Practical .NET 8 Microservices/Distributed Engineering Implementation

[GitHub Repository](https://github.com/alphayu/adnc)

If you are looking for a .NET microservices base project that is ready to run, easy to extend, and ideal for learning, ADNC is an excellent reference. It provides reusable infrastructure (`Adnc.Infra.*` / `Adnc.Shared.*`), complete demos, and comprehensive wiki documentation to help you transition from "understanding" to "hands-on modification."

This guide provides a straightforward overview of the repository's contents, the problems it solves, and the recommended learning path.

---

## At a Glance

ADNC is an open-source distributed/microservices framework based on `.NET 8`, which can also be used for monolithic projects. It focuses on the following capabilities:

- Service Registration/Discovery and Configuration Center
- Inter-service calls (HTTP / gRPC), Load Balancing, and Fault Tolerance (Polly)
- Authentication/Authorization, Logging, Tracing, Health Checks, and Metrics
- Caching, Message Queues, Event Bus, Distributed Transactions, and Read/Write Splitting

It offers a production-ready project structure and infrastructure integrations (see `README.md`).

---

## 1. What's in the Repository (By Deliverables)

### 1.1 Directory Structure

The core code resides in the `src` folder (see `README.md` for details):

- `src/Infrastructures`: Infrastructure integrations such as Consul, EventBus, Caching, etc.
- `src/ServiceShared`: Common service layers, including WebApi startup wrappers, remote call wrappers, and shared middleware.
- `src/Gateways`: API Gateway (Ocelot).
- `src/Demo`: Runnable demo microservices demonstrating different ways to organize services.

### 1.2 The "Demonstration Significance" of Demo Services

The Demo services are more than just "runnable samples." They demonstrate how services can be organized with varying levels of complexity under the same infrastructure and standards:

- **Admin**: Classic 3-tier + Contract separation.
- **Maint**: Compact 3-tier.
- **Cust**: Single-project minimal structure.
- **Ord / Whse**: DDD structure with a Domain layer.

This helps you make trade-offs in real-world projects: **Not every service requires DDD, and not every service belongs in a single monolithic project.**

---

## 2. Request Lifecycle in ADNC (Understanding Architecture)

Here is a typical request flow. In local development, the "Direct" mode works similarly; switching to Consul/CoreDns simply changes "how the downstream address is resolved":

1. The client request first hits the Gateway (`src/Gateways/Ocelot`).
2. The Gateway forwards the request to a specific service's WebApi (`src/Demo/*/Api`).
3. The service handles common logic like exceptions, authentication, CORS, Swagger, and health checks through unified middleware provided by `ServiceShared`.
4. Business logic in the Application layer orchestrates operations: querying/writing to the database, accessing cache, publishing events, and calling other services synchronously when necessary.
5. Inter-service communication typically uses one of three methods:
   - **HTTP (Refit)**: See `docs/wiki/service-http-call.md`
   - **gRPC**: See `docs/wiki/service-grpc-call.md`
   - **Events (CAP)**: See `docs/wiki/service-event-call.md`

Think of ADNC as: **Consolidating the "glue code" repeated in every service into `Infrastructures/ServiceShared`, allowing you to focus on the business logic itself.**

---

## 3. Core Capabilities at a Glance

### 3.1 Configuration Center (ConfigurationType)

Used for "loading configurations." Supports local files and Consul KV (see `docs/wiki/config-center.md`):

- `File`: Loads shared appsettings from the execution directory.
- `Consul`: Loads from Consul KV with polling for updates, supporting placeholders like `$SERVICENAME/$SHORTNAME/$RELATIVEROOTPATH`.

### 3.2 Registration Center (RegisterType)

Used for "service registration and discovery." Supports `Direct/Consul/CoreDns` (see `docs/wiki/appsettings.md` for configuration and `docs/wiki/registry-center.md` for usage):

- `Direct`: Most intuitive for development; hardcodes URLs.
- `Consul`: Services register on startup; discovered by service name during calls.
- `CoreDns`: Used for in-cluster domain names in K8S scenarios.

### 3.3 Inter-service Communication (HTTP / gRPC / Events)

The usage principle is simple and practical:

- **Query/Validation**: Use **HTTP or gRPC** for synchronous results and short call chains.
  - HTTP: `docs/wiki/service-http-call.md`
  - gRPC: `docs/wiki/service-grpc-call.md`
- **Cross-service Collaboration / Eventual Consistency**: Prefer **Events** for decoupling and reducing call chain complexity.
  - CAP: `docs/wiki/service-event-call.md`

### 3.4 Authentication & Authorization

ADNC provides an engineered approach for "service-to-service" calls: supporting both Basic and Bearer token passthrough, reducing boilerplate code for token handling (see `docs/wiki/claims-based-authentication.md`).

### 3.5 Caching, Distributed Locks, and Bloom Filter

Caching capabilities (including handling common issues like cache stampede, breakdown, and penetration) are consolidated in one document for easy implementation (see `docs/wiki/cache-redis-distributedlock-bloomfilter.md`).

### 3.6 Data Access and Transactions (EF Core + UnitOfWork)

Repositories, Unit of Work, local/distributed transactions, and raw SQL are covered in demos and docs (start with the `docs/wiki/efcore-*.md` series).

### 3.7 Observability and DevOps Friendliness

In microservices, "visibility and diagnostics" are often more critical than "writing code." ADNC provides unified entry points for:

- **Tracing**: SkyAPM (SkyWalking .NET Agent). Enablement guide: `docs/wiki/skyapm-tracing.md`.
- **Health Checks**: Services expose health check endpoints by default, compatible with Consul health probes.
- **Logging & Auditing**: Unified logging configuration and output (start with the `Logging` node in `docs/wiki/appsettings.md`).

---

## 4. Getting Started Locally

To just "get it running," follow the order in `docs/wiki/quickstart.md`. The core steps are:

1. **Shared Configuration**: `src/Demo/Shared/resources/appsettings.shared.Development.json` (Redis, RabbitMQ, etc.).
2. **Service Private Configuration**: `src/Demo/*/Api/appsettings.Development.json` (DB connection strings, ports, etc.).
3. **Database Scripts**: `doc/dbsql/adnc.sql` (one-time import).

Recommended startup projects:

- `Adnc.Gateway.Ocelot`
- `Adnc.Demo.Admin.Api`
- `Adnc.Demo.Maint.Api`
- `Adnc.Demo.Cust.Api`

---

## 5. Recommended Learning Path

To understand, modify, and stabilize your implementation, we suggest this reading order:

1. `docs/wiki/quickstart.md`: Get it running.
2. `docs/wiki/appsettings.md`: Understand key configuration nodes.
3. `docs/wiki/config-center.md`: Configuration Center (for multi-environment/unified delivery).
4. `docs/wiki/registry-center.md`: Registration Center (for real service discovery and load balancing).
5. `docs/wiki/service-http-call.md` / `docs/wiki/service-grpc-call.md`: How to write sync calls, pass authentication, and configure addresses.
6. `docs/wiki/service-event-call.md`: Event-driven patterns, idempotency, transactions, and retries.
7. `docs/wiki/feature-dev-guide.md` + `docs/wiki/api-dev-guide.md` + `docs/wiki/service-dev-guide.md`: Follow standards to add a new CRUD feature.
8. `docs/wiki/skyapm-tracing.md`: For performance diagnostics and call chain analysis.
9. `docs/wiki/quickly-docker-deploy.md`: For server or container deployment.

---

## 6. Conclusion: A Framework and a Blueprint

ADNC's value lies not just in its features, but in how it addresses common microservices challenges through **runnable demos + readable docs + reusable infrastructure code**.

You can:
- Use it as a starting point to "build a microservices foundation from scratch."
- Extract specific parts (e.g., remote call wrappers, config/registry integration, event-driven templates) to integrate into your existing systems.
