## ADNC 如何使用仓储 - CodeFirst

[GitHub 仓库地址](https://github.com/alphayu/adnc)
本文主要介绍在 `ADNC` 框架中，如何将实体映射到数据库。示例采用 Code First 模式；如有需要，也可使用 DB First 模式。
本文所有操作均以 `Adnc.Demo.Cust` 服务为例，  其它服务的定义方式一致。

## 如何映射
### 第一步，创建实体
在 `Adnc.Demo.Cust.Api` 工程的 `Repository/Entities` 目录下创建实体（例如 `Customer`）。如果采用 DDD 模式，则实体通常位于 `Adnc.Demo.Xxx.Domain` 工程中。

> 若采用经典三层模式开发，实体需直接或间接继承 `EfEntity` 类。
> 若采用 DDD 架构模式开发，实体需直接或间接继承 `AggregateRoot` 或 `DomainEntity`。

`EfEntity` 经典三层开发模式中所有实体类的基类 

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
创建一个实体类完整代码如下：
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
### 第二步，定义映射关系
在`Entities/Config`目录下创建映射关系类，如`CustomerConfig`  
> 通过`fluentapi`创建映射关系。

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
很多示例中`CustomerConfig`是直接继承`IEntityTypeConfiguration<TEntity>`这个接口。我这里稍微封装了下。创建了一个`AbstractEntityTypeConfiguration<TEntity>`抽象类。然后我们实体关系映射类再继承这个抽象类。这样做主要是为了统一处理一些公共特性字段的映射。如软删除、并发列映射等等，代码如下。
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
### 第三步，创建EntityInfo类
创建一个`EntityInfo`类，并继承`AbstractEntityInfo`抽象类，GetEntityAssemblies()方法就是在当前程序集中查找继承了`EfEntity`的类，并放入集合中。

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
### 第四步，生成迁移代码并更新到数据库
- 设置`Adnc.Demo.Cust.Api`为启动项目(迁移命令会从这个工程读取数据库连接串)
- 在VS工具中打开Nuget的程序包管理器控制台(工具=>Nuget包管理器=>程序包管理器控制台)
- 执行命令`add-migration Update2021030401`。执行成功后，`Migrations`目录下生成迁移文件。
- 执行命令`update-database`,更新到数据库。

---

## 实体如何关联数据库

我们看`Adnc.Infra.Repository.EfCore`工程的`AdncDbContext`类的源码。

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

—— 完 ——

如有帮助，欢迎 Star & Fork。
