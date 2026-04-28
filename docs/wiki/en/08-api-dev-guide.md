# ADNC API Layer Development Guidelines

[GitHub repository](https://github.com/alphayu/adnc)

The API layer provides external HTTP interfaces and handles routing, protocol adaptation, authentication and permission checks, parameter binding, input validation, response wrapping, and error output. Keep the API layer thin: business rules and transaction logic should stay in the service layer (`Application`).

## 1. Design Principles

- Clear responsibilities: The API layer only handles protocol adaptation and boundary control, including routing, authentication, parameters, and responses. It should not directly implement business rules.
- Contract-oriented: Inputs and outputs should use DTOs from `Application.Contracts`; do not expose entity models directly.
- Consistency: Use consistent routing, response structures, error codes, and error messages to simplify front-end and third-party integration.
- Observability: API logs, tracing, and audit information should be handled consistently at the framework layer instead of repeated in business code.

## 2. Directory Structure Example

```text
Api/
├── Controllers/              # Controllers
├── Filters/                  # Filters, optional
├── Consts.cs                 # Constants and permission codes, optional
├── DependencyRegistrar.cs    # Dependency registration
├── MiddlewareRegistrar.cs    # Middleware registration
├── Program.cs                # Application entry point
└── appsettings*.json         # Configuration files
```

## 3. Routing and Controller Guidelines

- Controller naming: Name controllers after resources, such as `StudentController`, and use plural resource routes such as `/students`.
- Route organization: Prefer a unified route root, such as `RouteConsts.Admin`, to avoid scattered hard-coded routes.
- Controller base class: Inherit the framework-provided controller base class, such as `AdncControllerBase`, to reuse common result handling and basic capabilities.

## 4. Authentication and Permission Control

- Default authentication: Prefer global policies that require authentication for all interfaces by default. Anonymous endpoints should be explicitly marked, such as with `[AllowAnonymous]`.
- Minimum permissions: Configure permission codes for write endpoints, such as create, update, and delete, and for sensitive read endpoints. Manage permission codes centrally, such as in `PermissionConsts`.
- Authentication schemes: In scenarios that support multiple authentication schemes, clearly define which scheme combinations each endpoint allows to avoid unclear default behavior.

## 5. Parameter Binding and Input Validation

- Clear parameter sources: Mark `[FromRoute]`, `[FromQuery]`, and `[FromBody]` explicitly to avoid ambiguity from implicit binding.
- Pre-validation: Use FluentValidation for DTO input validation. The framework triggers validators uniformly, so the API layer does not need repeated handwritten `if` / `else` checks.
- Semantic DTOs: Separate creation, update, query, and paging DTOs, such as `CreationDto`, `UpdationDto`, and `SearchPagedDto`, so one DTO does not carry multiple meanings.

## 6. Response and Error Handling

- Response structure: Prefer a unified result structure, such as `ServiceResult` / `Problem`, to avoid multiple return formats for the same kind of endpoint.
- HTTP semantics: Return 201 for creation, 204 or a unified result for successful update/delete, 404 when a resource does not exist, and 400 for validation failures.
- Exception boundary: Business and system exceptions are converted into standard error responses by unified exception-handling middleware. The API layer should not swallow exceptions directly.

## 7. API Documentation and Examples

- OpenAPI/Swagger: Add necessary summaries and response code annotations to controllers/actions so generated documentation remains readable.
- Clear examples: Provide request examples and field descriptions for endpoints such as paging, batch operations, import, and export to lower the adoption cost.

## 8. Reference Implementation

For details, see `Api/Controllers` and the corresponding `Application` service implementations in each Demo service.

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
