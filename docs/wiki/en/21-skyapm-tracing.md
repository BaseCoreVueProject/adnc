# ADNC Observability: Enabling SkyAPM (SkyWalking)

[GitHub Repository](https://github.com/alphayu/adnc)

ADNC integrates **SkyAPM (SkyWalking .NET Agent)** for distributed tracing. Once enabled, you can visualize request traces, service dependencies, and operation latencies (HTTP, gRPC, CAP, Redis) in the SkyWalking UI.

---

## 0. Prerequisites

- A running **SkyWalking OAP** backend.
- Ensure the `SkyWalking:Transport:gRPC:Servers` configuration points to your OAP address (e.g., `127.0.0.1:11800`).

## 1. Quick Enablement

1. **Configure OAP Address**: Update `SkyWalking:Transport:gRPC:Servers`.
2. **Enable the Agent**: Set the environment variable `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore` and restart your services.

## 2. Configuration Settings

Shared settings are found in `appsettings.shared.Development.json`:

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

- **Namespace**: Used to distinguish environments or projects.
- **Sampling**: Controls the trace sampling rate. Use full sampling for development.

## 3. Enabling in Local Development (Visual Studio)

In the `launchSettings.json` of your API project, uncomment the following line:

```json
"ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "SkyAPM.Agent.AspNetCore"
```

## 4. Enabling in Docker/Server Deployment

Add the following environment variables to your container:

- `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore`
- `SKYWALKING__SERVICENAME=your-service-name`

## 5. Extended Tracing (CAP & Redis)

ADNC provides extensions to trace CAP events and Redis operations. These spans will automatically appear in your trace timelines when enabled.

## 6. Troubleshooting

- **Verification**: Check if the `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` variable is correctly set in the running process.
- **Connectivity**: Verify that the service can reach the SkyWalking OAP gRPC port.
- **Logs**: Check the SkyAPM agent logs, typically located at `txtlogs/skyapm-{Date}.log`.

---
*If this helps, please Star & Fork.*
