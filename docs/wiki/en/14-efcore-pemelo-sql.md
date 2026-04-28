# ADNC Repository Usage - execute raw SQL

[GitHub repository](https://github.com/alphayu/adnc)

This article mainly introduces how to execute raw SQL in the`ADNC` repository. When encountering scenarios such as complex queries, multi-table queries, and large-volume write operations, reluctantly using EF Core implementation is not the best solution. For example,`SqlSugar`and`FreeSql`also provide the ability to directly operate ADO to execute SQL; therefore, in a production environment, the appropriate use of raw SQL is often inevitable.

## execute raw SQL in the EF Core repository
The following takes the EF Core repository interface as an example to illustrate one attribute and two methods related to SQL:

```csharp
public interface IEfRepository<TEntity> : IEfBaseRepository<TEntity>
where TEntity : EfEntity
{
    /// <summary>
    /// Executes a raw SQL query.
    /// </summary>
    IAdoQuerierRepository? AdoQuerier { get; }

    /// <summary>
    /// Executes a raw SQL write operation.
    /// </summary>
    Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a raw SQL write operation.
    /// </summary>
    Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default);
}
```
The implementation instructions are as follows:
- `AdoQuerier`The implementation class injects the`IAdoQuerierRepository`interface through the constructor and assigns it to the object after setting up the database connection. The implementation of this interface in ADNC is located in`DapperRepository`of the`Adnc.Infra.Repository.Dapper`project. The definition of`IAdoQuerierRepository`almost covers Dapper's common query methods, so Dapper's query capabilities can be called through`AdoQuerier`in the EF Core repository.
- `ExecuteSqlInterpolatedAsync`Directly call the EF native method`DbContext.Database.ExecuteSqlInterpolatedAsync()`to execute SQL (write operation). This method can reduce the risk of SQL injection and is recommended to be used first.
- `ExecuteSqlRawAsync`Directly call the EF native method`DbContext.Database.ExecuteSqlRawAsync()`to execute SQL (write operation).

The sample code is as follows:

```csharp
var sql2 = @"SELECT * FROM Customer ORDER BY Id ASC";
var dbCustomer = await _customerRsp.AdoQuerier.QueryFirstAsync<Customer>(sql2, null, _customerRsp.CurrentDbTransaction);

var rawSql1 = "update Customer set nickname='test8888' where id=1000000000";
var rows = await _customerRsp.ExecuteSqlRawAsync(rawSql1);

var id=10000000;
var newNickName = "test8888";
FormattableString formatSql2 = $"update Customer set nickname={newNickName} where id={id}";
rows = await _customerRsp.ExecuteSqlInterpolatedAsync(formatSql2);
```
# Use Dapper repository directly
> Unless absolutely necessary, it is not recommended to use Dapper repository directly. Prioritize unified access through`AdoQuerier`of the EF Core repository to reduce the coupling of the data access layer and keep the calling method consistent. The advantage of using the Dapper repository directly is the flexibility to operate on any database type it supports.

```csharp
public class xxxAppService
{
    private IAdoExecuterWithQuerierRepository _dapperRepo;
    public xxxAppService(IAdoExecuterWithQuerierRepository dapperRepo)
    {
        _dapperRepo = dapperRepo;
        _dapperRepo.ChangeOrSetDbConnection(connectingstring,DbTypes.MYSQL);
    }

    public Demomethod()
    {
        var sql="SELECT * FROM Customer Order by Id desc";
        var result = await _dapperRepo.QueryAsync(sql);
    }
}
```

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
