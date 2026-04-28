# How to Enable SkyAPM (SkyWalking) Tracing in ADNC

[GitHub repository](https://github.com/alphayu/adnc)

This project has integrated SkyAPM (SkyWalking .NET Agent) dependencies. After enabling it, you can view request traces, service dependencies, endpoint latency, and related information in the SkyWalking UI. HTTP, gRPC, CAP publishing/consumption, Redis cache operations, and other spans can be connected to help troubleshoot issues.

---

## 0. Prerequisites

- SkyWalking OAP (backend) is deployed. This project reports data through gRPC, so `SkyWalking:Transport:gRPC:Servers` must point to an accessible OAP address. The default example is `127.0.0.1:11800`.
- The service to be traced has started and can read the `SkyWalking` configuration section.

## 1. Quick Start

1. Configure the SkyWalking OAP address by modifying `SkyWalking:Transport:gRPC:Servers`.
2. Enable the agent by setting `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore` and restarting the service.

## 2. Configuration Location

The shared Demo service configuration is:

- `src/Demo/Shared/resources/appsettings.shared.Development.json`

The `SkyWalking` node is included as an excerpt:

```json
"SkyWalking": {
  "ServiceName": "$SERVICENAME",
  "Namespace": "adnc",
  "Transport": {
    "gRPC": {
      "Servers": "127.0.0.1:11800"
    }
  }
}
```

Notes:

- `Namespace`: Distinguishes environments or projects. Optional.
- `Sampling`: Sampling strategy. Full sampling is common during development; in production, enable sampling and configure `IgnorePaths` as needed.

## 3. Enable Local Debugging (Visual Studio)

Each Demo service has reserved switches in `launchSettings.json`. Taking Cust as an example:

- `src/Demo/Cust/Api/Properties/launchSettings.json`

Uncomment the following configuration:

```json
"ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "SkyAPM.Agent.AspNetCore"
```

## 4. Enable Container/Server Deployment (Docker)

The deployment scripts include examples as comments, for example:

- `src/deploy_demo.sh`

The core step is to add two environment variables to the container:

- `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore`: enables the agent.

## 5. CAP and Redis Trace Supplement (Optional)

This project has mounted SkyAPM extensions for CAP and Redis cache. The related extensions are registered automatically:

- CAP: `src/Infrastructures/EventBus/Extensions/ServiceCollectionExtension.cs`
- Redis cache: `src/Infrastructures/Redis.Caching/Extensions/ServiceCollectionExtension.cs`

In the UI, you will usually see:

- HTTP/gRPC calls.
- CAP publishing/consumption traces, making the same business process easier to follow.
- Redis cache spans if the call path involves caching.

## 6. Verify That Tracing Works

- Whether the service actually has the agent enabled: Confirm that the runtime environment variable `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` contains `SkyAPM.Agent.AspNetCore`.
- Whether the OAP address is reachable: Confirm that `SkyWalking:Transport:gRPC:Servers` points to an accessible address and port.
- Logs: Check the log file specified by `SkyWalking:Logging:FilePath`, which defaults to `txtlogs\\skyapm-{Date}.log`.

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
