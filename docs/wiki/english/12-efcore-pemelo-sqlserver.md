# ADNC Repository Usage: Switching Database Types

[GitHub Repository](https://github.com/alphayu/adnc)

This article describes how to switch database types in ADNC. Theoretically, ADNC supports any database supported by EF Core. While ADNC defaults to MariaDB/MySQL, this guide uses switching to **SQL Server** as an example.

## Scenarios
1. **Global Switch**: All services switch to a new database type.
2. **Partial Switch**: Different services use different database types (e.g., Service A uses MySQL, Service B uses SQL Server).

---

## Global Switch

To switch globally, you need to modify three main files (example: MySql to SqlServer):

### 1. `AbstractApplicationDependencyRegistrar.Repositories.cs`

Update the `AddEfCoreContext` method to use the SQL Server provider:

```csharp
protected virtual void AddEfCoreContext()
{
    AddOperater(_services);
    var connectionString = _configuration[NodeConsts.SqlServer_ConnectionString];
    var migrationsAssemblyName = _serviceInfo.MigrationsAssemblyName;

    _services.AddAdncInfraEfCoreSQLServer(RepositoryOrDomainLayerAssembly, optionsBuilder =>
    {
        optionsBuilder.UseLowerCaseNamingConvention();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(migrationsAssemblyName);
        });
    }, _lifetime);
}
```

### 2. `AbstractApplicationDependencyRegistrar.EventBus.cs`

Update the CAP configuration to use SQL Server:

```csharp
protected virtual void AddCapEventBus(...)
{
    // ...
    Services.AddAdncInfraCap(subscribers, capOptions =>
    {
        // ...
        capOptions.UseSqlServer(config =>
        {
            config.ConnectionString = connectionString;
            config.Schema = "cap";
        });
    }, null, Lifetime);
}
```

### 3. `appsettings.Development.json`

Replace the `MySql` node with `SqlServer`:

```json
"SqlServer": {
    "ConnectionString": "Data Source=SERVER;Initial Catalog=adnc_db;User Id=sa;Password=pwd;"
}
```

### 4. Project References
Remove `Adnc.Infra.Repository.EfCore.Mysql` and add `Adnc.Infra.Repository.EfCore.SqlServer`.

---

## Partial Switch

For a partial switch (e.g., only the Warehouse service uses SQL Server), override the relevant methods in the specific service's `DependencyRegistrar`:

1. Add the SQL Server infrastructure reference to the specific service project.
2. Override `AddEfCoreContext` and `AddCapEventBus` in the service's `DependencyRegistrar.cs`.
3. Update the health check registration to use SQL Server.

```csharp
public override void AddAdncServices()
{
    _services.AddHealthChecks()
        .AddSqlServer(connectionString)
        .AddRedis(...)
        .AddRabbitMQ(...);
}
```

---
*If this helps, please Star & Fork.*
