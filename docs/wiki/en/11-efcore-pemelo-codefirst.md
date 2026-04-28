# ADNC Repository Usage: Code First

[GitHub Repository](https://github.com/alphayu/adnc)

This article describes how to map entities to the database in the `ADNC` framework using the **Code First** pattern. 

## Mapping Workflow

### Step 1: Create the Entity
Create your entity class in the `Entities` directory of the `Adnc.Cus.Repository` project.

- For **Classic 3-tier architecture**, inherit from `EfEntity`.
- For **DDD architecture**, inherit from `AggregateRoot` or `DomainEntity`.

```csharp
namespace Adnc.Cus.Entities
{
    public class Customer : EfFullAuditEntity
    {
        public string Account { get; set; }
        public string Nickname { get; set; }
        public string Realname { get; set; }
        public virtual CustomerFinance FinanceInfo { get; set; }
        public virtual ICollection<CustomerTransactionLog> TransactionLogs { get; set; }
    }
}
```

### Step 2: Define the Mapping Configuration
Create a mapping class in the `Entities/Config` directory using **Fluent API**.

```csharp
namespace Adnc.Cus.Entities.Config
{
    public class CustomerConfig : EntityTypeConfiguration<Customer>
    {
        public override void Configure(EntityTypeBuilder<Customer> builder)
        {
            base.Configure(builder);
            builder.HasOne(d => d.FinanceInfo)...;
            builder.Property(x => x.Account).IsRequired().HasMaxLength(50);
        }
    }
}
```

Note: `EntityTypeConfiguration<TEntity>` is a base class in ADNC that handles common mapping concerns like soft deletes, concurrency tokens (`RowVersion`), and primary keys.

### Step 3: Create the EntityInfo Class
Define an `EntityInfo` class implementing `IEntityInfo`. This class is used to scan the assembly for entities.

```csharp
namespace Adnc.Cus.Entities
{
    public class EntityInfo : AbstractEntityInfo
    {
        public override IEnumerable<EntityTypeInfo> GetEntitiesTypeInfo()
        {
            return base.GetEntityTypes(this.GetType().Assembly)
                       .Select(x => new EntityTypeInfo() { Type = x });
        }
    }
}
```

### Step 4: Dependency Injection
The `EntityInfo` is registered automatically via the `services.AddEfCoreContextWithRepositories()` extension method during application startup.

### Step 5: Migrations
1. Set the API project (e.g., `Adnc.Cus.WebApi`) as the **Startup Project**.
2. Open the **Package Manager Console**.
3. Set the default project to the Migrations project (e.g., `Adnc.Cus.Migrations`).
4. Run `add-migration InitialCreate`.
5. Run `update-database`.

---

## How Entities Relate to the Database

ADNC uses a custom `AdncDbContext` that automatically applies configurations:

1. **Table Naming**: Automatically converts table and column names to lowercase.
2. **Comments**: Reads `<summary>` tags from your C# code and applies them as database comments.
3. **Fluent API**: Automatically loads all configurations from the assembly via `modelBuilder.ApplyConfigurationsFromAssembly`.

---
*If this helps, please Star & Fork.*
