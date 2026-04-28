# ADNC Configuration Nodes Detailed Explanation

[GitHub Repository](https://github.com/alphayu/adnc)

## 1. Shared Service Configuration

> `adnc\src\Demo\Shared\resources\appsettings.shared.Development.json`

### 1.1 RegisterType

- **Service Registration Type**:
  - `Direct`: No registration. Inter-service calls use direct URIs (see `RpcInfo`).
  - `Consul`: Registered to Consul. Inter-service calls use service names (see `RpcInfo`).
  - `CoreDns`: Registered to K8S. Inter-service calls use internal K8S domain names (see `RpcInfo`).

```json
"RegisterType": "Direct"
```

### 1.2 Basic

- **Basic Auth Info**: Used for inter-service authentication.
  - `UserName`: Username
  - `Password`: Password

```json
"Basic": {
    "UserName": "adnc",
    "Password": "yvMRER0wzSStw2Va0z59PNQd0lqeMYIP"
}
```

### 1.3 RpcInfo

- **RPC Configuration**:
  - `Polly:Enable`: Whether to enable Polly policies.
    - `true`: Enabled
    - `false`: Disabled
  - `Address`: Service address configuration.
    - `Service`: Service name.
    - `Direct`: Service address when `RegisterType = Direct`.
    - `Consul`: Service address when `RegisterType = Consul`.
    - `CoreDns`: Service address when `RegisterType = CoreDns`.

```json
"RpcInfo": {
    "Polly": {
        "Enable": false
    },
    "Address": [
        {
            "Service": "adnc-demo-admin-api",
            "Direct": "http://localhost:50010",
            "Consul": "http://adnc-demo-admin-api",
            "CoreDns": "http://adnc-demo-admin-api.default.svc.cluster.local"
        },
        {
            "Service": "adnc-demo-maint-api",
            "Direct": "http://localhost:50020",
            "Consul": "http://adnc-demo-maint-api",
            "CoreDns": "http://adnc-demo-maint-api.default.svc.cluster.local"
        },
        {
            "Service": "adnc-demo-cust-api",
            "Direct": "http://localhost:50030",
            "Consul": "http://adnc-demo-cust-api",
            "CoreDns": "http://adnc-demo-cust-api.default.svc.cluster.local"
        }
    ]
}
```

### 1.4 Redis

- **Redis Configuration**:
  - `Provider`: Client driver (currently only `StackExchange` is supported).
  - `EnableLogging`: Whether to enable logging for Redis operations.
  - `SerializerName`: Serialization method (`json`, `binary`, `proto`).
  - `EnableBloomFilter`: Whether to enable Bloom Filter support.
  - `Dbconfig:ConnectionString`: Redis connection string.

```json
"Redis": {
    "Provider": "StackExchange",
    "EnableLogging": true,
    "SerializerName": "json",
    "EnableBloomFilter": false,
    "Dbconfig": {
        "ConnectionString": "SERVER_IP:PORT,password=PASSWORD,defaultDatabase=0,ssl=false,sslHost=null,connectTimeout=4000,allowAdmin=true"
    }
}
```

### 1.5 Caching

- **Caching Configuration** (Depends on Redis):
  - `MaxRdSecond`: Maximum random seconds added to expiration time to prevent cache stampede.
  - `LockMs`: Distributed lock duration (milliseconds).
  - `SleepMs`: Sleep duration when failing to acquire a distributed lock (milliseconds).
  - `EnableLogging`: Whether to enable cache operation logs.
  - `PollyTimeoutSeconds`: Polly timeout for cache-database sync compensation mechanisms.
  - `PenetrationSetting`: Cache penetration protection settings.
    - `Disable`: Whether to disable protection.
    - `BloomFilterSetting`: Bloom Filter configuration.
      - `Name`: Name of the Bloom Filter.
      - `Capacity`: Capacity.
      - `ErrorRate`: False positive rate.

```json
"Caching": {
    "MaxRdSecond": 30,
    "LockMs": 6000,
    "SleepMs": 300,
    "EnableLogging": true,
    "PollyTimeoutSeconds": 11,
    "PenetrationSetting": {
        "Disable": true,
        "BloomFilterSetting": {
            "Name": "adnc:$SHORTNAME:bloomfilter:cachekeys",
            "Capacity": 10000000,
            "ErrorRate": 0.001
        }
    }
}
```

### 1.6 RabbitMQ

- **RabbitMQ Configuration**:
  - `HostName`: RabbitMQ host address.
  - `Port`: Port number.
  - `VirtualHost`: Virtual host name.
  - `UserName`: Username.
  - `Password`: Password.

```json
"RabbitMq": {
    "HostName": "SERVER_IP",
    "Port": "5672",
    "VirtualHost": "/",
    "UserName": "admin",
    "Password": "password"
}
```

### 1.7 SysLogDb

- **System Log Database**: Used for login/audit logs.
  - `DbType`: Database type (`mysql`, `sqlserver`, `oracle`).
  - `ConnectionString`: Database connection string.

```json
"SysLogDb": {
    "DbType": "mysql",
    "ConnectionString": "Server=SERVER_IP;Port=PORT;database=adnc_syslog;uid=USERNAME;pwd=PASSWORD;connection timeout=30;"
}
```

### 1.8 Consul

- **Consul Configuration**:
  - `ServiceName`: Service name placeholder.
  - `ServerTags`: Service tags.
  - `HealthCheckUrl`: Health check endpoint.
  - `HealthCheckIntervalInSecond`: Health check interval (seconds).
  - `DeregisterCriticalServiceAfter`: Time to deregister service after failure (seconds).
  - `Timeout`: Health check timeout (seconds).

```json
"Consul": {
    "ServiceName": "$SERVICENAME",
    "ServerTags": [ "urlprefix-/$SHORTNAME" ],
    "HealthCheckUrl": "$RELATIVEROOTPATH/health-CHECK_ID",
    "HealthCheckIntervalInSecond": 6,
    "DeregisterCriticalServiceAfter": 20,
    "Timeout": 6
}
```

### 1.9 Logging

- **Logging Configuration**:
  - `LogContainer`: Logging target (`console`, `file`, `loki`), corresponding to NLog config files in `adnc\src\Demo\Shared\resources\NLog`.
  - `LogLevel`: Log levels for different categories.
- **Loki**: Connection info for Loki (used if `Logging:LogContainer = loki`).

```json
"Logging": {
    "IncludeScopes": true,
    "LogContainer": "console",
    "LogLevel": {
        "Default": "Information",
        "Adnc": "Debug",
        "Microsoft": "Information"
    }
},
"Loki": {
    "Endpoint": "http://SERVER_IP:3100",
    "UserName": "",
    "Password": ""
}
```

### 1.10 CorsHosts

- **CorsHosts**: Allowed domains for CORS. `*` means all domains.

```json
"CorsHosts": "*"
```

### 1.11 JWT

- **JWT Configuration**: Settings for token creation and validation.

```json
"JWT": {
    "ValidateIssuer": true,
    "ValidIssuer": "adnc",
    "ValidateIssuerSigningKey": true,
    "SymmetricSecurityKey": "alphadotnetcoresecurity_KEY",
    "ValidateAudience": true,
    "ValidAudience": "manager",
    "ValidateLifetime": true,
    "RequireExpirationTime": true,
    "ClockSkew": 1,
    "RefreshTokenAudience": "manager",
    "Expire": 6000,
    "RefreshTokenExpire": 10080
}
```

### 1.12 SkyWalking

- **SkyWalking Tracing Configuration**:
  - `ServiceName`: Registered service name for tracing.

```json
"SkyWalking": {
    "ServiceName": "$SERVICENAME",
    "Namespace": "adnc",
    "HeaderVersions": [ "sw8" ],
    "Sampling": {
        "SamplePer3Secs": -1,
        "Percentage": -1.0,
        "IgnorePaths": [ "/*/health-*", "**/appsettings", "**/swagger.json", "**/loki/api/v1/push" ]
    },
    "Logging": {
        "Level": "Error",
        "FilePath": "txtlogs\\skyapm-{Date}.log"
    },
    "Transport": {
        "Interval": 3000,
        "ProtocolVersion": "v8",
        "QueueSize": 30000,
        "BatchSize": 3000,
        "gRPC": {
            "Servers": "SERVER_IP:11800",
            "Timeout": 10000,
            "ConnectTimeout": 10000,
            "ReportTimeout": 600000,
            "Authentication": ""
        }
    }
}
```

## 2. Service-Specific Configuration

> Examples for `Admin.Api` in Development and Production:
> `adnc\src\Demo\Admin.Api\appsettings.Development.json`
> `adnc\src\Demo\Admin.Api\appsettings.Production.json`

### 2.1 `appsettings.Development.json`

- `ConfigurationType`:
  - `File`: Loads `appsettings.shared.Development.json` on startup.
- `Mysql`: Business database connection string.
- `Kestrel`: Service URL and port configuration.

```json
{
    "ConfigurationType": "File",
    "Mysql": {
        "ConnectionString": "Server=127.0.0.1;Port=PORT;database=adnc_admin;uid=root;pwd=PASSWORD;connection timeout=30;"
    },
    "Kestrel": {
        "Endpoints": {
            "Default": { "Url": "http://0.0.0.0:50010" },
            "Grpc": { "Url": "http://0.0.0.0:50011", "Protocols": "Http2" }
        }
    }
}
```

### 2.2 `appsettings.Production.json`

- `ConfigurationType`:
  - `Consul`: Fetches configuration from Consul on startup.
- `Consul`: Connection info for fetching configurations.

```json
{
  "ConfigurationType": "Consul",
  "Consul": {
    "ConsulUrl": "http://SERVER_IP:8500",
    "ConsulKeyPath": "adnc/production/shared/appsettings,adnc/production/$SHORTNAME/appsettings"
  }
}
```

#### 2.3 Configuration Loading Code

```csharp
public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder, IServiceInfo serviceInfo)
{
    var configurationType = builder.Configuration.GetValue<string>(NodeConsts.ConfigurationType) ?? ConfigurationTypeConsts.File;
    switch (configurationType)
    {
        case ConfigurationTypeConsts.File:
            builder.Configuration.AddJsonFile($"{AppContext.BaseDirectory}/appsettings.shared.{builder.Environment.EnvironmentName}.json", true, true);
            break;
        case ConfigurationTypeConsts.Consul:
            var consulOption = builder.Configuration.GetSection(NodeConsts.Consul).Get<ConsulOptions>();
            if (consulOption is null || consulOption.ConsulKeyPath.IsNullOrWhiteSpace())
            {
                throw new NotImplementedException(NodeConsts.Consul);
            }
            else
            {
                consulOption.ConsulKeyPath = consulOption.ConsulKeyPath.Replace("$SHORTNAME", serviceInfo.ShortName);
                builder.Configuration.AddConsulConfiguration(consulOption, true);
            }
            break;
        // ...
    }
    return builder;
}
```

---

*If this helps, please Star & Fork.*
