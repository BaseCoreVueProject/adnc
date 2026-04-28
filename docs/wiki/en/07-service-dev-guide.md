# ADNC Service Layer Development Guidelines

[GitHub repository](https://github.com/alphayu/adnc)

The service layer (`Application` / `Application.Contracts`) is responsible for business orchestration, DTO mapping, transaction control, cross-service calls, and related application-layer concerns. It is a core part of the application layer. Follow layering, decoupling, and testability principles.

## 1. Design Principles

- Single responsibility: Each application service handles the business of one aggregate or module.
- Separate interface and implementation: Define interfaces in `Application.Contracts` and place implementations in `Application`.

## 2. Directory Structure

```text
Application/
├── Services/              # Business service implementations
├── Dtos/                  # Data transfer objects
├── Validators/            # Parameter validation
├── Cache/                 # Cache handling objects
├── Subscribers/           # Subscriber objects, if any
├── MapperProfile.cs       # Object mapping configuration
└── DependencyRegistrar.cs # Dependency registration
Contracts/
├── Dtos/                  # DTO definitions
└── Interfaces/            # Interface definitions
```

## 3. Dependency Injection and Interceptors

- Inject dependencies such as repositories, remote services, and domain services through constructors. Avoid implicit dependency access patterns such as Service Locator.
- The framework automatically applies interceptors for caching, logging, transactions, and other cross-cutting behavior.

## 4. DTO Mapping

- Use Mapster or AutoMapper to convert between entities and DTOs.
- DTOs are only used to carry data and should not contain business logic.

## 5. Transaction Control

- Automatic transactions through interceptors and manual control through `IUnitOfWork` are both supported.
- Transaction boundaries should usually be controlled at the service layer.

## 6. Cross-Service Calls

- Remote calls are supported through Refit, gRPC, CAP events, and similar mechanisms.
- Remote interfaces are defined in `Shared/Remote.Http`, `Remote.gRPC`, and `Remote.Event`.

## 7. Parameter Validation

- Use FluentValidation to define parameter validation rules.
- The framework automatically registers and applies validators.

## 8. Errors and Exceptions

- Exceptions are handled by exception middleware.
- Return standard `ServiceResult` / `Problem` results for business failures.

## 9. Reference Documentation

- [Service layer development documentation](https://aspdotnetcore.net/docs/application-service/)
- For details, see the Application layer implementations in each Demo service.

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
