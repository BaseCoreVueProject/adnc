# ADNC Configuration Center: Consul

[GitHub Repository](https://github.com/alphayu/adnc)

ADNC supports two modes for loading configurations:

- **File**: Reads from local `appsettings` files (default for local development).
- **Consul**: Reads from Consul Key-Value (KV) store (recommended for Staging/Production).

---

## 0. Quick Start

1. Start Consul.
2. Add configurations to Consul KV (typically a shared config and a service-specific config).
3. Set `ConfigurationType` to `Consul` in your service's `appsettings.json`.
4. Configure `ConsulUrl` and `ConsulKeyPath`.
5. Start the service. Configurations are refreshed automatically (polling).

## 1. Configuration Loading Process

When a service starts, it calls `AddConfiguration`:

- **File Mode**: Loads `appsettings.shared.{Environment}.json` from the execution directory.
- **Consul Mode**: Connects to `ConsulUrl` and loads the keys specified in `ConsulKeyPath`.
- **Placeholder Replacement**: After loading, the system automatically replaces placeholders like `$SERVICENAME`, `$SHORTNAME`, and `$RELATIVEROOTPATH`.

## 2. Switching to Consul

Example configuration for a Staging environment:

```json
{
  "ConfigurationType": "Consul",
  "Consul": {
    "ConsulUrl": "http://172.80.0.4:8500",
    "ConsulKeyPath": "adnc/staging/shared/appsettings,adnc/staging/$SHORTNAME/appsettings"
  }
}
```

- **ConsulUrl**: The HTTP API address of your Consul instance.
- **ConsulKeyPath**: Comma-separated paths to KV keys. They are loaded in order.
- **$SHORTNAME**: Automatically replaced with the service's short name (e.g., `cust-api`).

## 3. Organizing KV Keys (Shared + Specific)

We recommend a two-layer configuration strategy:

1. **Shared Config** (`adnc/{env}/shared/appsettings`): Contains common settings like Redis, RabbitMQ, Polly, and SkyWalking.
2. **Service-Specific Config** (`adnc/{env}/{shortName}/appsettings`): Overrides or adds settings specific to that service (e.g., DB connection strings).

The KV values should be **valid JSON strings**.

## 4. Dynamic Updates (Hot Reload)

When using Consul, ADNC enables `reloadOnChanges=true`. The Consul provider polls the KV store every 3 seconds. If the `LastIndex` changes, the configuration is reloaded and placeholders are re-processed.

Note: While configurations reload, some components that cache settings on startup may still require a restart to reflect changes.

## 5. Initializing Consul KV

The repository includes scripts to initialize a Demo environment:
- **Scripts**: `doc/devops-staging/adnc-consul/consul-init.sh`
- **Initial Data**: `doc/devops-staging/adnc-consul/kv.json`

---
*If this helps, please Star & Fork.*
