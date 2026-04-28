# ADNC Repository Usage - CodeFirst

[GitHub repository](https://github.com/alphayu/adnc)

This article mainly introduces how to map entities to the database in the`ADNC`framework. The examples are in Code First mode; you can also use DB First mode if needed.All operations in this article take the`Adnc.Demo.Cust`service as an example, and other services are defined in the same way.

## How to map
## The first step is to create an entity
Create an entity (for example,`Customer`) in the`Repository/Entities`directory of the`Adnc.Demo.Cust.Api`project. If using DDD mode, the entity is usually located in the`Adnc.Demo.Xxx.Domain`project.> If developed using the classic three-tier model, the entity needs to directly or indirectly inherit the`EfEntity`class.
> If developed using the DDD architecture model, the entity needs to directly or indirectly inherit`AggregateRoot`or`DomainEntity`.`EfEntity`The base class of all entity classes in the classic three-tier development model

```csharp
public abstract class EfEntity : Entity, IEfEntity<long>
{ 
}

public abstract class EfBasicAuditEntity : EfEntity, IBasicAuditInfo
{
}

public abstract class EfFullAuditEntity : EfEntity, IFullAuditInfo
{
}
```
The complete code to create an entity class is as follows:

```csharp
public class Customer : EfFullAuditEntity
{
    public static readonly int Account_MaxLength = 16;
    public static readonly int Password_Maxlength = 32;
    public static readonly int Nickname_MaxLength = 16;
    public static readonly int Realname_Maxlength = 16;
    
    public string Account { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Realname { get; set; } = string.Empty;
    public virtual required Finance FinanceInfo { get; set; }
    public virtual ICollection<TransactionLog>? TransactionLogs { get; set; }
}
```
## The second step is to define the mapping relationship
Create a mapping relationship class in the`Entities/Config`directory, such as`CustomerConfig`> Create a mapping relationship through`fluentapi`.

```csharp
public class CustomerConfig : AbstractEntityTypeConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        builder.HasOne(d => d.FinanceInfo).WithOne().HasForeignKey<Finance>(d => d.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(d => d.TransactionLogs).WithOne().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.Account).HasMaxLength(Customer.Account_MaxLength);
        builder.Property(x => x.Password).HasMaxLength(Customer.Password_Maxlength);
        builder.Property(x => x.Nickname).HasMaxLength(Customer.Nickname_MaxLength);
        builder.Property(x => x.Realname).HasMaxLength(Customer.Realname_Maxlength);
    }
}
```
In many examples,`CustomerConfig`directly inherits the`IEntityTypeConfiguration<TEntity>`interface. I've encapsulated it a little bit here. Created a`AbstractEntityTypeConfiguration<TEntity>`abstract class. Then our entity relationship mapping class inherits this abstract class. This is mainly done to uniformly handle the mapping of some public feature fields. Such as soft deletion, concurrent column mapping, etc., the code is as follows.

```csharp
public abstract class AbstractEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
   where TEntity : Entity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        var entityType = typeof(TEntity);
        ConfigureKey(builder, entityType);
        ConfigureConcurrency(builder, entityType);
        ConfigureSoftDelete(builder, entityType);
        ConfigureAuditInfo(builder, entityType);
    }
}
```
## The third step is to create the EntityInfo class
Create a`EntityInfo`class and inherit the`AbstractEntityInfo`abstract class. The GetEntityAssemblies() method is to find the class that inherits`EfEntity`in the current assembly and put it into the collection.

```csharp
public class EntityInfo : AbstractEntityInfo
{
    protected override List<Assembly> GetEntityAssemblies() => [GetType().Assembly, typeof(EventTracker).Assembly];

    protected override void SetTableName(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventTracker>().ToTable("cust_eventtracker");
        modelBuilder.Entity<Customer>().ToTable("cust_customer");
        modelBuilder.Entity<Finance>().ToTable("cust_finance");
        modelBuilder.Entity<TransactionLog>().ToTable("cust_transactionlog");
    }
}
```
## Step 4: Generate migration code and update it to the database
- Set`Adnc.Demo.Cust.Api`as the startup project (the migration command will read the database connection string from this project)
- Open Nuget's package manager console in the VS tool (Tools => Nuget Package Manager => Package Manager Console)
- Execute command`add-migration Update2021030401`. After successful execution, the migration file will be generated in the`Migrations`directory.
- Execute command`update-database`to update to the database.

---

# How entities are associated with the databaseLet’s look at the source code of the`AdncDbContext`class of the`Adnc.Infra.Repository.EfCore`project.

```csharp
public abstract class AdncDbContext(DbContextOptions options, IEntityInfo entityInfo, Operater operater) : DbContext(options)
{
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        //efcore7 support this feature , default is WhenNeeded
        //Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;
        SetAuditFields();
        var result = base.SaveChangesAsync(cancellationToken);
        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        entityInfo.OnModelCreating(modelBuilder);
    }

    protected virtual void SetAuditFields()
    {
        var allBasicAuditEntities = ChangeTracker.Entries<IBasicAuditInfo>().Where(x => x.State == EntityState.Added);
        allBasicAuditEntities.ForEach(entry =>
        {
            entry.Entity.CreateBy = operater.Id;
            entry.Entity.CreateTime = DateTime.Now;
        });

        var auditFullEntities = ChangeTracker.Entries<IFullAuditInfo>().Where(x => x.State == EntityState.Modified || x.State == EntityState.Added);
        auditFullEntities.ForEach(entry =>
        {
            entry.Entity.ModifyBy = operater.Id;
            entry.Entity.ModifyTime = DateTime.Now;
        });
    }
}
```

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
