# ADNC API Layer Development Guide

[GitHub Repository](https://github.com/alphayu/adnc)

The API layer is responsible for exposing HTTP interfaces, handling routing, protocol adaptation, authentication, authorization, parameter binding, input validation, and response encapsulation. The API layer should remain "thin," avoiding business rules and transaction logic, which should be delegated to the Service layer (`Application`).

---

## 1. Design Principles

- **Clear Responsibility**: The API layer only handles protocol adaptation and boundary control (routing, auth, parameters, responses).
- **Contract-Based**: Use DTOs (`Application.Contracts`) for all input and output to avoid exposing internal entity models.
- **Consistency**: Maintain a unified routing style, response structure, and error coding strategy for easier integration by frontend and third-party systems.
- **Observability**: Interface logging, tracing, and auditing should be handled uniformly at the framework level rather than in business code.

## 2. Directory Structure (Example)

```
Api/
├── Controllers/              # Controllers
├── Filters/                  # Custom Filters (Optional)
├── Consts.cs                 # Constants/Permission Codes (Optional)
├── DependencyRegistrar.cs    # Dependency Registration 
├── MiddlewareRegistrar.cs    # Middleware Registration
├── Program.cs                # Application Entry Point
└── appsettings*.json         # Configuration Files
```

## 3. Routing and Controller Standards

- **Naming**: Use resource names (e.g., `StudentController`) with pluralized routes (e.g., `/students`).
- **Organization**: Use a unified routing root (e.g., `RouteConsts.Admin`) to avoid hardcoded strings.
- **Base Class**: Inherit from the framework's base controller (e.g., `AdncControllerBase`) to leverage common result types and features.

## 4. Authentication and Authorization

- **Default to Secure**: Require authentication for all endpoints by default; use explicit markers (e.g., `[AllowAnonymous]`) only when necessary.
- **Principle of Least Privilege**: Assign permission codes to all write operations (Create/Update/Delete) and sensitive read operations. Centralize these codes in `PermissionConsts`.
- **Explicit Schemes**: In environments with multiple auth schemes, explicitly specify which schemes are allowed for each endpoint.

## 5. Parameter Binding and Validation

- **Explicit Sources**: Use `[FromRoute]`, `[FromQuery]`, or `[FromBody]` to avoid ambiguity in parameter binding.
- **Pre-validation**: Use FluentValidation for DTOs. These are triggered automatically by the framework; do not write manual if/else checks in the API layer.
- **Semantic DTOs**: Separate DTOs for different operations (e.g., `CreationDto`, `UpdationDto`, `SearchPagedDto`) to avoid overloaded semantics.

## 6. Response and Error Handling

- **Unified Structure**: Return standard structures like `ServiceResult` or `Problem` to maintain a consistent API contract.
- **HTTP Semantics**: Return `201 Created` for creations, `204 No Content` for successful updates/deletions, `404 Not Found` for missing resources, and `400 Bad Request` for validation failures.
- **Exception Boundaries**: Business and system exceptions are converted to standard error responses via middleware. Do not "swallow" exceptions in the API layer.

## 7. Documentation

- **OpenAPI/Swagger**: Add XML summaries and response code attributes to actions to ensure the generated documentation is readable and useful.
- **Clear Examples**: Provide request examples for complex operations like paging, bulk updates, or imports/exports.

## 8. Reference Implementation

- Refer to the `Api/Controllers` in the Demo services and their corresponding `Application` implementations.
