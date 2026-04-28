# ADNC Repository Usage - transactions

[GitHub repository](https://github.com/alphayu/adnc)

This article mainly introduces transactions and work units.[EFCore Transactions](https://docs.microsoft.com/zh-cn/ef/core/saving/transactions)

is divided into the following three types:- SaveChanges
- DbContextTransaction
- TransactionScope

By default in EF Core, SaveChanges starts transactions as needed.1. Batch update/deletion of`ExecuteUpdateAsync`,`ExecuteDeleteAsync`, etc. does not go through the EF Core native change tracking process and is not controlled by the SaveChanges transaction.2. CAP transactions are not controlled by the SaveChanges transaction.3. The addition, deletion, modification and query of raw SQL are not controlled by the SaveChanges transaction.4. Implementation constraints related to read-write separation (please refer to "How to Implement Read-Write Separation").

```csharp
public class AdncDbContext : DbContext
{
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        //efcore7 support this feature , default is WhenNeeded
        //Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;
        SetAuditFields();
        var result = base.SaveChangesAsync(cancellationToken);
        return result;
    }
}
```

`ADNC`centrally controls transactions through`DbContextTransaction`. If the business logic requires multiple addition/deletion/modification operations (for example, ≥ 2 times), it is recommended to explicitly enable transaction control (for example, through interceptors/property declarations).> Master-slave table insertion, master-slave table update, batch addition, batch modification, and batch deletion operations can all be implemented through an add/delete/modify method, and there is no need to explicitly start a transaction.`Adnc.Infra.Repository.EfCore.MySql`implements the`IUnitOfWork`interface of`Adnc.Infra.Repository`. We only need to explicitly declare the interceptor attribute or inject it from the constructor.

## How to use
## Register interceptor`services.AddAppliactionSerivcesWithInterceptors()`This extension method is well implemented. Please refer to the source code for specific implementation.

### Explicitly declare interceptors
The unified declaration of the service interface in the`Adnc.Xxx.Application.Contracts`layer is also declared on the interface.

```csharp
// Local transaction.
[UnitOfWork]
Task<AppSrvResult> UpdateAsync(long id, DeptUpdationDto input);

// CAP transaction/distributed transaction.
[UnitOfWork(Distributed =true)]
Task<AppSrvResult> ProcessPayingAsync(long transactionLogId, long customerId, decimal amount);
```
---
## Free call to start transaction
If you don't like the interceptor to handle transactions or the interceptor to handle transactions cannot meet your needs, you can also enable it freely.

```csharp
public class xxxAppService
{
    private IUnitOfWork _uow;
    public xxxAppService(IUnitOfWork uow) _uow=>uow;
    public DemoMethod()
    {
        try
        {
            _uow.BeginTransaction(distributed:false);
            // Operation 1
            // Operation 2
            // Operation 3
            // Operation N
            _unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            _unitOfWork.Rollback();
        }
        finally
        {
            _unitOfWork.Dispose();
        }  
    }
}

```
-

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
