# ADNC Repository Usage - Switching Database Types

[GitHub repository](https://github.com/alphayu/adnc)

This article explains how to switch the database type used by the `ADNC` repository layer. In principle, ADNC can switch smoothly as long as the database type is supported by EF Core. ADNC uses MariaDB/MySQL by default. This article uses switching from the default database type to SQL Server as an example; the steps for other database types are similar.

There are usually two database switching scenarios:

1. Global switching: The database type of all services is switched to another type, such as from MySQL to SQL Server.
2. Partial switching: Different services use different database types, such as service A using MySQL, service B using SQL Server, and service C using Oracle. ADNC supports different database types per service.

## Global Switch

Global switching requires adjusting three files. The following example switches the default MySQL provider to SQL Server.

- `AbstractApplicationDependencyRegistrar.Repositories.cs`

```csharp
/*project:Adnc.Shared.Application*/
namespace Adnc.Shared.Application.Registrar;

public abstract partial class AbstractApplicationDependencyRegistrar 
{
	/// <summary>
    /// Registers EFCoreContext.
    /// </summary>
        protected virtual void AddEfCoreContext()
    {
        AddOperater(_services);

        var connectionString = _configuration[NodeConsts.SqlServer_ConnectionString] ?? throw new InvalidDataException("SqlServer ConnectionString is null");
        var migrationsAssemblyName = _serviceInfo.MigrationsAssemblyName;
        _services.AddAdncInfraEfCoreSQLServer(RepositoryOrDomainLayerAssembly, optionsBuilder =>
        {
            optionsBuilder.UseLowerCaseNamingConvention();
            optionsBuilder.UseSqlServer(connectionString, optionsBuilder =>
            {
                optionsBuilder.MinBatchSize(4)
                                        .MigrationsAssembly(migrationsAssemblyName)
                                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        }, _lifetime);
    }
}
```
- `AbstractApplicationDependencyRegistrar.EventBus.cs`

  > option.UseMySQL => option.UseSqlServer

```csharp
/*project:Adnc.Shared.Application*/
namespace Adnc.Shared.Application.Registrar;

public abstract partial class AbstractApplicationDependencyRegistrar
{
    protected virtual void AddCapEventBus(IEnumerable<Type> subscribers, Action<FailedInfo>? failedThresholdCallback = null)
    {
        ArgumentNullException.ThrowIfNull(subscribers, nameof(subscribers));
        Checker.Argument.ThrowIfNullOrCountLEZero(subscribers, nameof(subscribers));

        var connectionString = Configuration.GetValue<string>(NodeConsts.SqlServer_ConnectionString) ?? throw new InvalidDataException("SqlServer ConnectionString is null");
        var rabbitMQOptions = Configuration.GetRequiredSection(NodeConsts.RabbitMq).Get<RabbitMQOptions>() ?? throw new InvalidDataException(nameof(RabbitMQOptions));
        var clientProvidedName = ServiceInfo.Id;
        var version = ServiceInfo.Version;
        var groupName = $"cap.{ServiceInfo.ShortName}.{this.GetEnvShortName()}";
        Services.AddAdncInfraCap(subscribers, capOptions =>
                                 {
                                    SetCapBasicInfo(capOptions, version, groupName,failedThresholdCallback);
                                    SetCapRabbitMQInfo(capOptions, rabbitMQOptions, clientProvidedName);
                                    // Requires a reference to DotNetCore.CAP.SqlServer.
                                    option.UseSqlServer(config =>
                                    {
                                        config.ConnectionString = connectionString;
                                        config.Schema = "cap";
                                    });
                                 }, null, Lifetime);
    }
}
```
- `appsettings.Development.json`

> Delete the MySql node and add the SqlServer node.

```json
  "SqlServer": {
    "ConnectionString": "Data Source=114.132.157.111;Initial Catalog=adnc_xxx_dev;User Id=sa;Password=xxx;"
  },
```

- Remove the `Adnc.Infra.Repository.EfCore.MySQL.csproj` reference from the project file and reference `Adnc.Infra.Repository.EfCore.SqlServer.csproj`.

## Partial Switch

Partial switching is relatively simple. You only need to override `AddCapEventBus` and `AddEfCoreContext`. The following example uses the whse service.

1. The `Adnc.Whse.Application` project references `Adnc.Infra.Repository.EfCore.SqlServer.csproj`.

```csharp
namespace Adnc.Whse.Application.Registrar;

public sealed class DependencyRegistrar(IServiceCollection services, IServiceInfo serviceInfo, IConfiguration configuration, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    : AbstractApplicationDependencyRegistrar(services, serviceInfo, configuration, lifetime)
{        
    protected override void AddCapEventBus(IEnumerable<Type> subscribers, Action<FailedInfo>? failedThresholdCallback = null)
    {
        var connectionString = _configuration[NodeConsts.SqlServer_ConnectionString] ?? throw new InvalidDataException("SqlServer ConnectionString is null");
        var rabbitMQOptions = _configuration.GetRequiredSection(NodeConsts.RabbitMq).Get<RabbitMQOptions>() ?? throw new InvalidDataException(nameof(NodeConsts.RabbitMq));
        var clientProvidedName = _serviceInfo.Id;
        var version = _serviceInfo.Version;
        var groupName = $"cap.{_serviceInfo.ShortName}.{this.GetEnvShortName()}";
        _services.AddAdncInfraCap(subscribers, capOptions =>
        {
            SetCapBasicInfo(capOptions, version, groupName, failedThresholdCallback);
            SetCapRabbitMQInfo(capOptions, rabbitMQOptions, clientProvidedName);
            capOptions.UseSqlServer(sqlServerOptions =>
            {
                sqlServerOptions.ConnectionString = connectionString;
                sqlServerOptions.Schema = "cap";
            });
        }, null, _lifetime);
    }

    protected override void AddEfCoreContext()
    {
        AddOperater(_services);

        var connectionString = _configuration[NodeConsts.SqlServer_ConnectionString] ?? throw new InvalidDataException("SqlServer ConnectionString is null");
        var migrationsAssemblyName = _serviceInfo.MigrationsAssemblyName;
        _services.AddAdncInfraEfCoreSQLServer(RepositoryOrDomainLayerAssembly, optionsBuilder =>
        {
            optionsBuilder.UseLowerCaseNamingConvention();
            optionsBuilder.UseSqlServer(connectionString, optionsBuilder =>
            {
                optionsBuilder.MinBatchSize(4)
                                        .MigrationsAssembly(migrationsAssemblyName)
                                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        }, _lifetime);
    }
}        
```

2. The `Adnc.Whse.Migrations` project references `Adnc.Infra.Repository.EfCore.SqlServer.csproj` and removes the `Adnc.Infra.Repository.EfCore.MySQL.csproj` reference.

3. Register the SQL Server health check in `DependencyRegistrar.cs`.

```csharp
namespace Adnc.Whse.WebApi.Registrar;

public sealed class DependencyRegistrar(IServiceCollection services, IServiceInfo serviceInfo, IConfiguration configuration) : AbstractWebApiDependencyRegistrar(services, serviceInfo, configuration)
{
    public override void AddAdncServices()
    {
        _services.AddHealthChecks(checksBuilder =>
        {
            checksBuilder
                    .AddSqlServer(connectionString) // sqlserver 
                    .AddRedis(redisSecton)
                    .AddRabbitMQ(rabbitSecton, clientProvidedName);
        });
    }
}
```

- `appsettings.Development.json`

> Delete the MySql node and add the SqlServer node.

```json
  "SqlServer": {
    "ConnectionString": "Data Source=114.132.157.111;Initial Catalog=adnc_xxx_dev;User Id=sa;Password=xxx;"
  },
```
---
If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
