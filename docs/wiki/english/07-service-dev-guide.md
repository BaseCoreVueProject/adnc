# ADNC Service Layer Development Guide

[GitHub Repository](https://github.com/alphayu/adnc)

The Service layer (`Application` / `Application.Contracts`) is responsible for business orchestration, DTO mapping, transaction control, and cross-service calls. It is the core of the application layer. We recommend following design principles focused on layering, decoupling, and testability.

---

## 1. Design Principles

- **Single Responsibility**: Each application service should handle business for only one aggregate or module.
- **Interface Segregation**: Interfaces are defined in `Application.Contracts`, while implementations reside in `Application`.

## 2. Directory Structure

```
Application/
├── Services/              # Business Service Implementations
├── Dtos/                  # Data Transfer Objects (Internal or Logic)
├── Validators/            # Parameter Validation Rules
├── Cache/                 # Cache Handling Objects
├── Subscribers/           # Event Subscribers (if any)
├── MapperProfile.cs       # Object Mapping Configuration
└── DependencyRegistrar.cs # Dependency Registration
Contracts/
├── Dtos/                  # DTO Definitions
└── Interfaces/            # Service Interface Definitions
```

## 3. Dependency Injection & Interceptors

- Inject dependencies like repositories, remote services, and domain services via constructors. Avoid implicit dependencies like Service Locators.
- The framework automatically applies interceptors for caching, logging, and transactions.

## 4. DTO Mapping

- Use Mapster or AutoMapper for conversions between Entities and DTOs.
- DTOs should be pure data carriers and should not contain business logic.

## 5. Transaction Control

- Supports both automatic transactions (via interceptors) and manual control (via `IUnitOfWork`).
- Transaction boundaries should generally be managed at the Service layer.

## 6. Cross-Service Calls

- Supports multiple remote call methods: Refit (HTTP), gRPC, and CAP (Events).
- Remote interfaces are defined in `Shared/Remote.Http`, `Remote.Grpc`, and `Remote.Event`.

## 7. Parameter Validation

- Define validation rules using FluentValidation.
- The framework automatically registers and applies these validators.

## 8. Errors and Exceptions

- Handled via exception middleware.
- Return standard `ServiceResult` or `Problem` responses.

## 9. Reference Documentation

- [Application Service Documentation](https://aspdotnetcore.net/docs/application-service/)
- Refer to the `Application` layer implementation in the Demo services for examples.
