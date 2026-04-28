# ADNC Repository Layer Development Guidelines

[GitHub repository](https://github.com/alphayu/adnc)

The Repository layer, meaning the `Repository` project or `Domain` plus `Infrastructure` in DDD scenarios, encapsulates data persistence and query capabilities. This includes entity modeling, mapping configuration, database contexts, repository interface implementations, read/write splitting, soft deletion, concurrency control, and other foundational capabilities.

The Repository layer should focus on data access. Business orchestration and transaction boundaries should usually be handled by the service layer.

## 1. Design Principles

- Focus on data access: Repository code is responsible only for persistence and querying, not business rules or process orchestration.
- Depend on abstractions: Upper layers should depend on repository interfaces instead of concrete implementations, making testing and data-source replacement easier.
- Composable queries: Prefer composable query capabilities such as filters, sorting, and paging, instead of adding a separate method for every query scenario.
- Consistent constraints: Cross-cutting rules such as naming conventions, soft deletion, concurrency control, and audit fields should be implemented consistently in the infrastructure layer.

## 2. Directory Structure Example

```text
Repository/
├── Entities/                 # Entities
│   └── Config/               # EF Core mapping configuration (Fluent API)
├── Migrations/               # Migrations, or in a separate Migrations project
└── EntityInfo.cs             # Entity registration/scanning, if required by the framework
```

## 3. Entity Modeling Guidelines

- Entity responsibilities: An entity describes data structure and domain state. It should not directly take on application-layer orchestration.
- Base class selection: Choose the proper entity base class or interface, such as audit base classes, `ISoftDelete`, or `IConcurrency`, based on audit fields, soft deletion, and concurrency requirements.
- Field constraints: Length, required fields, indexes, and similar constraints should mainly be defined through the Fluent API and managed centrally in entity configuration classes.

## 4. Mapping Configuration (Fluent API)

- Unified location: Provide an independent configuration class, such as `StudentConfig`, for each entity under `Entities/Config`.
- Explicit constraints: Explicitly configure length, precision, default values, indexes, and relationships such as 1:1, 1:N, and N:N. Avoid relying too much on implicit conventions.
- Shared rules: Table names, column names, soft delete filters, concurrency columns, and similar rules should be handled consistently in base configuration or `DbContext`.

## 5. Migrations

- Migration strategy: Standardize migration naming and execution for development, testing, and production to avoid conflicts in team development.
- Environment isolation: Keep connection strings and migration execution strategies clear across environments to avoid applying migrations to the wrong database.

## 6. Repository Interface and Implementation Suggestions

- Interface separation: Basic repository interfaces provide common CRUD and query capabilities. Special queries can be added through extension methods, Specification, Dapper, or raw SQL.
- Read/write splitting: If read/write splitting is enabled, upper layers must specify whether a query requires the write database for strongly consistent reads. Avoid hidden behavior that can cause data inconsistency.
- Transaction boundaries: Repository methods should not actively start business transactions. Transaction boundaries for multiple write operations should be controlled by the service layer (`Application`), such as through `IUnitOfWork` or a transaction interceptor.

## 7. Performance and Consistency

- Paging: Use stable sorting fields for paging queries to avoid inconsistent page results.
- N+1: Use explicit `Include` or projection for navigation property loading. Avoid performance issues from implicit lazy loading.
- Concurrency control: Use concurrency flags, such as row version or concurrency fields, for critical writes and return understandable errors for conflicts.
- Soft deletion: Entities with soft deletion enabled should use global filters consistently and provide a necessary "include deleted" query channel only for management scenarios.

## 8. Reference Implementation

- Entity, mapping, and migration example: see `docs/wiki/en/feature-dev-guide.md`.
- EF Core repository and unit of work: see `docs/wiki/en/efcore-pemelo-curd.md` and `docs/wiki/en/efcore-pemolo-unitofwork.md`.
- Raw SQL capability: see `docs/wiki/en/efcore-pemelo-sql.md`.

-----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
