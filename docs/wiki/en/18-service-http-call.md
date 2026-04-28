# How to Call ADNC Services Through HTTP (Refit)

[GitHub repository](https://github.com/alphayu/adnc)

In ADNC, synchronous calls between services, such as service A calling an interface of service B, usually use HTTP with Refit or gRPC. This article uses `src/Demo/Cust/Api/Controllers/RestClientDemoController.cs` as an example to explain how the Cust service calls the Admin service over HTTP, covering interface definition, client registration, address configuration, authentication, and fault-tolerance policies.

---

## 0. Quick Start

1. Define the Refit interface in the shared project, for example `src/Demo/Shared/Remote.Http/Services/IAdminRestClient.cs`.
2. Register the client in the caller service with `AddRestClient<IAdminRestClient>(...)`, for example in `src/Demo/Cust/Api/DependencyRegistrar.cs`.
3. Inject and call the client in business code, such as by injecting `IAdminRestClient` through a controller or service constructor.

## 1. Design Suggestions

- Use HTTP calls mainly for read operations such as queries and validation. The longer a synchronous cross-service call chain is, the easier it is to amplify latency and failures.
- For cross-service consistency, prefer event-driven collaboration such as CAP.
- Depend only on interfaces and DTOs. Do not directly reference another service's API project, and do not share entity models across services.
- Let the framework handle common concerns such as authentication, retry, timeout, and circuit breaking. Business code should focus on what to call and how to use the result.

## 2. Related Directories and Components

Taking the Demo project as an example, HTTP call-related code is usually distributed as follows:

```text
src/Demo/Shared/Remote.Http/
├── Messages/                  # Request/response DTOs shared across services
└── Services/                  # Refit client interfaces shared across services
```

## 3. Define the Service Call Interface (Refit Client)

### 3.1 Interface Constraints

The HTTP client interface between services needs to inherit `IRestClient`, which is a marker interface used to constrain the generic scope of `AddRestClient<T>`.

- `src/ServiceShared/Remote/Http/IRestClient.cs`

### 3.2 Example: `IAdminRestClient`

Taking `src/Demo/Shared/Remote.Http/Services/IAdminRestClient.cs` as an example:

- Use Refit attributes to describe the request shape, such as `[Get("/api/admin/dicts/options")]`.
- Use `[Headers("Authorization: Basic")]` to specify the authentication scheme. You only need to write the scheme (`Basic` or `Bearer`); the framework completes the token automatically.

In simple terms:

- `Authorization` supports the `Basic` and `Bearer` schemes.
- `Basic` is more suitable for inter-service calls where user permissions do not need to be passed through the entire chain.
- `Bearer` is more suitable when the downstream call should inherit the current user identity; the user's Bearer token is passed through to the downstream service.

## 4. Authentication and Token Generation

### 4.1 Outbound Token Injection

`AddRestClient` mounts `TokenDelegatingHandler` (`src/ServiceShared/Remote/Handlers/TokenDelegatingHandler.cs`) by default. When the Refit interface declares an `Authorization` header such as `Basic` or `Bearer`, the handler generates the token according to the scheme and writes back `Authorization: {scheme} {token}`.

### 4.2 Basic and Bearer Generation Logic

- Basic: `src/ServiceShared/Remote/Handlers/Token/BasicTokenGenerator.cs` generates a short-lived token (`BasicTokenValidator.PackToBase64`) based on `BasicOptions` (username/password). The current `UserContext.Id` is also written into the token to identify the caller.
- Bearer: `src/ServiceShared/Remote/Handlers/Token/BearerTokenGenerator.cs` directly intercepts and forwards the Bearer token from the current inbound request's `Authorization` header, meaning the downstream service is called with the current user's identity.

### 4.3 Basic Configuration

The Basic authentication configuration is located in `appsettings.shared.{Environment}.json`. A typical configuration is:

```json
"Basic": {
  "UserName": "adnc",
  "Password": "your-strong-secret"
}
```

## 5. Register the HTTP Client (`AddRestClient`)

Register the Refit client in application-layer dependency registration through `AddRestClient<T>`. Taking `src/Demo/Cust/Api/DependencyRegistrar.cs` as an example:

- Generate the default Polly policy with `GenerateDefaultRefitPolicies()` (`src/ServiceShared/Application/Extensions/DependencyRegistrarExtension.cs`).
- Register the client with `AddRestClient<IAdminRestClient>(ServiceAddressConsts.AdminDemoService, restPolicies)`.

`ServiceAddressConsts` (`src/Demo/Shared/Remote.Http/ServiceAddressConsts.cs`) defines service-name constants that match the `RpcInfo:Address` node in configuration.

## 6. Service Discovery and Address Configuration (`RegisterType` + `RpcInfo`)

`AddRestClient` reads:

- `RegisterType`: Decides whether to use direct addresses, CoreDNS addresses, or Consul addresses.
- `RpcInfo`: Contains the `Polly` switch and the address list for each service.

Configuration example:

```json
"RegisterType": "Direct",
"RpcInfo": {
  "Polly": { "Enable": true },
  "Address": [
    {
      "Service": "adnc-demo-admin-api",
      "Direct": "http://localhost:50010",
      "Consul": "http://adnc-demo-admin-api",
      "CoreDns": "http://adnc-demo-admin-api.default.svc.cluster.local"
    }
  ]
}
```

Notes:

- `Direct`: Most commonly used for local development. Fill in the URL directly.
- `Consul`: Service registration/discovery mode. Configure the service name as the address; `ConsulDiscoverDelegatingHandler` performs instance discovery and routing.
- `CoreDns`: Commonly used in Kubernetes scenarios, accessed through the in-cluster domain name.

## 7. Calling Example (Controller/AppService)

Taking `src/Demo/Cust/Api/Controllers/RestClientDemoController.cs` as an example, the controller injects `IAdminRestClient` through the constructor and initiates calls:

- `GetDictOptionsAsync`: Calls the Admin service to obtain dictionary options.
- `GetSysConfigListAsync`: Calls the Admin service to obtain the system configuration list.

Suggestion:

- The API layer can initiate calls, but it is recommended to place cross-service calls in the service layer (`Application`) so exception handling and retry rules can be managed consistently.
- Convert downstream exceptions such as timeouts, 5xx responses, and circuit-breaker failures into unified business error output instead of exposing downstream details directly to the front end.

## 8. Fault-Tolerance Policy (Polly)

The default policy is provided by `GenerateDefaultRefitPolicies()` and includes:

- Retry: Retry with a wait interval when a timeout or 5xx response occurs.
- Timeout: Set the maximum duration of each call.
- Circuit breaker: Pause calls for a period after consecutive failures reach the threshold to avoid spreading failures.

You can disable the policy through `RpcInfo:Polly:Enable`, but disabling it in production is not recommended.

## 9. FAQ

- 401/authentication failed: Confirm that the Refit interface declares the correct `Authorization` scheme (`Basic` or `Bearer`); confirm that the `Basic` configuration is consistent; if Bearer passthrough is used, confirm that the inbound request carries a Bearer token.
- Service address not found: Confirm that `ServiceAddressConsts.*` exactly matches `RpcInfo:Address[].Service`; confirm that the address field corresponding to the current `RegisterType` is configured.
- Call chain is too long: Longer synchronous call chains are less stable. Consider event-driven decomposition or introduce aggregation/query services to reduce call depth.

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
