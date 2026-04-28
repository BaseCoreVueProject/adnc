# ADNC Inter-service Communication: HTTP (Refit)

[GitHub Repository](https://github.com/alphayu/adnc)

In ADNC, synchronous inter-service calls (e.g., Service A calling Service B) typically use HTTP (via Refit) or gRPC. This guide uses `RestClientDemoController.cs` as an example to explain how the Cust service calls the Admin service, covering interface definition, client registration, address configuration, authentication, and fault tolerance.

---

## 0. Quick Start (3 Steps)

1. **Define the Refit Interface**: Place it in a shared project (e.g., `src/Demo/Shared/Remote.Http/Services/IAdminRestClient.cs`).
2. **Register the Client**: In the caller's `DependencyRegistrar.cs`, call `AddRestClient<IAdminRestClient>(...)`.
3. **Inject and Call**: Inject `IAdminRestClient` into your Controller or Service via constructor and call the methods.

## 1. Design Recommendations

- **Use for Queries/Validations**: Synchronous calls increase latency and failure points. For cross-service consistency, prefer Event-driven patterns (e.g., CAP).
- **Depend on Interfaces + DTOs**: Do not reference the target service's API project directly or share entity models across services.
- **Offload Common Concerns**: Let the framework handle authentication, retries, timeouts, and circuit breaking.

## 2. Shared Components

HTTP call-related code is typically organized in:
```
src/Demo/Shared/Remote.Http/
├── Messages/                  # Request/Response DTOs (Shared)
└── Services/                  # Refit Client Interfaces (Shared)
```

## 3. Defining the Refit Client Interface

The client interface must inherit from `IRestClient` (a marker interface):

```csharp
[Headers("Authorization: Basic")] // Specify authentication scheme
public interface IAdminRestClient : IRestClient
{
    [Get("/api/admin/dicts/options")]
    Task<ApiResponse<List<DictRto>>> GetDictOptionsAsync();
}
```

- **Basic**: Best for internal service-to-service calls where user identity propagation is not required.
- **Bearer**: Best for scenarios where the current user's identity (token) needs to be passed downstream.

## 4. Authentication and Token Injection

The `AddRestClient` method automatically attaches a `TokenDelegatingHandler`:

1. **Outbound Requests**: When a Refit interface has an `Authorization` header, the handler generates the appropriate token based on the scheme.
2. **Basic Logic**: Generates a short-lived token based on `BasicOptions` (username/password) and includes the current `UserContext.Id` to identify the caller.
3. **Bearer Logic**: Extracts and forwards the Bearer token from the current incoming request.

## 5. Registering the HTTP Client

Register the client in the application layer. Example from `Cust` service:

```csharp
var restPolicies = GenerateDefaultRefitPolicies();
AddRestClient<IAdminRestClient>(ServiceAddressConsts.AdminDemoService, restPolicies);
```

## 6. Service Discovery and Address Configuration

The client resolution depends on:
- `RegisterType`: Determines whether to use direct URLs, CoreDNS, or Consul.
- `RpcInfo`: Contains the Polly toggle and address lists.

```json
"RegisterType": "Direct",
"RpcInfo": {
  "Polly": { "Enable": true },
  "Address": [
    {
      "Service": "adnc-demo-admin-api",
      "Direct": "http://localhost:50010",
      "Consul": "http://adnc-demo-admin-api"
    }
  ]
}
```

## 7. Fault Tolerance (Polly)

Default policies are provided by `GenerateDefaultRefitPolicies()`:
- **Retry**: Retries on timeouts or 5xx errors with an interval.
- **Timeout**: Sets a maximum duration for each call.
- **Circuit Breaker**: Pauses calls if the failure threshold is reached to prevent cascading failures.

---
*If this helps, please Star & Fork.*
