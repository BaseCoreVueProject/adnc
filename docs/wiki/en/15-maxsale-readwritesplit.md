# ADNC Repository Usage - read/write splitting

[GitHub repository](https://github.com/alphayu/adnc)

When a single database system encounters a performance bottleneck, read-write separation is usually one of the preferred optimization methods. In most systems, there are far more read operations than write operations, and a large number of time-consuming read requests can easily lead to table locks, which in turn affects writes. Therefore, the read/write splitting is particularly important.Regarding how to achieve read-write separation for`EF Core`, common solutions include registering`DbContextFactory`, injecting`ReadDbContext`and`WriteDbContext`respectively, or dynamically switching the database connection string.Although these methods can achieve basic read-write separation, if it is necessary to support slave database status monitoring (such as slave database downtime), automatic master-backup switching, slave database load balancing, routing specific SQL to designated slave databases, some tables without read-write separation (such as basic data tables) and other advanced features, or if sharding (sharding, sharding) and distributed transactions are involved in the future, it will be extremely complex and difficult to ensure stability by relying on business code alone.Is there a more elegant solution? Database middleware may be a better choice.`EF Core`can also elegantly achieve read/write splitting through middleware.

## Why use middleware?
- Direct client connection (implemented in code) is used to separate reading and writing. Although the middleware forwarding link is omitted, the query performance may be slightly improved, but you need to understand the back-end deployment details. In scenarios such as active/standby switching and database migration, the client needs to sense and adjust the database connection information. If forwarded through middleware, the client does not need to care about back-end details, connection maintenance, and master-slave switching. All are automatically handled by the middleware, and the business side can focus on business logic development.

- In most production environments, the database is the performance bottleneck. Separating read and write is usually the first optimization method; if it still cannot meet the demand, sharding (sharding of databases and tables) is required. After data is sharded, applications need to process multiple data sources; without database middleware, applications need to deal with issues such as data source switching, transaction processing, and data aggregation on their own, which makes it easy to repeatedly implement common capabilities and significantly increases development complexity. After the introduction of database middleware, applications can focus on business processing, and general data aggregation, transactions, data source switching, etc. are left to the middleware.
- Mainstream domestic manufacturers and cloud platforms all have self-developed database middleware.
# Introduction to mainstream free open source middleware
Currently, the mature, free and continuously maintained database middleware in the community include:- Mycat
- ShardingSphere-Proxy
- ProxySQL
- MaxScale

## Mycat
- Official website:http://www.mycat.org.cn/- Development language:`Java`- Whether to support sharding: Yes
- Supported databases: MySQL/MariaDB, Oracle, DB2, SQL Server, PostgreSQL
- Routing rules: All SQLs wrapped in transactions go to the write library. When there is no transaction package, you can set the read-write library through Hint, and other functions are implemented through configuration.
- Introduction: Mycat was separated and reconstructed from Alibaba Cobar in 2013, and is continuously updated. In 2015, there were applications for telecommunications and bank-level customers. Mycat is the one that supports the most database types and has the most complete functions among the four middlewares. It is recommended to read<a href="ZXQ0002QXZ target="_blank" rel="noopener">Mycat authoritative guide</a>to understand the rules, advantages and disadvantages of master-slave replication, database and table sharding. It should be noted that official documentation, Wiki and mycat-web will be updated slowly after 2016; if the documentation and supporting tools can continue to be improved, Mycat will be the first choice.

### ShardingSphere-Proxy
- Official website:http://shardingsphere.apache.org/index_zh.html- Development language:`Java`- Whether to support sharding: Yes
- Supported databases: MySQL/MariaDB, PostgreSQL
- Routing rules: If a write operation occurs in the same thread and the same database connection, subsequent read operations will also be routed to the write library; routing can be forcibly specified through Hint, and other functions are implemented through configuration.
- Introduction: ShardingSphere provides a variety of products, and ShardingSphere-Proxy is recommended for.NET scenarios. The project was open sourced by Dangdang, with JD.com deeply involved, and became an Apache top-level project in April 

2020. The relevant documents are mainly official content, with relatively few cases, so we will continue to pay attention to them in the future.

### ProxySQL
- Official website:https://proxysql.com/- Development language:`C++`- Whether to support sharding: Yes
- Supported databases: MySQL/MariaDB
- Introduction: ProxySQL is a mature MySQL/MariaDB database middleware with detailed official website documentation and rich cases. Its routing rules are flexible and can be customized based on users, schema or a single SQL statement. It also supports specifying routes through Hint and routing rules. It is a choice worth considering.

### MaxScale
- Official website:https://mariadb.com/kb/en/maxscale/- Development language:`C`- Whether to support sharding: Not supported
- Supported databases: MySQL/MariaDB
- Routing rules: All SQLs wrapped in transactions go to the write library. When there is no transaction package, you can set the read-write library through Hint, and other functions are implemented through configuration.
- Introduction: MaxScale is officially developed by MariaDB, with mature functions, detailed documentation, and rich cases. It provides a variety of filters, such as HintFilter, NamedServerFilter (can route all tables to the write library), TopFilter (can route the slowest N queries to the specified read library), etc. Its high availability configuration is relatively complete among similar products. Please refer to the official documentation for details.In summary, the four middlewares all have flexible routing rules and rich configuration options, which can achieve efficient read-write separation instead of simple master-slave switching. They all support Hint syntax and backend database monitoring. For the code side, you only need to set the SQL Hint, and the rest is handled by the middleware.> Hint, as a supplementary syntax for SQL, plays an important role in relational databases. It allows users to influence the way SQL is executed through specific syntax to achieve special optimizations.

>
> For example, maxscale specifies the Hint usage of the read-write library:
>

```sql
> -- maxscale route to master
> SELECT * FROM table1;
> ```

## Reasons why Adnc chose MaxScale
- Adnc backend database is MariaDB, Maxscale is officially developed by MariaDB and has the best compatibility
- Maxscale has detailed official documentation, rich network cases, and standardized version control.
- Maxscale provides the richest high-availability configuration for database clusters
- Maxscale routing rules are flexible and support multiple custom filters
- Maxscale has excellent performance when paired with MariaDB (partly because it does not support sharding of databases/tables)
- Maxscale does not support sharding of databases and tables, but Adnc, as a microservice framework, has naturally implemented sharding of databases by service. The demand for sub-tables cannot be met for the time being, but over-design is not the best practice. Suitability for your own scenario is the most important.
# EF Core generates the implementation of MaxScale Hint
To achieve read-write separation, you need to deploy a database cluster based on the maxscale middleware, and you also need to install maxscale.For details on EF Core’s`TagWith`method, see[Official documentation](https://docs.microsoft.com/en-us/ef/core/querying/tags).

```csharp
public static class RepositoryConsts
{
    public static readonly string MYCAT_ROUTE_TO_MASTER = "#mycat:db_type=master";
    public static readonly string MAXSCALE_ROUTE_TO_MASTER = "maxscale route to master";
}

public abstract class BaseRepository<TDbContext, TEntity> : IEfRepository<TEntity>
       where TDbContext : DbContext
       where TEntity : EfEntity
{
    public virtual IQueryable<TrdEntity> GetAll<TrdEntity>(bool writeDb = false) where TrdEntity : EfEntity
    {
        var dbSet = DbContext.Set<TrdEntity>().AsNoTracking();
        if (writeDb)
            // Route read operations to the write database.
            return dbSet.TagWith(RepositoryConsts.MAXSCALE_ROUTE_TO_MASTER);
        return dbSet;
    }

    public virtual async Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null, bool writeDb = false)
    {
        if (writeDb)
            // Integrate Dapper for complex queries, with read operations routed to the write database.
            sql = string.Concat("/* ", RepositoryConsts.MAXSCALE_ROUTE_TO_MASTER, " */", sql);
        return await DbContext.Database.GetDbConnection().QueryAsync<TResult>(sql, param, null, commandTimeout, commandType);
    }
}
```
The code implementation based on maxscale is as above. The database connection string is consistent with the direct connection to the database. You only need to change the port to the maxscale port.

## EF Core generates the implementation of Mycat Hint
The following describes how to generate mycat Hint. It also requires deploying a database cluster first and based on mycat middleware.The Mycat Hint generated by EF Core is slightly complicated. The Hint generated by the`TagWith`method is as follows:

```sql
-- #mycat:db_type=master
SELECT * FROM TABLE1
```

The format required by Mycat is:

```sql
/*#mycat:db_type=master*/
SELECT * FROM TABLE1
```
Taking`Pomelo.EntityFrameworkCore.MySql`as an example, EFCore provides the`IQuerySqlGeneratorFactory`interface,`MySqlQuerySqlGeneratorFactory`of`Pomelo`implements this interface, and the`Create()`method is responsible for generating the specific`QuerySqlGenerator`, that is, the generation of query SQL.There are three main steps to complete:
- Create a new factory class`AdncMySqlQuerySqlGeneratorFactory`, inherit`MySqlQuerySqlGeneratorFactory`and override the`Create()`method. The sample code is as follows

```csharp
namespace Pomelo.EntityFrameworkCore.MySql.Query.ExpressionVisitors.Internal
{
    /// <summary>
    /// ADNC SQL generator factory.
    /// </summary>
    public class AdncMySqlQuerySqlGeneratorFactory : MySqlQuerySqlGeneratorFactory
    {
        private readonly QuerySqlGeneratorDependencies _dependencies;
        private readonly MySqlSqlExpressionFactory _sqlExpressionFactory;
        private readonly IMySqlOptions _options;

        public AdncMySqlQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            ISqlExpressionFactory sqlExpressionFactory,
            IMySqlOptions options) : base(dependencies, sqlExpressionFactory, options)
        {
            _dependencies = dependencies;
            _sqlExpressionFactory = (MySqlSqlExpressionFactory)sqlExpressionFactory;
            _options = options;
        }

        /// <summary>
        /// Overrides QuerySqlGenerator.
        /// </summary>
        /// <returns></returns>
        public override QuerySqlGenerator Create()
        {
            var result = new AdncQuerySqlGenerator(_dependencies, _sqlExpressionFactory, _options);
            return result;
        }
    }
}
```

- Create a new Sql generated class`AdncQuerySqlGenerator`that inherits`QuerySqlGenerator`and overrides two methods.

```csharp
namespace Pomelo.EntityFrameworkCore.MySql.Query.ExpressionVisitors.Internal
{
    /// <summary>
    /// ADNC SQL generator.
    /// </summary>
    public class AdncQuerySqlGenerator : MySqlQuerySqlGenerator
    {
        protected readonly Guid ContextId;
        private bool _isQueryMaseter = false;

        public AdncQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] MySqlSqlExpressionFactory sqlExpressionFactory,
            [CanBeNull] IMySqlOptions options)
            : base(dependencies, sqlExpressionFactory, options)
        {
            ContextId = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the tags from IQueryable.
        /// </summary>
        /// <param name="selectExpression"></param>
        protected override void GenerateTagsHeaderComment(SelectExpression selectExpression)
        {
            if (selectExpression.Tags.Contains(EfCoreConsts.MyCAT_ROUTE_TO_MASTER))
            {
                _isQueryMaseter = true;
                selectExpression.Tags.Remove(EfCoreConsts.MyCAT_ROUTE_TO_MASTER);
            }
            base.GenerateTagsHeaderComment(selectExpression);
        }

        /// <summary>
        /// The final SQL generated by Pomelo.
        /// This method is mainly used for debugging.
        /// </summary>
        /// <param name="selectExpression"></param>
        /// <returns></returns>
        public override IRelationalCommand GetCommand(SelectExpression selectExpression)
        {
            var command = base.GetCommand(selectExpression);
            return command;
        }

        /// <summary>
        /// Inserts the MyCAT annotation before Pomelo generates the query SQL.
        /// The annotation means reading data from the write database.
        /// </summary>
        /// <param name="selectExpression"></param>
        /// <returns></returns>
        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            if (_isQueryMaseter)
                Sql.Append(string.Concat("/*", EfCoreConsts.MyCAT_ROUTE_TO_MASTER, "*/ "));

            return base.VisitSelect(selectExpression);
        }
    }
}
```
- Replace the SQL generation factory of`Pomelo`when registering DbContext

```csharp
/// <summary>
/// Registers EFCoreContext.
/// </summary>
public virtual void AddEfCoreContext()
{
    _services.AddDbContext<AdncDbContext>(options =>
    {
       options.UseMySql(_mysqlConfig.ConnectionString, mySqlOptions =>
       {
          mySqlOptions.ServerVersion(new ServerVersion(new Version(10, 5, 4), ServerType.MariaDb));
          mySqlOptions.CharSet(CharSet.Utf8Mb4);
       });
       // Replace the default query SQL generator. If read/write splitting is implemented through MyCAT middleware, replace the default SQL factory.
       options.ReplaceService<IQuerySqlGeneratorFactory, AdncMySqlQuerySqlGeneratorFactory>();
    });
}
```
- How to use

```csharp

public class EfCoreConsts
{
    public const string MyCAT_ROUTE_TO_MASTER = "#mycat:db_type=master";
    public const string MAXSCALE_ROUTE_TO_MASTER = "maxscale route to master";
}

public abstract class BaseRepository<TDbContext, TEntity> : IEfRepository<TEntity>
       where TDbContext : DbContext
       where TEntity : EfEntity
{
        public virtual IQueryable<TrdEntity> GetAll<TrdEntity>(bool writeDb = false) where TrdEntity : EfEntity
        {
            var dbSet = DbContext.Set<TrdEntity>().AsNoTracking();
            if (writeDb)
                // Route read operations to the write database.
                return dbSet.TagWith(EfCoreConsts.MyCAT_ROUTE_TO_MASTER);
            return dbSet;
        }

        public virtual async Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null, bool writeDb = false)
        {
            if (writeDb)
                // This method integrates Dapper for complex queries, with read operations routed to the write database.
                sql = string.Concat("/* ", EfCoreConsts.MyCAT_ROUTE_TO_MASTER, " */", sql);
            return await DbContext.Database.GetDbConnection().QueryAsync<TResult>(sql, param, null, commandTimeout, commandType);
        }
}
```
The code based on Mycat is implemented as above. The database connection string is consistent with the direct connection to the database. You only need to change the port to the Mycat port.

---
-- over --If you can help, welcome[star & fork](https://github.com/alphayu/adnc).
