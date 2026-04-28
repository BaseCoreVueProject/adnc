# How to Use the Registry Center in ADNC

[GitHub repository](https://github.com/alphayu/adnc)

This project supports multiple service registration and discovery modes, controlled by `RegisterType`:

- `Direct`: No registration is required; fixed URLs are used directly for inter-service calls.
- `Consul`: Services register to Consul, and inter-service calls discover instance addresses by service name.
- `CoreDns`: Used in Kubernetes scenarios, accessed through the in-cluster domain name.

---

## 0. Quick Start

1. Start Consul, or use an existing Consul environment.
2. Switch the service `RegisterType` to `Consul`.
3. Complete the `Consul` node configuration, including at least `ConsulUrl`, health-check configuration, service name, and tags.
4. Start the service and open the Consul UI to confirm that the service has been registered and its health check has passed.
5. Switch inter-service call addresses to the Consul format, such as `RpcInfo:Address[*]:Consul = http://{service-name}`, and verify that calls work.

---

## 1. Registry Center vs. Configuration Center

In this project:

- **Configuration Center** is controlled by `ConfigurationType` (`File/Consul/...`) and is used to load appsettings configuration. See `docs/wiki/en/config-center.md`.
- **Registry Center** is controlled by `RegisterType` (`Direct/Consul/CoreDns`) and is used for service registration at startup and service discovery during inter-service calls.

Both can be used together:

- Common practice: `ConfigurationType = Consul` (configuration comes from Consul KV) + `RegisterType = Consul` (service is also registered to Consul).
- Registry only: `ConfigurationType = File` + `RegisterType = Consul` (local configuration files, but still registered to Consul).
- Consul configuration + CoreDNS registration: `ConfigurationType = Consul` + `RegisterType = CoreDns`.

---

## 2. Service Registration Process

Each service calls the following in `Program.cs`:

- `app.UseRegistrationCenter()`; see `src/Demo/*/Api/Program.cs`.

The core logic is located in `src/ServiceShared/WebApi/Extensions/HostExtension.cs`:

- Read `RegisterType`.
- `Direct`: Do not register.
- `Consul/CoreDns`: Call `RegisterToConsul(...)`.

Registration and deregistration timing:

- `ApplicationStarted`: Register the service with Consul.
- `ApplicationStopping`: Deregister the service from Consul.

The Consul registration implementation is located at `src/Infrastructures/Consul/Registrar/RegistrationProvider.cs`. Registration information includes:

- `Name`: `Consul:ServiceName`, usually `$SERVICENAME`, automatically replaced at startup.
- `Address/Port`: From `Kestrel:Endpoints:Default:Url`. If the address is `0.0.0.0`, it is automatically replaced by an IPv4 address of the current machine.
- `Tags`: `Consul:ServerTags`, such as `urlprefix-/$SHORTNAME`, commonly used for gateway routing or grouping.
- `Check`: HTTP health check, determined by configuration such as `Consul:HealthCheckUrl`.

---

## 3. Switch to the Registry Center (Consul)

### 3.1 Set `RegisterType = Consul`

Taking local development as an example, where the default `src/Demo/Shared/resources/appsettings.shared.Development.json` is `Direct`, set the corresponding environment configuration to:

```json
{
  "RegisterType": "Consul"
}
```

> Tip: If you use the Configuration Center (Consul KV) to load shared configuration, it is recommended to place `RegisterType` in shared KV for unified management.

### 3.2 Configure the `Consul` Node

The registry center requires at least `ConsulUrl`, along with service name and health-check information. A typical configuration excerpt is:

```json
{
  "Consul": {
    "ConsulUrl": "http://127.0.0.1:8500",
    "ServiceName": "$SERVICENAME",
    "ServerTags": [ "urlprefix-/$SHORTNAME" ],
    "HealthCheckUrl": "$RELATIVEROOTPATH/health-24b01005-a76a-4b3b-8fb1-5e0f2e9564fb",
    "HealthCheckIntervalInSecond": 6,
    "DeregisterCriticalServiceAfter": 20,
    "Timeout": 6
  }
}
```

Notes:

- `ConsulUrl`: Consul HTTP API address, used for both registration and discovery.
- `ServiceName/ServerTags/HealthCheckUrl`: Supports `$SERVICENAME`, `$SHORTNAME`, and `$RELATIVEROOTPATH`; these placeholders are automatically replaced at startup. See `src/ServiceShared/WebApi/Extensions/WebApplicationBuilderExtension.cs`.
- `HealthCheckUrl`: Projects expose health-check endpoints by default. See `UseHealthChecks(...)` in `src/ServiceShared/WebApi/Registrar/AbstractWebApiMiddlewareRegistrar.cs`.

### 3.3 Confirm the `Kestrel` Default Port

The address registered to Consul comes from `Kestrel:Endpoints:Default:Url`.

For example, the Demo Cust service (`src/Demo/Cust/Api/appsettings.Development.json`) contains:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Default": { "Url": "http://0.0.0.0:50030" },
      "gRPC": { "Url": "http://0.0.0.0:50031", "Protocols": "Http2" }
    }
  }
}
```

When `Url` is bound to `0.0.0.0`, the local IPv4 address is used during registration to avoid registering an inaccessible address.

---

## 4. Discover Instances Through Consul During Inter-Service Calls

The inter-service call address is determined by `RpcInfo:Address`, and different base addresses are selected according to `RegisterType`. See `src/ServiceShared/Application/Registrar/AbstractApplicationDependencyRegistrar.RpcClient.cs`.

When `RegisterType = Consul`, configure `RpcInfo:Address[*]:Consul` in service-name format:

```json
{
  "RpcInfo": {
    "Address": [
      {
        "Service": "adnc-demo-admin-api",
        "Consul": "http://adnc-demo-admin-api"
      }
    ]
  }
}
```

### 4.1 HTTP (REST / Refit)

- When `BaseAddress` is `http://{service-name}`, requests go through `ConsulDiscoverDelegatingHandler` (`src/Infrastructures/Consul/Discover/Handler/ConsulDiscoverDelegatingHandler.cs`).
- The handler queries healthy instances from Consul by `{service-name}`, selects an instance address, and rewrites the actual request to `http://{ip}:{port}/{path}`.

### 4.2 gRPC

- When `BaseAddress` is `consul://{service-name}`, the custom resolver (`src/Infrastructures/Consul/Discover/gRPCResolver/ConsulgRPCResolver.cs`) is enabled.
- The resolver periodically pulls all healthy instances from Consul and works with gRPC `RoundRobin` for load balancing.

> Note: This project assumes that the gRPC port of the same service is HTTP port + 1. The resolver internally handles the `+1` port conversion.

---

## 5. FAQ

- Service does not appear in the Consul UI: Confirm `RegisterType = Consul`; confirm that `Consul:ConsulUrl` is correct and accessible; confirm that the service startup code calls `UseRegistrationCenter()`.
- Consul shows the service as unhealthy: Check whether `Consul:HealthCheckUrl` is consistent with the health-check route exposed by the project; confirm that the gateway/reverse proxy does not intercept this path.
- Instance address cannot be resolved when calling: Confirm that the target service is registered and healthy; confirm that the host of `RpcInfo:Address[*]:Consul` matches the registered `ServiceName`, usually the service name.

---

## 6. Quickly Start Consul

The repository provides Consul docker-compose files and initialization scripts, including KV initialization. They can also be used directly as a registry center:

- `deploy/staging/adnc-consul/docker-compose.yml`
- `deploy/staging/adnc-consul/consul-init.sh`

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
