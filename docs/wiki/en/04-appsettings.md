# ADNC configuration node details

[GitHub repository](https://github.com/alphayu/adnc)

## 1. Service public configuration

`adnc\src\Demo\Shared\resources\appsettings.shared.Development.json`

## 1.1 RegisterType

- Service registration type:
  
- `Direct`: No registration is performed, calls between services are through URI addresses, see`RpcInfo`node for details.
  
- `Consul`: registered to Consul, calls between services are through service names, see`RpcInfo`node for details.
  
- `CoreDns`: registered to K8S, calls between services are made through the K8S internal domain name, see`RpcInfo`node for details.

```json
"RegisterType": "Direct"
```

## 1.2 Basic

- `Basic`authentication information: calls between services use`Basic`authentication
  
- `UserName`: Username
  
- `Password`: Password

```json
"Basic": {
    "UserName": "adnc",
    "Password": "yvMRER0wzSStw2Va0z59PNQd0lqeMYIP"
}
```

## 1.3 RpcInfo

- Service call configuration information:
  
- `Polly:Enable`: Whether to enable the`Polly`policy
  
- `true`: enabled
  
- `false`: Disabled
  
- `Address`: Service address configuration
  
- `Service`: Service name
  
- `Direct`: Service address at`RegisterType = Direct`
- `Consul`: Service address at`RegisterType = Consul`
- `CoreDns`: Service address at`RegisterType = CoreDns`

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

## 1.4 Redis

- `Redis`: cache database configuration information
  
- `Provider`: client driver, currently only supports`StackExchange`
- `EnableLogging`: Whether to enable logging
  
- `SerializerName`: serialization mode, supports`json`,`binary`,`proto`
- `EnableBloomFilter`: Whether to enable bloom filter
  
- `Dbconfig:ConnectionString`: Redis connection string

```json
"Redis": {
    "Provider": "StackExchange",
    "EnableLogging": true,
    "SerializerName": "json",
    "EnableBloomFilter": false,
    "Dbconfig": {
        "ConnectionString": "62.234.187.128:13379,password=football,defaultDatabase=0,ssl=false,sslHost=null,connectTimeout=4000,allowAdmin=true"
    }
}
```

## 1.5 Caching

- `Caching`: Cache related configuration information (depends on Redis implementation)
  
- `MaxRdSecond`: The maximum random number of seconds to increase the cache expiration time, used to prevent cache avalanches
  
- `LockMs`: distributed lock duration (milliseconds)
  
- `SleepMs`: Sleep duration (milliseconds) when the distributed lock is not acquired
  
- `EnableLogging`: Whether to enable cache operation logs
  
- `PollyTimeoutSeconds`: Polly timeout, used for cache and database synchronization compensation mechanism
  
- `PenetrationSetting`: Cache penetration protection related configurations
  
- `Disable`: Whether to disable penetration protection
  
- `BloomFilterSetting`: Bloom filter configuration
  
- `Name`: Bloom filter name
  
- `Capacity`: Capacity
  
- `ErrorRate`: False positive rate

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

## 1.6 RabbitMQ

- `RabbitMQ`: Message queue configuration information
  
- `HostName`: RabbitMQ host address
  
- `Port`: port number
  
- `VirtualHost`: virtual host name
  
- `UserName`: Username
  
- `Password`: Password

```json
"RabbitMq": {
    "HostName": "62.234.187.128",
    "Port": "5672",
    "VirtualHost": "/",
    "UserName": "admin",
    "Password": "football"
}
```

## 1.7 SysLogDb

- `SysLogDb`: Login/audit log database configuration information
  
- `DbType`: database type, supports`mysql`,`sqlserver`,`oracle`
- `ConnectionString`: Database connection string

```json
"SysLogDb": {
    "DbType": "mysql",
    "ConnectionString": "Server=62.234.187.128;Port=13308;database=adnc_syslog;uid=root;pwd=alpha.netcore;connection timeout=30;"
}
```

## 1.8 Consul

- `Consul`: Configuration information related to service registration and discovery
  
- `ServiceName`: Service name placeholder
  
- `ServerTags`: Service tag
  
- `HealthCheckUrl`: Health check address
  
- `HealthCheckIntervalInSecond`: Health check interval (seconds)
  
- `DeregisterCriticalServiceAfter`: Time to log out of the service after failing the health check (seconds)
  
- `Timeout`: Timeout (seconds)

```json
"Consul": {
    "ServiceName": "$SERVICENAME",
    "ServerTags": [ "urlprefix-/$SHORTNAME" ],
    "HealthCheckUrl": "$RELATIVEROOTPATH/health-24b01005-a76a-4b3b-8fb1-5e0f2e9564fb",
    "HealthCheckIntervalInSecond": 6,
    "DeregisterCriticalServiceAfter": 20,
    "Timeout": 6
}
```

## 1.9 Logging

- `Logging`: Log related configuration information
  
- `LogContainer`: Logging mode, supports`console`,`file`,`loki`, corresponding to the configuration files in the`adnc\src\Demo\Shared\resources\NLog`directory respectively
  
- `LogLevel`: Log level configuration
- `Loki`: When`Logging:LogContainer = loki`, read Loki connection information from this node

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
    "Endpoint": "http://10.2.8.5:3100",
    "UserName": "",
    "Password": ""
}
```

## 1.10 CorsHosts

- `CorsHosts`: Browser cross-domain domain name configuration information,`*`indicates that all domain names are allowed to access

```json
"CorsHosts": "*"
```

## 1.11 JWT

- `JWT`: Token creation and authentication related configuration information

```json
"JWT": {
    "ValidateIssuer": true,
    "ValidIssuer": "adnc",
    "ValidateIssuerSigningKey": true,
    "SymmetricSecurityKey": "alphadotnetcoresecurity24b010055e0f2e9564fb",
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

## 1.12 SkyWalking

- `SkyWalking`: Link tracking client configuration information
  
- `ServiceName`: Service name

```json
"SkyWalking": {
    "ServiceName": "$SERVICENAME",
    "Namespace": "adnc",
    "HeaderVersions": [
        "sw8"
    ],
    "Sampling": {
        "SamplePer3Secs": -1,
        "Percentage": -1.0,
        "IgnorePaths": [ "/*/health-24b01005-a76a-4b3b-8fb1-5e0f2e9564fb", "http://**/appsettings", "/**/swagger.json", "http://**/loki/api/v1/push" ]
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
            "Servers": "62.234.187.128:11800",
            "Timeout": 10000,
            "ConnectTimeout": 10000,
            "ReportTimeout": 600000,
            "Authentication": ""
        }
    }
}
```

2. Single service exclusive configuration> The following takes the development environment and production environment configuration of the Admin.Api service as an example:
> adnc\src\Demo\Admin.Api\appsettings.Development.json
> adnc\src\Demo\Admin.Api\appsettings.Production.json

### 2.1 `appsettings.Development.json`

- `ConfigurationType`: Configuration file type
  
- `File`: Load`appsettings.shared.Development.json`when the service starts
- `MySQL`: Business database connection information
- `Kestrel`: Service URL and port configuration information

```json
{
    "ConfigurationType": "File",
    "MySQL": {
        "ConnectionString": "Server=127.0.0.1;Port=13308;database=adnc_admin;uid=root;pwd=alpha.netcore;connection timeout=30;"
    },
    "Kestrel": {
        "Endpoints": {
            "Default": {
                "Url": "http://0.0.0.0:50010"
            },
            "gRPC": {
                "Url": "http://0.0.0.0:50011",
                "Protocols": "Http2"
            }
        }
    }
}
```

## 2.2 `appsettings.Production.json`

- `ConfigurationType`: Configuration file type
  
- `Consul`: Get configuration from Consul when service starts
- `Consul`: Consul related configuration information
  
- `ConsulUrl`: Consul service address
  
- `ConsulKeyPath`: Configuration information Key path

```json
{
  "ConfigurationType": "Consul",
  "Consul": {
    "ConsulUrl": "http://10.2.8.5:8500",
    "ConsulKeyPath": "adnc/production/shared/appsettings,adnc/production/$SHORTNAME/appsettings"
  }
}
```

### 2.3 Obtain configuration related code

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
        case ConfigurationTypeConsts.Nacos:
            throw new NotImplementedException(nameof(ConfigurationTypeConsts.Nacos));
        case ConfigurationTypeConsts.Etcd:
            throw new NotImplementedException(nameof(ConfigurationTypeConsts.Etcd));
        default:
            throw new NotImplementedException(nameof(configurationType));
    }
    
    // ....

    return builder;
}
```

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
