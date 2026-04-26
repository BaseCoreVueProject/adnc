# ADNC Repository Usage: Executing Raw SQL

[GitHub Repository](https://github.com/alphayu/adnc)

This article describes how to execute raw SQL in the ADNC repository layer. In scenarios involving complex queries, multi-table joins, or high-volume writes, raw SQL is often more efficient than EF Core.

## Executing SQL via EF Core Repository

The `IEfRepository<TEntity>` interface provides a way to access Dapper for queries and EF's native methods for writes.

```csharp
public interface IEfRepository<TEntity> : IEfBaseRepository<TEntity>
{
    /// <summary>
    /// For Raw SQL Queries (via Dapper)
    /// </summary>
    IAdoQuerierRepository? AdoQuerier { get; }

    /// <summary>
    /// For Raw SQL Write Operations (via EF Interpolated)
    /// </summary>
    Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, ...);

    /// <summary>
    /// For Raw SQL Write Operations (via EF Raw)
    /// </summary>
    Task<int> ExecuteSqlRawAsync(string sql, ...);
}
```

### Usage Examples

- **Querying with Dapper via EF Repository**:
  The `AdoQuerier` property provides access to Dapper's query methods.
  ```csharp
  var sql = "SELECT * FROM Customer WHERE Id = @Id";
  var customer = await _repo.AdoQuerier.QueryFirstAsync<Customer>(sql, new { Id = 1 });
  ```

- **Writing with Interpolated SQL**:
  This method reduces the risk of SQL injection and is the recommended way to perform raw updates.
  ```csharp
  var id = 10001;
  var name = "NewNickName";
  await _repo.ExecuteSqlInterpolatedAsync($"UPDATE Customer SET Nickname={name} WHERE Id={id}");
  ```

## Direct Use of Dapper Repository

While prioritized through `AdoQuerier`, you can also inject `IAdoExecuterWithQuerierRepository` directly if needed for extreme flexibility across different database types.

```csharp
public class xxxAppService
{
    private readonly IAdoExecuterWithQuerierRepository _dapperRepo;
    public xxxAppService(IAdoExecuterWithQuerierRepository dapperRepo)
    {
        _dapperRepo = dapperRepo;
        _dapperRepo.ChangeOrSetDbConnection(connectionString, DbTypes.MYSQL);
    }
}
```

---
*If this helps, please Star & Fork.*
