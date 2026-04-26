# ADNC Development Workflow: A Complete Example with "Student"

[GitHub Repository](https://github.com/alphayu/adnc)

This guide uses the `Student` entity as an example to systematically explain the standard workflow for implementing complete CRUD (Create, Read, Update, Delete) functionality in an ADNC project. It covers key stages from Entity definition to the Controller layer, ensuring code consistency and maintainability.

---

## 1. Repository Layer

### 1.1 Defining the Entity

Create a new `Student` entity in the `Repository\Entities` directory:

1. The entity must directly or indirectly inherit from `EfEntity`.
2. If the entity contains `CreateBy`, `CreateByTime`, `ModifyBy`, or `ModifyTime` fields, inherit from `EfFullAuditEntity`.
3. If it only contains `CreateBy` and `CreateByTime`, inherit from `EfBasicAuditEntity`.
4. If it has no audit fields, inherit from `EfEntity`.
5. For soft delete support, implement the `ISoftDelete` interface.
6. For optimistic locking (concurrency control), implement the `IConcurrency` interface.

```csharp
namespace Adnc.Demo.Admin.Repository.Entities;

/// <summary>
/// Student Entity
/// </summary>
public class Student : EfBasicAuditEntity, ISoftDelete
{
    public static readonly int Name_MaxLength = 50;

    /// <summary>
    /// Student Name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Soft Delete Flag
    /// </summary>
    public bool IsDeleted { get; set; }
}
```

Register it in `EntityInfo.cs`:

```csharp
modelBuilder.Entity<Student>().ToTable("sch_student");
```

---

### 1.2 Defining the EntityConfig

Create a `StudentConfig` class in `Repository\Entities\Config`:

```csharp
namespace Adnc.Demo.Admin.Repository.Entities.Config;

public class StudentConfig : AbstractEntityTypeConfiguration<Student>
{
    public override void Configure(EntityTypeBuilder<Student> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Name).HasMaxLength(Student.Name_MaxLength);
    }
}
```

### 1.3 Database Migration

After defining the entity, perform the migration:

```powershell
cd src
# Add migration
dotnet ef migrations add Student --project Repository\Adnc.Demo.Admin.Repository.csproj --startup-project Api\Adnc.Demo.Admin.Api.csproj
# Update database
dotnet ef database update --project Repository\Adnc.Demo.Admin.Repository.csproj --startup-project Api\Adnc.Demo.Admin.Api.csproj
```

---

## 2. Application / Application.Contracts Layer

### 2.1 Configuring AutoMapper

Add mapping configurations in `Application\AutoMapper\MapperProfile.cs`:

```csharp
CreateMap<Student, StudentDto>();
CreateMap<StudentCreationDto, Student>();
```

### 2.2 Creating DTOs

Create DTOs in `Application\Contracts\Dtos\Student`:

1. If the DTO includes audit fields, inherit from `OutputFullAuditInfoDto` or `OutputBaseAuditDto`.
2. Otherwise, inherit from `InputDto` or `OutputDto`.
3. Paged search DTOs must inherit from `SearchPagedDto`.

```csharp
namespace Adnc.Demo.Admin.Application.Contracts.Dtos.Student;

public class StudentCreationDto : InputDto
{
    public string Name { get; set; } = string.Empty;
}

public class StudentUpdationDto : StudentCreationDto { }

public class StudentSearchPagedDto : SearchPagedDto { }

[Serializable]
public class StudentDto : OutputBaseAuditDto
{
    public string Name { get; set; } = string.Empty;
}
```

---

### 2.3 Writing Validators

Create validator classes in `Application\Contracts\Dtos\Student\Validators`:

```csharp
using Adnc.Demo.Admin.Application.Contracts.Dtos.Student;

namespace Adnc.Demo.Admin.Application.Validators;

public class StudentCreationDtoValidator : AbstractValidator<StudentCreationDto>
{
    public StudentCreationDtoValidator()
    { 
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Student.Name_MaxLength);
    }
}

public class StudentUpdationDtoValidator : AbstractValidator<StudentUpdationDto>
{
    public StudentUpdationDtoValidator()
    {
        Include(new StudentCreationDtoValidator());
    }
}
```

---

### 2.4 IStudentService Interface Definition

Define the interface in `Application\Contracts\Interfaces`:

1. Must inherit from `IAppService`.
2. All write operations must return `ServiceResult` or `ServiceResult<T>`.
3. For transactions, use the `[UnitOfWork]` attribute.

```csharp
namespace Adnc.Demo.Admin.Application.Contracts.Interfaces;

public interface IStudentService : IAppService
{
    [OperateLog(LogName = "Create Student")]
    Task<ServiceResult<IdDto>> CreateAsync(StudentCreationDto input);

    [OperateLog(LogName = "Update Student")]
    Task<ServiceResult> UpdateAsync(long id, StudentUpdationDto input);

    [OperateLog(LogName = "Delete Student")]
    Task<ServiceResult> DeleteAsync(long[] ids);

    Task<StudentDto?> GetAsync(long id);

    Task<PageModelDto<StudentDto>> GetPagedAsync(StudentSearchPagedDto input);
}
```

### 2.5 Implementing IStudentService

Implement the service in `Application\Services`:

1. Must inherit from `AbstractAppService`.
2. Return `Problem` for business validation failures instead of throwing exceptions.

```csharp
namespace Adnc.Demo.Admin.Application.Services;

public class StudentService(IEfRepository<Student> studentRepo)
    : AbstractAppService, IStudentService
{
    public async Task<ServiceResult<IdDto>> CreateAsync(StudentCreationDto input)
    {
        input.TrimStringFields();
        var nameExists = await studentRepo.AnyAsync(x => x.Name == input.Name);
        if (nameExists) return Problem(HttpStatusCode.BadRequest, "Name already exists");

        var entity = Mapper.Map<Student>(input, IdGenerater.GetNextId());
        await studentRepo.InsertAsync(entity);
        return new IdDto(entity.Id);
    }

    public async Task<ServiceResult> UpdateAsync(long id, StudentUpdationDto input)
    {
        input.TrimStringFields();
        var entity = await studentRepo.FetchAsync(x => x.Id == id, noTracking: false);
        if (entity is null) return Problem(HttpStatusCode.NotFound, "Student not found");

        var nameExists = await studentRepo.AnyAsync(x => x.Name == input.Name && x.Id != id);
        if (nameExists) return Problem(HttpStatusCode.BadRequest, "Name already exists");

        Mapper.Map(input, entity);
        await studentRepo.UpdateAsync(entity);
        return ServiceResult();
    }

    public async Task<ServiceResult> DeleteAsync(long[] ids)
    {
        await studentRepo.ExecuteDeleteAsync(x => ids.Contains(x.Id));
        return ServiceResult();
    }

    public async Task<StudentDto?> GetAsync(long id)
    {
        var entity = await studentRepo.FetchAsync(x => x.Id == id);
        return entity is null ? null : Mapper.Map<StudentDto>(entity);
    }

    public async Task<PageModelDto<StudentDto>> GetPagedAsync(StudentSearchPagedDto input)
    {
        input.TrimStringFields();
        var whereExpr = ExpressionCreator.New<Student>()
            .AndIf(input.Keywords.IsNotNullOrWhiteSpace(), x => EF.Functions.Like(x.Name, $"{input.Keywords}%"));

        var total = await studentRepo.CountAsync(whereExpr);
        if (total == 0) return new PageModelDto<StudentDto>(input);

        var entities = await studentRepo.Where(whereExpr)
            .OrderByDescending(x => x.Id)
            .Skip(input.SkipRows())
            .Take(input.PageSize)
            .ToListAsync();

        return new PageModelDto<StudentDto>(input, Mapper.Map<List<StudentDto>>(entities), total);
    }
}
```

---

## 3. API Layer

### 3.1 Permission Constants

Define permission codes in `Api\Consts.cs`:

```csharp
public static class Student
{
    public const string Create = "student-create";
    public const string Update = "student-update";
    public const string Delete = "student-delete";
    public const string Search = "student-search";
    public const string Get = "student-get";
}
```

### 3.2 Writing the Controller

Create the Controller in `Api\Controllers`:

```csharp
namespace Adnc.Demo.Admin.Api.Controllers;

[Route($"{RouteConsts.AdminRoot}/students")]
[ApiController]
public class StudentController(IStudentService studentService) : AdncControllerBase
{
    [HttpPost]
    [AdncAuthorize(PermissionConsts.Student.Create)]
    public async Task<ActionResult<IdDto>> CreateAsync([FromBody] StudentCreationDto input)
        => CreatedResult(await studentService.CreateAsync(input));

    [HttpPut("{id}")]
    [AdncAuthorize(PermissionConsts.Student.Update)]
    public async Task<ActionResult<long>> UpdateAsync([FromRoute] long id, [FromBody] StudentUpdationDto input)
        => Result(await studentService.UpdateAsync(id, input));

    [HttpDelete("{ids}")]
    [AdncAuthorize(PermissionConsts.Student.Delete)]
    public async Task<ActionResult> DeleteAsync([FromRoute] string ids)
    {
        var idArr = ids.Split(',').Select(long.Parse).ToArray();
        return Result(await studentService.DeleteAsync(idArr));
    }

    [HttpGet("page")]
    [AdncAuthorize(PermissionConsts.Student.Search)]
    public async Task<ActionResult<PageModelDto<StudentDto>>> GetPagedAsync([FromQuery] StudentSearchPagedDto input)
        => await studentService.GetPagedAsync(input);

    [HttpGet("{id}")]
    [AdncAuthorize([PermissionConsts.Student.Get, PermissionConsts.Student.Update])]
    public async Task<ActionResult<StudentDto>> GetAsync([FromRoute] long id)
    {
        var dto = await studentService.GetAsync(id);
        return dto is null ? NotFound() : dto;
    }
}
```

---

## 4. Summary

By following this workflow, you can implement features efficiently and consistently within the ADNC architecture. Key takeaways: always return `Problem` for business errors, keep controllers "thin," and ensure all cross-cutting concerns (logging, auth, validation) are handled via framework-level abstractions.
