# How to Use the Configuration Center (Consul) in ADNC

[GitHub repository](https://github.com/alphayu/adnc)

This project supports two configuration loading methods:

- `File`: Reads configuration from local `appsettings` files, suitable for local development.
- `Consul`: Reads configuration from Consul KV, suitable for unified management and dynamic delivery in test, staging, and production environments.

> Note: The `Nacos/Etcd` branch has been reserved in the code but is not currently implemented. See `src/ServiceShared/WebApi/Extensions/WebApplicationBuilderExtension.cs`.

---

## 0. Quick Start

1. Start Consul, or use an existing Consul environment.
2. Write configuration to Consul KV. This usually includes two entries: shared configuration and service-specific configuration.
3. Switch the service `ConfigurationType` to `Consul`, and configure `ConsulUrl` and `ConsulKeyPath`.
4. Start the service and confirm that configuration has taken effect. After modifying KV values, wait a few seconds to check whether they take effect automatically; this project uses polling refresh by default.

## 1. Configuration Loading Process

When each service starts, it calls `AddConfiguration` (`src/ServiceShared/WebApi/Extensions/WebApplicationBuilderExtension.cs`). The core logic is:

- Read `ConfigurationType`.
- If `ConfigurationType = File`, load `appsettings.shared.{Environment}.json` from the runtime directory (`AppContext.BaseDirectory`).
- If `ConfigurationType = Consul`, read `ConsulUrl` and `ConsulKeyPath` from the `Consul` node and load the corresponding KV values.
- After loading completes, automatically replace placeholders in configuration.
- When the configuration source changes and reload is triggered, placeholder replacement is performed again.

Supported placeholders:

- `$SERVICENAME`
- `$SHORTNAME`
- `$RELATIVEROOTPATH`

## 2. Switch to the Configuration Center (Consul)

Taking the Demo Cust service as an example, `appsettings.*.json` in test, staging, and production environments has been configured to use `Consul`:

- `src/Demo/Cust/Api/appsettings.Staging.json`

Configuration excerpt:

```json
{
  "ConfigurationType": "Consul",
  "Consul": {
    "ConsulUrl": "http://172.80.0.4:8500",
    "ConsulKeyPath": "adnc/staging/shared/appsettings,adnc/staging/$SHORTNAME/appsettings"
  }
}
```

Notes:

- `ConsulUrl`: Consul HTTP API address.
- `ConsulKeyPath`: Consul KV key path. Multiple keys can be separated by commas and are loaded in order.
- `$SHORTNAME`: Automatically replaced by the current service short name at startup, such as `cust-api` or `admin-api`.

## 3. Organize KV Keys (Shared + Service-Specific)

This project recommends loading shared configuration first and service-specific configuration second:

- Shared: `adnc/{env}/shared/appsettings`
- Service-specific: `adnc/{env}/{shortName}/appsettings`

Load both configurations through `ConsulKeyPath`:

```text
adnc/staging/shared/appsettings,adnc/staging/$SHORTNAME/appsettings
```

Loading order matters:

- Put `shared` first to provide common configuration for each service, such as Redis, RabbitMQ, RpcInfo, and SkyWalking.
- Put service-specific configuration last to override only the values that differ by service, such as database connection strings and ports.

The KV value content is JSON text, consistent with `appsettings.json`. The Consul provider parses the KV value as a JSON configuration file (`src/Infrastructures/Consul/Configuration/DefaultConsulConfigurationProvider.cs`).

## 4. Do Configuration Changes Take Effect Automatically?

Yes.

When using the Consul configuration source, loading configuration enables `reloadOnChanges=true`. The Consul provider polls KV every 3 seconds, compares `LastIndex`, triggers configuration reload (`OnReload()`) when changes are found, and performs placeholder replacement again.

Notes:

- Automatic reload only affects the configuration read path. If a component reads and caches configuration at startup, it may still need a restart to take full effect, depending on that component's implementation.
- Configuration items that require hot update should be designed with Options/Monitor patterns or another dynamically readable mechanism. Sensitive changes, such as connection strings or certificates, are recommended to take effect through staged rollout and restart.

## 5. Initialize Consul KV with devops-staging

This repository provides Consul deployment and KV initialization scripts for the staging environment:

- Consul Compose: `deploy/staging/adnc-consul/docker-compose.yml`
- KV initialization script: `deploy/staging/adnc-consul/consul-init.sh`
- Initial KV data: `deploy/staging/adnc-consul/kv.json`

After the script starts, it runs `consul kv import ... @/consul/kv.json` to import the initial KV values.

Notes:

- The `value` field in `kv.json` is base64 encoded, as required by Consul's import format. After import, Consul stores the decoded JSON content.
- The Compose file exposes the Consul UI on host port `8590` by default (`deploy/staging/adnc-consul/docker-compose.yml`).

## 6. FAQ

- Startup reports that Consul configuration cannot be found: Check whether `ConfigurationType` is `Consul`; check whether `ConsulUrl` is accessible; check whether `ConsulKeyPath` has a corresponding key.
- Configuration changes do not take effect: Confirm that the correct key was modified; wait 3 to 5 seconds; check the service logs; confirm whether the configuration item requires restart to take effect.
- Shared and service-specific configuration conflict: Confirm the loading order, where service-specific configuration should come last. Try to override only necessary items in service-specific configuration instead of redefining many nodes.

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
