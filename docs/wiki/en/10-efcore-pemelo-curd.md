# ADNC Repository Usage - Basic Functions

[GitHub repository](https://github.com/alphayu/adnc)

This article focuses on how to use repository operations with MariaDB/MySQL.

## Overall Design

Repository-related infrastructure layers:

- `Adnc.Infra.Repository` defines repository interfaces, unit-of-work interfaces, entity base classes, and constants.
- `Adnc.Infra.Repository.EfCore` implements `IEfRepository`, `IEfBasicRepository`, and `IUnitOfWork`, and uses EF Core to complete create, update, delete, query, and transaction operations.
- `Adnc.Infra.Repository.Dapper` implements `IAdoExecuterRepository`, `IAdoQuerierRepository`, and `IAdoExecuterWithQuerierRepository`, and uses Dapper to execute raw SQL for create, update, delete, and query operations.

Among the three repository infrastructure layers, `Adnc.Infra.Repository.EfCore` is the core. EF Core is the main ORM used by `ADNC`, while `Adnc.Infra.Dapper` provides auxiliary capabilities.

## Used through constructor injection

```csharp
// Inject the EF Core repository.
public CustomerAppService(IEfRepository<Customer> efRepo)
{
    _efRepo = efRepo;
}

// Inject the Dapper repository separately. Do not inject it separately unless necessary.
public CustomerAppService(IAdoExecuterWithQuerierRepository dapperRepo)
{
    _dapperRepo = dapperRepo;
}
```

## EF Core Repository Methods
## InsertAsync
### Method signature

```csharp
/// <summary>
/// Inserts a single entity.
/// </summary>
/// <param name="entity"><see cref="TEntity"/></param>
/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
/// <returns></returns>
Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
```
### Unit testing

```csharp
[Fact]
public async Task TestInsertSingle01()
{
    var customerRsp = fixture.Container.GetRequiredService<IEfRepository<Customer>>();
    var customter = GenerateCustomer();
    var id = customter.Id;
    await customerRsp.InsertAsync(customter);

    var newCust = await customerRsp.AdoQuerier.QueryAsync<Customer>("SELECT *  FROM customer WHERE Id=@Id", new { Id = id });
    Assert.NotEmpty(newCust);
}
```
## InsertRangeAsync
### Method signature

```csharp
/// <summary>
/// Inserts multiple entities.
/// </summary>
/// <param name="entities"><see cref="TEntity"/></param>
/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
/// <returns></returns>
Task<int> InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
```
### Unit testing

```csharp
[Fact]
public async Task TestInsertRange()
{
    var customerRsp = fixture.Container.GetRequiredService<IEfRepository<Customer>>();
    var custLogsRsp = fixture.Container.GetRequiredService<IEfRepository<CustomerTransactionLog>>();

    var customer = await customerRsp.FetchAsync(x => x.Id > 1);

    var id0 = UnittestHelper.GetNextId();
    var id1 = UnittestHelper.GetNextId();
    var logs = new List<CustomerTransactionLog>
    {
        new CustomerTransactionLog{ Id=id0,Account=customer.Account,ChangedAmount=0,Amount=0,ChangingAmount=0,CustomerId=customer.Id,ExchangeType=ExchangeBehavior.Recharge,ExchageStatus=ExchageStatus.Finished,Remark="test"}
        ,
        new CustomerTransactionLog{ Id=id1,Account=customer.Account,ChangedAmount=0,Amount=0,ChangingAmount=0,CustomerId=customer.Id,ExchangeType=ExchangeBehavior.Recharge,ExchageStatus=ExchageStatus.Finished,Remark="test"}
    };

    await custLogsRsp.InsertRangeAsync(logs);

    var logsFromDb = await custLogsRsp.Where(x => x.Id == id0 || x.Id == id1).ToListAsync();
    Assert.NotEmpty(logsFromDb);
    Assert.Equal(2, logsFromDb.Count);
}
```
## UpdateAsync
Without specifying an update field, the entity must be tracked.
### Method signature

```csharp
/// <summary>
/// Updates a single entity.
/// </summary>
/// <param name="entity"><see cref="TEntity"/></param>
/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
/// <returns></returns>
Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
```
### Unit testing

```csharp
[Fact]
public async Task TestUpdateWithTraking()
{
    var customerRsp = fixture.Container.GetRequiredService<IEfRepository<Customer>>();

    var cus1 = await InsertCustomer();
    var cus2 = await InsertCustomer();

    var ids = await customerRsp.AdoQuerier.QueryAsync<long>("SELECT Id FROM customer where ID in @ids ORDER BY ID", new { ids = new[] { cus1.Id, cus2.Id } });
    var id0 = ids.ToArray()[0];

    // IEfRepository<> disables tracking by default, so tracking must be enabled manually.
    var customer = await customerRsp.FetchAsync(x => x.Id == id0, noTracking: false);
    // The entity is already tracked.
    customer.Realname = "Tracked01";
    await customerRsp.UpdateAsync(customer);
    var newCust1 = await customerRsp.AdoQuerier.QueryAsync<Customer>("SELECT * FROM customer WHERE Id=@Id", new { Id = id0 });
    Assert.Equal("Tracked01", newCust1.FirstOrDefault().Realname);

    var customerId = (await customerRsp.AdoQuerier.QueryAsync<long>("SELECT Id  FROM customerfinance limit 0,1")).FirstOrDefault();
    customer = await customerRsp.FetchAsync(x => x.Id == customerId, x => x.FinanceInfo, noTracking: false);
    customer.Account = "ParentChildUpdate01";
    customer.FinanceInfo.Account = "ParentChildUpdate01";
    await customerRsp.UpdateAsync(customer);
    var newCust2 = await customerRsp.FetchAsync(x => x.Id == customerId, x => x.FinanceInfo);
    Assert.Equal("ParentChildUpdate01", newCust2.Account);
    Assert.Equal("ParentChildUpdate01", newCust2.FinanceInfo.Account);
}
```
## ExecuteUpdateAsync
Specify the update field, the entity can be in any state.
### Method signature

```csharp
/// <summary>
/// Batch update.
/// </summary>
/// <param name="whereExpression">Query condition</param>
/// <param name="setPropertyCalls">Fields to update</param>
/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
/// <returns></returns>
Task<int> ExecuteUpdateAsync(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellationToken = default);
```
### Unit testing

```csharp
[Fact]
public async Task TestUpdateAssigns()
{
    var customerRsp = fixture.Container.GetRequiredService<IEfRepository<Customer>>();

    var cus1 = await InsertCustomer();
    var cus2 = await InsertCustomer();
    var cus3 = await InsertCustomer();
    var cus4 = await InsertCustomer();

    var ids = await customerRsp.AdoQuerier.QueryAsync<long>("SELECT Id FROM customer where ID in @ids ORDER BY ID", new { ids = new[] { cus1.Id, cus2.Id, cus3.Id, cus4.Id } });
    var id0 = ids.ToArray()[0];
    var customer = await customerRsp.FetchAsync(x=>x.Id == id0, noTracking: false);
    // The entity is already tracked and specific columns are selected, so Realname will not be updated.
    customer.Nickname = "UpdateSelectedColumn";
    customer.Realname = "ColumnNotSelected";
    await customerRsp.ExecuteUpdateAsync(x => x.Id == customer.Id,
                                         setters => setters.SetProperty(c => c.Nickname, "UpdateSelectedColumn"));
    var newCus = (await customerRsp.AdoQuerier.QueryAsync<Customer>("SELECT * FROM customer WHERE ID=@ID", customer)).FirstOrDefault();
    Assert.Equal("UpdateSelectedColumn", newCus.Nickname);
    Assert.NotEqual("ColumnNotSelected", newCus.Realname);
}
```

## DeleteAsync
Delete based on Id
### Method signature

```csharp
/// <summary>
/// Deletes entities.
/// </summary>
/// <param name="keyValue">Id</param>
/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
/// <returns></returns>
Task<int> DeleteAsync(long keyValue, CancellationToken cancellationToken = default);
```
### Unit testing

```csharp
[Fact]
public async Task TestDelete()
{
    //single hard delete 
    var customer = await this.InsertCustomer();
    var customerFromDb = await _customerRsp.FindAsync(customer.Id);
    Assert.Equal(customer.Id, customerFromDb.Id);

    await _customerRsp.DeleteAsync(customer.Id);
    var result = await _customerRsp.QueryAsync<Customer>("SELECT * FROM Customer WHERE ID=@Id", new { Id = customer.Id });
    Assert.Empty(result);
}
```
## ExecuteDeleteAsync
Delete in batches based on conditions
### Method signature

```csharp
/// <summary>
/// Batch delete entities.
/// </summary>
/// <param name="whereExpression">Query condition</param>
/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
/// <returns></returns>
Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);
```
### Unit testing

```csharp
[Fact]
public async Task TestDeleteRange()
{
    var customerRsp = fixture.Container.GetRequiredService<IEfRepository<Customer>>();

    //batch hand delete
    var cus1 = await InsertCustomer();
    var cus2 = await InsertCustomer();
    var total = await customerRsp.CountAsync(c => c.Id == cus1.Id || c.Id == cus2.Id);
    Assert.Equal(2, total);

    await customerRsp.ExecuteDeleteAsync(c => c.Id == cus1.Id || c.Id == cus2.Id);
    var result2 = await customerRsp.AdoQuerier.QueryAsync<Customer>("SELECT * FROM customer WHERE ID in @ids", new { ids = new[] { cus1.Id, cus2.Id } });
    Assert.Equal(0, result2.Count());
}
```
## FetchAsync
Return the first item based on query conditions
### Method signature

```csharp
/// <summary>
/// Queries by condition and returns a single entity.
/// </summary>
/// <param name="whereExpression">The query condition.</param>
/// <param name="navigationPropertyPath">The navigation property path. Optional.</param>
/// <param name="orderByExpression">The sort field. Defaults to the primary key. Optional.</param>
/// <param name="ascending">The sort direction. Defaults to descending. Optional.</param>
/// <param name="writeDb">Whether to use the write database. Defaults to false. Optional.</param>
/// <param name="noTracking">Whether to disable tracking. Tracking is disabled by default. Optional.</param>
/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
Task<TEntity> FetchAsync(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, dynamic>> navigationPropertyPath = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, bool writeDb = false, bool noTracking = true, CancellationToken cancellationToken = default);

/// <summary>
/// Queries by condition and returns a single entity or object.
/// </summary>
/// <typeparam name="TResult">The anonymous object type.</typeparam>
/// <param name="selector">The selector.</param>
/// <param name="whereExpression">The query condition.</param>
/// <param name="orderByExpression">The sort field. Defaults to the primary key. Optional.</param>
/// <param name="ascending">The sort direction. Defaults to descending. Optional.</param>
/// <param name="writeDb">Whether to use the write database. Defaults to false. Optional.</param>
/// <param name="noTracking">Whether to disable tracking. Tracking is disabled by default. Optional.</param>
/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
Task<TResult> FetchAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, bool writeDb = false, bool noTracking = true, CancellationToken cancellationToken = default);
```
### Unit testing

```csharp
[Fact]
public async Task TestFetch()
{
    // Query specific columns.
    var customer = await _customerRsp.FetchAsync(x => new { x.Id, x.Account}, x => x.Id > 1);
    Assert.NotNull(customer);

    // Query specific columns, including navigation properties.
    var customer2 = await _customerRsp.FetchAsync(x => new { x.Id, x.Account, x.FinanceInfo }, x => x.Id > 1);
    Assert.NotNull(customer2);

    // Query without specifying columns.
    var customer3 = await _customerRsp.FetchAsync(x => x.Id > 1);
    Assert.NotNull(customer3);

    // Query without specifying columns and preload navigation properties.
    var customer4 = await _customerRsp.FetchAsync(x => x.Id > 1, x => x.FinanceInfo);
    Assert.NotNull(customer4);
}
```
## AnyAsync
To query whether the entity already exists, call the native method of Ef.
### Method signature

```csharp
/// <summary>
/// Checks whether an entity exists by condition.
/// </summary>
/// <param name="whereExpression">The query condition.</param>
/// <param name="writeDb">Whether to use the write database. Defaults to false. Optional.</param>
/// param name="cancellationToken"><see cref="CancellationToken"/></param>
/// <returns></returns>
Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression, bool writeDb = false, CancellationToken cancellationToken = default);
```

## CountAsync
Count the number of entities that meet the conditions and call the native method of Ef.
### Method signature

```csharp
/// <summary>
/// Counts entities that match the condition.
/// </summary>
/// <param name="whereExpression">The query condition.</param>
/// <param name="writeDb">Whether to use the write database. Defaults to false. Optional.</param>
/// param name="cancellationToken"><see cref="CancellationToken"/></param>
/// <returns></returns>
Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression, bool writeDb = false, CancellationToken cancellationToken = default);
```
## Where,GetAll
These are two universal query methods that return an IQueryable based on query conditions.`GetAll()``==``Where(x=>true)`

#### Method signature

```csharp
/// <summary>
/// Queries by condition and returns IQueryable<TEntity>.
/// </summary>
/// <param name="expression">The query condition.</param>
/// <param name="writeDb">Whether to use the write database. Defaults to false. Optional.</param>
/// <param name="noTracking">Whether to disable tracking. Defaults to false. Optional.</param>
IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool writeDb = false, bool noTracking = true);

 /// <summary>
 /// Returns IQueryable<TEntity>.
 /// </summary>
 /// <param name="writeDb">Whether to use the write database. Defaults to false. Optional.</param>
 /// <param name="noTracking">Whether to disable tracking. Defaults to false. Optional.</param>
IQueryable<TEntity> GetAll(bool writeDb = false, bool noTracking = true);
```
### Unit testing

```csharp
// Test queries.
[Fact]
public async Task TestWhereAndGetAll()
{
    // Return a collection.
    var customers = await _customerRsp.Where(x => x.Id > 1).ToListAsync();
    Assert.NotEmpty(customers);

    // Return a single item.
    var customer = await _customerRsp.Where(x => x.Id > 1).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
    Assert.NotNull(customer);

    // Combined query.
    //GetAll() = Where(x=>true)
    var customerAll = _customerRsp.GetAll();
    var custsLogs = _custLogsRsp.GetAll();

    var logs = await customerAll.Join(custsLogs, c => c.Id, t => t.CustomerId, (c, t) => new
    {
        t.Id
        ,
        t.CustomerId
        ,
        t.Account
        ,
        t.ChangedAmount
        ,
        t.ChangingAmount
        ,
        c.Realname
    })
    .Where(c => c.Id > 1)
    .ToListAsync();

    Assert.NotEmpty(logs);
}
```
---

-- over --If you can help, please feel free to Star & Fork.
