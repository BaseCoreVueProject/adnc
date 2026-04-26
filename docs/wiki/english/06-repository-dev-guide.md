# ADNC Repository Layer Development Guide

[GitHub Repository](https://github.com/alphayu/adnc)

The Repository layer (located in the `Repository` project, or `Domain` + `Infrastructure` in DDD scenarios) is responsible for encapsulating data persistence and query capabilities. This includes entity modeling, mapping configurations, database contexts, repository implementations, and infrastructure features like read/write splitting, soft deletes, and concurrency control. The Repository layer should focus exclusively on "data access" and should not handle business orchestration or transaction boundaries (which should be controlled by the Service layer).

---

## 1. Design Principles

- **Focus on Data Access**: Repositories handle persistence and queries only; they do not process business rules or workflow orchestration.
- **Abstractions Over Implementations**: Higher layers should depend on repository interfaces, facilitating easier testing and data source replacement.
- **Composable Queries**: Provide composable query capabilities (filters, sorting, paging) instead of creating separate methods for every specific query scenario.
- **Consistent Constraints**: Cross-cutting rules such as naming conventions, soft deletes, concurrency control, and audit fields should be implemented uniformly at the infrastructure level.

## 2. Directory Structure (Example)

```
Repository/
├── Entities/                 # Entities
│   └── Config/               # EF Core Mapping Config (Fluent API)
├── Migrations/               # Migrations
└── EntityInfo.cs             # Entity Registration/Scanning
```

## 3. Entity Modeling Standards

- **Entity Responsibility**: Entities describe data structures and domain states; they do not manage application-level orchestration logic.
- **Base Class Selection**: Choose appropriate base classes or interfaces based on requirements for audit fields, soft deletes, and concurrency control (e.g., Audit base classes, `ISoftDelete`, `IConcurrency`).
- **Field Constraints**: Manage constraints like length, required status, and indexes primarily through Fluent API in entity configuration classes.

## 4. Mapping Configuration (Fluent API)

- **Unified Location**: Provide independent configuration classes for each entity in `Entities/Config` (e.g., `StudentConfig`).
- **Explicit Constraints**: Explicitly configure lengths, precision, default values, indexes, and relationships (1:1/1:N/N:N) to avoid relying on implicit conventions.
- **Shared Rules**: Common rules such as table/column naming conventions, soft delete filters, and concurrency columns should be handled centrally in base classes or the DbContext.

## 5. Migrations

- **Migration Strategy**: Use unified naming and execution standards for migrations across development, testing, and production to avoid conflicts in collaborative environments.
- **Environment Isolation**: Clearly define connection strings and execution strategies for different environments to prevent accidental application of migrations to the wrong database.

## 6. Repository Interface and Implementation

- **Interface Segregation**: Basic repository interfaces provide standard CRUD and query capabilities. Specialized queries can be added via extension methods, Specifications, or Dapper/Raw SQL.
- **Read/Write Splitting**: If enabled, the calling layer must specify if a query requires "strong consistency" (hitting the write database) to avoid data inconsistency from eventual consistency lag.
- **Transaction Boundaries**: Repository methods should not initiate business transactions. Transaction boundaries for multiple write operations are managed by the Service layer (`Application`) using `IUnitOfWork` or transaction interceptors.

## 7. Performance and Consistency

- **Paging**: Use stable sort fields for paged queries to prevent record "shuffling" between pages.
- **N+1 Issues**: Use explicit `Include` or projections for navigation properties to avoid the performance penalties of implicit lazy loading.
- **Concurrency Control**: Use concurrency tokens (RowVersion/concurrency fields) for critical writes and provide meaningful error responses on conflict.
- **Soft Deletes**: Entities with soft deletes enabled should use global query filters and provide a "Include Deleted" bypass for administrative scenarios only.

## 8. Reference Implementations

- **Entity/Mapping/Migration Examples**: See `docs/wiki/feature-dev-guide.md`.
- **EF Core Repository & Unit of Work**: See `docs/wiki/efcore-pemelo-curd.md` and `docs/wiki/efcore-pemolo-unitofwork.md`.
- **Raw SQL Capabilities**: See `docs/wiki/efcore-pemelo-sql.md`.
