# ADNC Service Registry: Consul

[GitHub Repository](https://github.com/alphayu/adnc)

ADNC supports multiple service registration and discovery methods, controlled via the `RegisterType` setting:

- **Direct**: No registration. Services call each other using static URLs.
- **Consul**: Services register with Consul; discovery is handled by service name.
- **CoreDns**: Used in Kubernetes environments for in-cluster domain name resolution.

---

## 0. Quick Start

1. Start Consul.
2. Set `RegisterType` to `Consul` in your configuration.
3. Configure the `Consul` node (URL, service name, health check endpoint, etc.).
4. Start the service and verify it appears in the Consul UI with a passing health check.
5. Update your `RpcInfo` addresses to use the Consul service name format.

---

## 1. Registry vs. Configuration Center

- **Configuration Center** (`ConfigurationType`): Used to **load settings** from Consul KV.
- **Service Registry** (`RegisterType`): Used for **registering the instance** on startup and **discovering instances** during calls.

You can use both together (recommended) or independently.

---

## 2. Registration Process

On startup, each service triggers the registration logic:

- **Timing**:
  - `ApplicationStarted`: Registers the service with Consul.
  - `ApplicationStopping`: Deregisters the service from Consul.
- **Metadata Registered**:
  - `Name`: The service name (supports `$SERVICENAME` placeholder).
  - `Address/Port`: Extracted from Kestrel settings (automatically resolves `0.0.0.0` to local IPv4).
  - `Tags`: Used for routing or grouping (e.g., `urlprefix-/$SHORTNAME`).
  - `Check`: HTTP health check settings.

---

## 3. Configuration for Consul Registry

### 3.1 Enabling the Registry

```json
{
  "RegisterType": "Consul"
}
```

### 3.2 Configuring the Consul Node

```json
"Consul": {
  "ConsulUrl": "http://127.0.0.1:8500",
  "ServiceName": "$SERVICENAME",
  "ServerTags": [ "urlprefix-/$SHORTNAME" ],
  "HealthCheckUrl": "$RELATIVEROOTPATH/health-CHECK_ID",
  "HealthCheckIntervalInSecond": 6,
  "DeregisterCriticalServiceAfter": 20,
  "Timeout": 6
}
```

- **HealthCheckUrl**: ADNC services expose a health endpoint by default. Consul uses this to monitor instance status.

---

## 4. Service Discovery during Calls

Service addresses are defined in `RpcInfo:Address`. When `RegisterType = Consul`, use the service name as the host.

### 4.1 HTTP (Refit)
Requests to `http://{service-name}` are intercepted by `ConsulDiscoverDelegatingHandler`, which resolves the name to a healthy instance IP/Port from Consul.

### 4.2 gRPC
Requests to `consul://{service-name}` use a custom `ConsulGrpcResolver` and gRPC's `RoundRobin` load balancer. 

> Note: By convention in ADNC, the gRPC port is `HTTP Port + 1`.

---

## 5. Troubleshooting

- **Service Missing in UI**: Check `RegisterType` and `ConsulUrl`. Ensure `UseRegistrationCenter()` is called in the startup code.
- **Unhealthy Status**: Verify the `HealthCheckUrl` matches the service's actual health route. Ensure no firewall/gateway is blocking the health check.
- **Resolution Failure**: Ensure the `service-name` used in `RpcInfo` matches the name registered in Consul.

---
*If this helps, please Star & Fork.*
