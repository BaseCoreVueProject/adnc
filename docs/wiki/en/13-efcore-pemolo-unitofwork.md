# ADNC Repository Usage: Transactions and Unit of Work

[GitHub Repository](https://github.com/alphayu/adnc)

This article introduces transaction management and the Unit of Work pattern in ADNC.

## EF Core Transactions

EF Core supports three types of transactions:
- `SaveChanges`
- `DbContextTransaction`
- `TransactionScope`

By default, EF Core's `SaveChangesAsync` wraps operations in a transaction as needed. However, there are exceptions in ADNC:
1. **Bulk Operations**: `ExecuteUpdateAsync` and `ExecuteDeleteAsync` bypass the change tracker and are not controlled by `SaveChanges`.
2. **CAP Transactions**: CAP operations are outside the standard `SaveChanges` scope.
3. **Raw SQL**: Native SQL execution is not managed by `SaveChanges`.

## Unit of Work Implementation

ADNC uses `DbContextTransaction` to unify transaction control. If your business logic involves multiple write operations (≥ 2), it is recommended to explicitly enable transaction control.

### Using Interceptors (Declarative)

The easiest way to use transactions is via the `[UnitOfWork]` attribute on service interfaces in the `Application.Contracts` layer.

```csharp
// Local Transaction
[UnitOfWork]
Task<AppSrvResult> UpdateAsync(long id, DeptUpdationDto input);

// Distributed Transaction (CAP)
[UnitOfWork(Distributed = true)]
Task<AppSrvResult> ProcessPayingAsync(long transactionLogId, long customerId, decimal amount);
```

The framework automatically manages the transaction lifecycle (Begin, Commit, Rollback) through an interceptor.

### Manual Transaction Control

If you prefer manual control or have complex requirements, you can inject `IUnitOfWork` into your service:

```csharp
public class xxxAppService
{
    private readonly IUnitOfWork _uow;
    public xxxAppService(IUnitOfWork uow) => _uow = uow;

    public async Task DemoMethod()
    {
        try
        {
            await _uow.BeginTransactionAsync(distributed: false);
            // Operation 1
            // Operation 2
            await _uow.CommitAsync();
        }
        catch (Exception)
        {
            await _uow.RollbackAsync();
            throw;
        }
    }
}
```

---
*If this helps, please Star & Fork.*
