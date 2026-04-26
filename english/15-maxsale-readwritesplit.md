# ADNC Repository Usage: Read/Write Splitting

[GitHub Repository](https://github.com/alphayu/adnc)

When a single-database system encounters performance bottlenecks, Read/Write splitting is often the first optimization strategy. In most systems, read operations far outnumber writes. High-latency read requests can lock tables, impacting write performance. Therefore, offloading reads to replicas is crucial.

While common EF Core solutions involve `DbContextFactory` or dynamic connection string switching, these methods become complex when dealing with replica monitoring, automatic failover, load balancing, or routing specific SQL to specific replicas. Database middleware offers a more elegant and stable solution.

## Why Use Database Middleware?

- **Transparency**: Clients (C# code) don't need to know the deployment details of the backend. Failover and migrations are handled by the middleware, allowing the business logic to remain focused.
- **Scalability**: Middleware handles shared concerns like data aggregation, transactions across shards, and source switching, which reduces complexity in the application layer.
- **Enterprise Standard**: Major cloud providers and enterprises use middleware for database management.

## Mainstream Middleware Overview

- **Mycat**: Java-based, feature-rich, supports sharding across multiple database types.
- **ShardingSphere-Proxy**: Part of the Apache project, excellent for MySQL/PostgreSQL.
- **ProxySQL**: C++-based, high performance, flexible routing rules for MySQL.
- **MaxScale**: Developed by MariaDB, optimized for MariaDB/MySQL with advanced high-availability features.

## Why ADNC Chooses MaxScale

- **Native Compatibility**: Developed by MariaDB for MariaDB, ensuring the best compatibility.
- **Rich Documentation**: Excellent version control and documentation.
- **High Availability**: Comprehensive cluster HA configurations.
- **Flexible Filters**: Supports `HintFilter`, `NamedServerFilter`, and `TopFilter`.
- **Performance**: High efficiency for microservices which are already sharded by service.

## Implementing MaxScale Hints in EF Core

ADNC uses EF Core's `TagWith` method to inject hints into generated SQL. These hints tell MaxScale which replica to route the query to.

```csharp
public static class RepositoryConsts
{
    public static readonly string MAXSCALE_ROUTE_TO_MASTER = "maxscale route to master";
}

public abstract class BaseRepository<TDbContext, TEntity> : IEfRepository<TEntity>
{
    public virtual IQueryable<TEntity> GetAll(bool writeDb = false)
    {
        var query = DbContext.Set<TEntity>().AsNoTracking();
        if (writeDb)
            // Route read operation to master
            return query.TagWith(RepositoryConsts.MAXSCALE_ROUTE_TO_MASTER);
        return query;
    }

    public virtual async Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, ..., bool writeDb = false)
    {
        if (writeDb)
            // Manual hint injection for Dapper queries
            sql = $"/* {RepositoryConsts.MAXSCALE_ROUTE_TO_MASTER} */ {sql}";
        return await DbContext.Database.GetDbConnection().QueryAsync<TResult>(sql, ...);
    }
}
```

## Implementing Mycat Hints in EF Core

Mycat requires hints in the format `/*#mycat:db_type=master*/`. Since `TagWith` generates `--` comments, a custom `IQuerySqlGeneratorFactory` is required to wrap the tag in `/* */`.

1. Create a custom `AdncMySqlQuerySqlGeneratorFactory`.
2. Implement `AdncQuerySqlGenerator` to override SQL generation logic.
3. Replace the default service during `DbContext` registration:

```csharp
options.ReplaceService<IQuerySqlGeneratorFactory, AdncMySqlQuerySqlGeneratorFactory>();
```

---
*If this helps, please Star & Fork.*
