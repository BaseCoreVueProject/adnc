﻿namespace Adnc.Infra.Repository.EfCore.MySql.Transaction;

public class MySqlUnitOfWork<TDbContext>(TDbContext context, ICapPublisher? publisher = null) : UnitOfWork<TDbContext>(context)
    where TDbContext : MySqlDbContext
{
    private ICapPublisher? _publisher = publisher;

    protected override IDbContextTransaction GetDbContextTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, bool distributed = false)
    {
        if (distributed)
            if (_publisher is null)
                throw new ArgumentException("CapPublisher is null");
            else
                return AdncDbContext.Database.BeginTransaction(_publisher, false);
        else
            return AdncDbContext.Database.BeginTransaction(isolationLevel);
    }
}