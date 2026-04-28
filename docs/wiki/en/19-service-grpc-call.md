# How to Call ADNC Services Through gRPC

[GitHub repository](https://github.com/alphayu/adnc)

In ADNC, gRPC is commonly used for synchronous calls between services in addition to HTTP (Refit). gRPC provides better performance and a stronger contract based on `.proto`, making it suitable for internal service calls. This article uses `src/Demo/Cust/Api/Controllers/gRPCClientDemoController.cs` as an example to explain how the Cust service calls the Admin service through gRPC.

---

## 0. Quick Start

1. Define `.proto` files in the shared directory, such as `src/Demo/Shared/protos/services/admingrpc.proto`.
2. Generate client code by referencing `src/Demo/Shared/Remote.gRPC/Adnc.Demo.Remote.gRPC.csproj` from the caller. The project generates the gRPC client from `.proto` files.
3. Register the client in caller dependency registration with `AddgRPCClient<...>(...)`, for example in `src/Demo/Cust/Api/DependencyRegistrar.cs`.
4. Inject the generated `*gRPCClient`, construct the request and metadata such as `gRPCClientConsts.BasicHeader`, and initiate the call.

## 1. Design Suggestions

- Use gRPC mainly for read operations such as queries and validation. The longer a synchronous cross-service call chain is, the easier it is to amplify latency and failures.
- For consistency-oriented workflows, prefer event-driven collaboration such as CAP.
- Interfaces and DTOs should be generated uniformly from `.proto`. Do not share entity models across services or hand-write temporary structures just to align fields.
- gRPC calls also need governance: timeouts, retries, and circuit breakers should follow a unified strategy to avoid indefinite waits at a single call point.

## 2. Related Directories and Components

Taking the Demo project as an example, gRPC-related code is usually distributed as follows:

```text
src/Demo/Shared/protos/
├── messages/                  # proto messages
└── services/                  # proto service definitions

src/Demo/Shared/Remote.gRPC/
└── Adnc.Demo.Remote.gRPC.csproj # Generates client code
```

The server that provides gRPC services is in its own API project:

- Reference `gRPC.AspNetCore`.
- Generate the server base class through `<Protobuf ... gRPCServices="Server" />`.
- Implement `*gRPCBase` and map it to an endpoint.

## 3. Define the gRPC Service (`.proto`)

Taking `src/Demo/Shared/protos/services/admingrpc.proto` as an example:

- `service AdmingRPC` defines the service.
- `rpc GetSysConfigList(...) returns (...)` defines the method.

Notes:

- `.proto` files should be placed in the shared directory (`src/Demo/Shared/protos`) so both client and server code can be generated consistently.
- Prefer clear message types for method input/output parameters. Avoid overusing generic DTOs.

## 4. Generate Client and Server Code

### 4.1 Client (Caller)

The caller usually only needs to reference `src/Demo/Shared/Remote.gRPC/Adnc.Demo.Remote.gRPC.csproj`.

The project is configured in `Adnc.Demo.Remote.gRPC.csproj`:

- `<Protobuf Include="..\\protos\\messages\\*.proto" gRPCServices="Client" ... />`
- `<Protobuf Include="..\\protos\\services\\*.proto" gRPCServices="Client" ... />`

As long as the `.proto` file is added, the client code is generated automatically through the configured wildcard.

### 4.2 Server (Called Party)

Taking the Admin service as an example:

- gRPC implementation class: `src/Demo/Admin/Api/gRPC/AdmingRPCServer.cs`
- Inherit `AdmingRPC.AdmingRPCBase`.
- Implement the RPC method declared in the proto file.
- Register gRPC in `src/Demo/Admin/Api/DependencyRegistrar.cs` by calling `_services.AddgRPC();`.
- Map the endpoint in `src/Demo/Admin/Api/MiddlewareRegistrar.cs` with `endpoint.MapgRPCService<AdmingRPCServer>();`.

## 5. Port and Kestrel Configuration (HTTP/2)

gRPC requires HTTP/2. In the Demo, a separate gRPC port is usually opened:

- Admin: `50011` in `src/Demo/Admin/Api/appsettings.Development.json`
- Cust: `50031` in `src/Demo/Cust/Api/appsettings.Development.json`

Configuration excerpt:

```json
"Kestrel": {
  "Endpoints": {
    "Default": { "Url": "http://0.0.0.0:50010" },
    "gRPC": { "Url": "http://0.0.0.0:50011", "Protocols": "Http2" }
  }
}
```

## 6. Register the gRPC Client (`AddgRPCClient`)

In the caller, such as the Cust application-layer dependency registration:

- Generate the default policy with `GenerateDefaultgRPCPolicies()`, which is consistent with the default HTTP policy.
- Register the client with `AddgRPCClient<AdmingRPC.AdmingRPCClient>(ServiceAddressConsts.AdminDemoService, policies)`.

The implementation of `AddgRPCClient` is located at `src/ServiceShared/Application/Registrar/AbstractApplicationDependencyRegistrar.RpcClient.cs`.

It selects the Direct, Consul, or CoreDNS address based on `RegisterType` and `RpcInfo:Address`, and configures load balancing (RoundRobin) and token handling.

## 7. Passing Authentication Metadata

Headers are usually passed through `Metadata` when calling gRPC. In the Demo, only the scheme is passed, and the framework fills in the token:

- `gRPCClientConsts.BasicHeader`: Sets only `Authorization: Basic`.
- `gRPCClientConsts.BearerHeader`: Sets only `Authorization: Bearer`.

Corresponding code:

- `src/Demo/Shared/Remote.gRPC/gRPCClientConsts.cs`

The framework generates and completes the token through `TokenDelegatingHandler`, resulting in `Authorization: Basic {token}` or `Authorization: Bearer {token}`.

## 8. Calling Example (Demo)

Example controller: `src/Demo/Cust/Api/Controllers/gRPCClientDemoController.cs`

- Get dictionary options:
  - Construct `DictOptionRequest`.
  - Call `admingRPCClient.GetDictOptionsAsync(request, gRPCClientConsts.BasicHeader)`.
- Get system configuration:
  - Construct `SysConfigSimpleRequest`.
  - Call `admingRPCClient.GetSysConfigListAsync(request, gRPCClientConsts.BasicHeader)`.

Suggestion:

- The API layer can initiate calls, but it is recommended to place gRPC calls in the service layer (`Application`) so exception handling and retry rules can be managed consistently.
- Convert downstream exceptions such as timeouts and circuit-breaker failures into unified business error output instead of exposing them directly to the front end.

## 9. Fault-Tolerance Policy (Polly)

The default policy comes from `GenerateDefaultRefitPolicies()`; gRPC reuses the same policy set:

- Retry: Retry with a wait interval when a timeout or 5xx response occurs.
- Timeout: Set the maximum duration of each call.
- Circuit breaker: Pause calls for a period after consecutive failures reach the threshold to avoid spreading failures.

## 10. FAQ

- gRPC call fails: First check whether the callee has opened the gRPC port and whether `Protocols` is `Http2`; then check whether the caller address points to the gRPC port. In Direct mode, the Demo convention is HTTP port + 1.
- 401/authentication failed: Confirm that `gRPCClientConsts.BasicHeader` or `gRPCClientConsts.BearerHeader` was passed during the call; confirm that the `Basic` configuration is consistent; if Bearer is used, confirm that the inbound request carries a Bearer token.
- Service not found in Consul mode: Confirm that `RpcInfo:Address[].Service` is consistent with `ServiceAddressConsts.*` and that the service has been registered.

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
