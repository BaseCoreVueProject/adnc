# ADNC Repository Usage: Basic Functionality

[GitHub Repository](https://github.com/alphayu/adnc)

This article focuses on using repositories to operate MariaDB/MySQL.

## Architecture Design

The infrastructure layers for repositories are:

- `Adnc.Infra.Repository`: Defines repository interfaces, unit of work interfaces, entity base classes, and constants.
- `Adnc.Infra.Repository.EfCore`: Implements `IEfRepository`, `IEfBasicRepository`, and `IUnitOfWork` using EF Core for CRUD and transactions.
- `Adnc.Infra.Repository.Dapper`: Implements `IAdoExecuterRepository` and related interfaces using Dapper for raw SQL operations.

`Adnc.Infra.Repository.EfCore` is the core implementation; Dapper is used as a supplementary tool.

## Dependency Injection

```csharp
// Injecting EF Core Repository
public CustomerAppService(IEfRepository<Customer> efRepo)
{
    _efRepo = efRepo;
}

// Injecting Dapper Repository (use only when necessary)
public CustomerAppService(IAdoExecuterWithQuerierRepository dapperRepo)
{
    _dapperRepo = dapperRepo;
}
```

## EF Core Repository Methods

### InsertAsync

```csharp
Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
```

**Unit Test Example:**
```csharp
[Fact]
public async Task TestInsertSingle01()
{
    var customerRsp = fixture.Container.GetRequiredService<IEfRepository<Customer>>();
    var customer = GenerateCustomer();
    await customerRsp.InsertAsync(customer);

    var newCust = await customerRsp.AdoQuerier.QueryAsync<Customer>("SELECT * FROM customer WHERE Id=@Id", new { Id = customer.Id });
    Assert.NotEmpty(newCust);
}
```

### InsertRangeAsync

```csharp
Task<int> InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
```

### UpdateAsync
Updates the entire entity. The entity must be in a **tracked** state.

```csharp
Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
```

**Unit Test Example:**
```csharp
[Fact]
public async Task TestUpdateWithTracking()
{
    var customerRsp = fixture.Container.GetRequiredService<IEfRepository<Customer>>();
    // EF Repository disables tracking by default; enable it manually for updates.
    var customer = await customerRsp.FetchAsync(x => x.Id == id0, noTracking: false);
    customer.Realname = "UpdatedName";
    await customerRsp.UpdateAsync(customer);
}
```

### ExecuteUpdateAsync
Updates specific fields. The entity can be in any state (Bulk update).

```csharp
Task<int> ExecuteUpdateAsync(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellationToken = default);
```

### DeleteAsync
Deletes by Primary Key.

```csharp
Task<int> DeleteAsync(long keyValue, CancellationToken cancellationToken = default);
```

### ExecuteDeleteAsync
Bulk deletes based on conditions.

```csharp
Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);
```

### FetchAsync
Retrieves the first entity matching the criteria.

```csharp
Task<TEntity> FetchAsync(Expression<Func<TEntity, bool>> whereExpression, ...);
```

### AnyAsync / CountAsync
Standard EF Core operations for checking existence or counting records.

### Where / GetAll
Both methods return an `IQueryable<TEntity>` for further composition. `GetAll()` is equivalent to `Where(x => true)`.

```csharp
IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool writeDb = false, bool noTracking = true);
```

**Unit Test Example:**
```csharp
[Fact]
public async Task TestWhereAndGetAll()
{
    var customers = await _customerRsp.Where(x => x.Id > 1).ToListAsync();
    Assert.NotEmpty(customers);
}
```

---
*If this helps, please Star & Fork.*
