# ADNC Project Tour: A Practical .NET 8 Microservices/Distributed Engineering Practice

[GitHub Repository](https://github.com/alphayu/adnc)

If you are looking for a .NET microservices foundation project that can run out of the box, is easy to extend, and is also suitable for learning, ADNC is a good reference. The repository includes reusable infrastructure (`Adnc.Infra.*` / `Adnc.Shared.*`), complete demos, and supporting Wiki documentation, helping you move step by step from "understanding it first" to "modifying it yourself."

This article uses plain language to quickly introduce what is in this repository, what problems it mainly solves, and the recommended order for reading and getting started.

---

# First, the One-Sentence Explanation

ADNC is an open-source distributed/microservices framework based on `.NET 8`, and it can also be used for monolithic projects. It mainly focuses on these capabilities:

- Service registration and discovery, configuration center
- Inter-service calls (HTTP / gRPC), load balancing, fault-tolerance governance (Polly)
- Authentication and authorization, logging, distributed tracing, health checks, metrics
- Caching, message queues, event bus, distributed transactions, read/write splitting

It provides a project structure and infrastructure integrations that can be used directly in real projects (see `README_ZH.md`).

---

## 1. What Is in This Repository (Grouped by "Visible Results")

## 1.1 Code Directory Overview

The main body of the repository is under `src` (see `README_ZH.md` for details):

- `src/Infrastructures`: Infrastructure integrations, such as Consul, EventBus, caching, and so on.
- `src/ServiceShared`: Common service layer, such as WebApi startup encapsulation, remote call encapsulation, common middleware, and so on.
- `src/Gateways`: Gateway (Ocelot).
- `src/Demo`: Runnable sample microservices used to demonstrate different service organization styles.

## 1.2 The "Demonstration Value" of the Demo Services

The demos are not just "runnable samples." More importantly, they demonstrate that under the same infrastructure and conventions, services can organize code with different levels of complexity (see `README_ZH.md` for details):

- Admin: classic three-layer architecture + contract separation
- Maint: more compact three-layer architecture
- Cust: minimal single-project structure
- Ord / Whse: DDD structure with a domain layer

This helps you make trade-offs in real projects: **not every service must use DDD, and not every service is suitable for being piled into one project**.

---

## 2. How a Request Roughly Flows Through ADNC (Understanding the Architecture Through the "Perceived Path")

The following is a common path. It also applies when using Direct mode locally; switching to Consul/CoreDns only changes "how the downstream address is found":

1. A client request first enters the gateway (`src/Gateways/Ocelot`).
2. The gateway forwards the request to a service's WebApi (`src/Demo/*/Api`).
3. The service handles common processes such as exceptions, authentication and authorization, CORS, Swagger, and health checks through unified middleware. This part is reused from `ServiceShared`.
4. Business code in the Application layer is responsible for "orchestration": reading from the database, writing to the database, calling the cache, publishing events, and synchronously calling other services when necessary.
5. Inter-service communication usually has three forms:
   - HTTP (Refit): see `docs/wiki/service-http-call-zh.md`
   - gRPC: see `docs/wiki/service-grpc-call-zh.md`
   - Events (CAP): see `docs/wiki/service-event-call-zh.md`

You can understand ADNC as: **centralizing the repeated glue code that every service would otherwise write into `Infrastructures/ServiceShared`, so business code can focus more on the business itself.**

---

## 3. Quick Overview of ADNC's Key Capabilities (Starting from "What You Will Use")

## 3.1 Configuration Center (ConfigurationType)

Used for "loading configuration." It supports local files and Consul KV (see `docs/wiki/config-center-zh.md` for details):

- `File`: loads shared appsettings from the runtime directory
- `Consul`: loads from Consul KV, polls for refresh by default, and replaces placeholders such as `$SERVICENAME/$SHORTNAME/$RELATIVEROOTPATH`

## 3.2 Registry Center (RegisterType)

Used for "service registration and discovery." It supports `Direct/Consul/CoreDns` (for configuration, see `docs/wiki/appsettings-zh.md`; for usage, see `docs/wiki/registry-center-zh.md`):

- `Direct`: the most straightforward option during development; URLs are written directly
- `Consul`: services register when they start, and callers discover instances by service name
- `CoreDns`: used for in-cluster domain names in K8S scenarios

## 3.3 Inter-Service Communication (HTTP / gRPC / Events)

The recommended usage principle is very simple and also the most practical:

- **Queries/validation**: use HTTP or gRPC to get results synchronously and keep the call chain as short as possible  
  - HTTP: `docs/wiki/service-http-call-zh.md`
  - gRPC: `docs/wiki/service-grpc-call-zh.md`
- **Cross-service write collaboration/eventual consistency**: prefer events, which make decoupling easier and can also reduce call chains  
  - CAP: `docs/wiki/service-event-call-zh.md`

## 3.4 Authentication and Authorization

In "service-to-service call" scenarios, ADNC provides a more engineering-oriented approach: it supports both Basic and Bearer passthrough, reducing repeated Token-handling work in business code (see `docs/wiki/claims-based-authentication-zh.md`).

## 3.5 Caching, Distributed Locks, and Bloom Filters

Caching-related capabilities, including handling common issues such as cache avalanche, cache breakdown, and cache penetration, are collected in one document and are suitable for direct implementation (see `docs/wiki/cache-redis-distributedlock-bloomfilter-zh.md`).

## 3.6 Data Access and Transactions (EF Core + UnitOfWork)

Repositories, unit of work, local transactions/distributed transactions, native SQL, and related topics all have corresponding demos and documentation (you can start with the `docs/wiki/efcore-*.md` series).

## 3.7 Observability and Operations Friendliness (Logging / Tracing / Health Checks)

In microservices, "being able to see and troubleshoot" is often more important than "being able to write code." ADNC has already made many foundational capabilities available through unified entry points:

- Tracing: SkyAPM (SkyWalking .NET Agent). For how to enable it, see `docs/wiki/skyapm-tracing-zh.md`.
- Health checks: services expose health check endpoints by default and can also work with Consul for health probing (this is also used by the registry center).
- Logging and auditing: the project has unified logging configuration and output methods (you can start with the Logging node in `docs/wiki/appsettings-zh.md`).

---

## 4. How to Run It Locally (The Minimum Things You Need to Change)

If you only want to "run it and take a look" first, follow the order in `docs/wiki/quickstart-zh.md`. The core items are just these three:

1. Common configuration: `src/Demo/Shared/resources/appsettings.shared.Development.json` (Redis, RabbitMQ, and so on).
2. Private configuration for each service: `src/Demo/*/Api/appsettings.Development.json` (database connection strings, ports, and so on).
3. Database script: `doc/dbsql/adnc.sql` (import once).

When starting projects, it is recommended to begin with these 4:

- `Adnc.Gateway.Ocelot`
- `Adnc.Demo.Admin.Api`
- `Adnc.Demo.Maint.Api`
- `Adnc.Demo.Cust.Api`

---

## 5. Recommended Learning/Adoption Path (Without Detours)

If you want to "understand it, modify it, and keep it stable after modification," it is recommended to read in this order:

1. `docs/wiki/quickstart-zh.md`: run it first
2. `docs/wiki/appsettings-zh.md`: understand the key configuration nodes
3. `docs/wiki/config-center-zh.md`: configuration center (if you need multiple environments or unified configuration delivery)
4. `docs/wiki/registry-center-zh.md`: registry center (if you need real service discovery and load balancing)
5. `docs/wiki/service-http-call-zh.md` / `docs/wiki/service-grpc-call-zh.md`: how to write synchronous calls, carry authentication, and configure addresses
6. `docs/wiki/service-event-call-zh.md`: how event-driven code handles idempotency, transactions, and retries
7. `docs/wiki/feature-dev-guide-zh.md` + `docs/wiki/api-dev-guide-zh.md` + `docs/wiki/service-dev-guide.md`: follow the conventions to add a CRUD feature
8. Read `docs/wiki/skyapm-tracing-zh.md` when you need to troubleshoot performance or call chains
9. Read `docs/wiki/quickly-docker-deploy-zh.md` when you need to deploy to a server or container

---

## 6. Conclusion: Treat It as Both a "Framework" and a "Template Project"

ADNC's value is not only in "how many features it encapsulates," but also in how it connects common problems in a microservices project through **runnable demos + readable documentation + reusable infrastructure code**.

You can:

- use it as a starting point for "building a microservices foundation from scratch";
- or take only part of it (such as remote call encapsulation, configuration/registry center integration, or event-driven templates) and integrate it into your existing system.
