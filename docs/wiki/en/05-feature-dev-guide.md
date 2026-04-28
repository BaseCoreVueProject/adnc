# ADNC The complete development process using Student as an example
[GitHub repository](https://github.com/alphayu/adnc)

This article takes the Student entity as an example, combined with the project code style, to systematically explain the standard process for implementing the complete CRUD function from Entity definition to Controller layer in the ADNC project. The content covers key links such as Entity, EntityConfig, DTO, Validator, Service, AutoMapper, and Controller, aiming to standardize development practices and improve code consistency and maintainability.

---

## 1. Repository layer

### 1.1 Definition Entity

Create a new Student entity in the `Repository\Entities` directory:

1. The entity class must directly or indirectly inherit`EfEntity`.
2. If the entity contains fields`CreateBy`,`CreateByTime`,`ModifyBy`, and`ModifyTime`, it needs to inherit`EfFullAuditEntity`.
3. If the entity only contains`CreateBy`and`CreateByTime`fields, it needs to inherit`EfBasicAuditEntity`.
4. If the entity does not contain the above audit fields, it will inherit`EfEntity`.
5. If the entity needs to support soft deletion, implement the `ISoftDelete` interface.
6. If the entity needs to support optimistic locking (row concurrency control), implement the `IConcurrency` interface.

```csharp
namespace Adnc.Demo.Admin.Repository.Entities;

/// <summary>
/// Student
/// </summary>
public class Student : EfBasicAuditEntity, ISoftDelete
{
    public static readonly int Name_MaxLength = 50;

    /// <summary>
    /// student name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }
}
```

Register it in `EntityInfo.cs`:

```csharp
modelBuilder.Entity<Student>().ToTable("sch_student");
```

---

### 1.2 Definition EntityConfig

Create a new StudentConfig entity configuration class in the `Repository\Entities\Config` directory:

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

## 1.3 Database migrationAfter the entity definition is completed, database migration operations need to be performed:

```powershell
cd src
# Add migration
dotnet ef migrations add Student --project Repository\Adnc.Demo.Admin.Repository.csproj --startup-project Api\Adnc.Demo.Admin.Api.csproj
# Update database
dotnet ef database update --project Repository\Adnc.Demo.Admin.Repository.csproj --startup-project Api\Adnc.Demo.Admin.Api.csproj
```

---

## 2. Application/Application.Contracts layer

### 2.1 Configure AutoMapper

Add mapping configuration in `Application\AutoMapper\MapperProfile.cs`:

```csharp
CreateMap<Student, StudentDto>();
CreateMap<StudentCreationDto, Student>();
```

### 2.2 Create DTOs

Create new DTOs under `Application\Contracts\Dtos\Student`:

1. If the DTO contains CreateBy, CreateByTime, ModifyBy, and ModifyTime fields, it can inherit OutputFullAuditInfoDto.
2. If the DTO only contains CreateBy and CreateByTime fields, it can inherit OutputBaseAuditDto.
3. If the DTO does not contain the above audit fields, it can inherit InputDto or OutputDto.
4. Paged search DTOs must inherit SearchPagedDto.

```csharp
namespace Adnc.Demo.Admin.Application.Contracts.Dtos.Student;

/// <summary>
/// Represents the payload used to create a student.
/// </summary>
public class StudentCreationDto : InputDto
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

}

/// <summary>
/// Represents the payload used to update a student.
/// </summary>
public class StudentUpdationDto : StudentCreationDto
{ }

namespace Adnc.Demo.Admin.Application.Contracts.Dtos.Student;

/// <summary>
/// Represents the paging and filtering criteria used to search student records.
/// </summary>
public class StudentSearchPagedDto : SearchPagedDto
{ }

/// <summary>
/// Represents a student.
/// </summary>
[Serializable]
public class StudentDto : OutputBaseAuditDto
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

}
```

---

### 2.3 Writing Validators

Create a new validator class in the `Application\Contracts\Dtos\Student\Validators` directory:

```csharp
using Adnc.Demo.Admin.Application.Contracts.Dtos.Student;

namespace Adnc.Demo.Admin.Application.Validators;

/// <summary>
/// Validates <see cref="StudentCreationDto"/> instances.
/// </summary>
public class StudentCreationDtoValidator : AbstractValidator<StudentCreationDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StudentCreationDtoValidator"/> class.
    /// </summary>
    public StudentCreationDtoValidator()
    { 
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Student.Name_MaxLength);
    }
}

/// <summary>
/// Validates <see cref="StudentUpdationDto"/> instances.
/// </summary>
public class StudentUpdationDtoValidator : AbstractValidator<StudentUpdationDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StudentUpdationDtoValidator"/> class.
    /// </summary>
    public StudentUpdationDtoValidator()
    {
        Include(new StudentCreationDtoValidator());
    }
}
```

---

### 2.4 IStudentService Interface Definition

Create a new IStudentService under `Application\Contracts\Interfaces`:

1. The interface must inherit `IAppService`.
2. All write operations must return `ServiceResult` or `ServiceResult<T>`.
3. Read operations are recommended to return DTO types, but `ServiceResult<T>` can also be returned.
4. If transactions are required, add the `[UnitOfWork]` attribute or inject `IUnitOfWork` into the implementation class to start the transaction manually.
5. If CAP distributed transactions are required for publishing events, add `[UnitOfWork(Distributed = true)]` or inject `IUnitOfWork` into the implementation class to start the transaction manually.

```csharp
using Adnc.Demo.Admin.Application.Contracts.Dtos.Student;

namespace Adnc.Demo.Admin.Application.Contracts.Interfaces;

/// <summary>
/// Defines student management services.
/// </summary>
public interface IStudentService : IAppService
{
    /// <summary>
    /// Creates a student.
    /// </summary>
    /// <param name="input">The student to create.</param>
    /// <returns>The ID of the created student.</returns>
    [OperateLog(LogName = "Create student")]
    Task<ServiceResult<IdDto>> CreateAsync(StudentCreationDto input);

    /// <summary>
    /// Updates a student.
    /// </summary>
    /// <param name="id">The student ID.</param>
    /// <param name="input">The student changes.</param>
    /// <returns>A result indicating whether the student was updated.</returns>
    [OperateLog(LogName = "Update student")]
    Task<ServiceResult> UpdateAsync(long id, StudentUpdationDto input);

    /// <summary>
    /// Deletes one or more student records.
    /// </summary>
    /// <param name="ids">The student IDs.</param>
    /// <returns>A result indicating whether the records were deleted.</returns>
    [OperateLog(LogName = "Delete student")]
    Task<ServiceResult> DeleteAsync(long[] ids);

    /// <summary>
    /// Gets a student by ID.
    /// </summary>
    /// <param name="id">The student ID.</param>
    /// <returns>The requested student, or <c>null</c> if it does not exist.</returns>
    Task<StudentDto?> GetAsync(long id);

    /// <summary>
    /// Gets a paged list of student records.
    /// </summary>
    /// <param name="input">The paging and filtering criteria.</param>
    /// <returns>A paged list of student records.</returns>
    Task<PageModelDto<StudentDto>> GetPagedAsync(StudentSearchPagedDto input);
}
```

### 2.5 Implement IStudentService

Create a new StudentService under `Application\Services`:

1. The service class must inherit AbstractAppService.
2. When business validation fails, return Problem consistently instead of throwing exceptions.

```csharp
using Adnc.Demo.Admin.Application.Contracts.Dtos.Student;

namespace Adnc.Demo.Admin.Application.Services;

/// <inheritdoc cref="IStudentService"/>
public class StudentService(IEfRepository<Student> studentRepo)
    : AbstractAppService, IStudentService
{
    /// <inheritdoc />
    public async Task<ServiceResult<IdDto>> CreateAsync(StudentCreationDto input)
    {
        input.TrimStringFields();
        var nameExists = await studentRepo.AnyAsync(x => x.Name == input.Name);
        if (nameExists)
        {
            return Problem(HttpStatusCode.BadRequest, "This student name already exists");
        }
        var entity = Mapper.Map<Student>(input, IdGenerater.GetNextId());
        await studentRepo.InsertAsync(entity);
        return new IdDto(entity.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> UpdateAsync(long id, StudentUpdationDto input)
    {
        input.TrimStringFields();
        var entity = await studentRepo.FetchAsync(x => x.Id == id, noTracking: false);
        if (entity is null)
        {
            return Problem(HttpStatusCode.NotFound, "This student does not exist");
        }

        var nameExists = await studentRepo.AnyAsync(x => x.Name == input.Name && x.Id != id);
        if (nameExists)
        {
            return Problem(HttpStatusCode.BadRequest, "This student name already exists");
        }
        var newEntity = Mapper.Map(input, entity);
        await studentRepo.UpdateAsync(newEntity);
        return ServiceResult();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(long[] ids)
    {
        await studentRepo.ExecuteDeleteAsync(x => ids.Contains(x.Id));
        return ServiceResult();
    }

    /// <inheritdoc />
    public async Task<StudentDto?> GetAsync(long id)
    {
        var entity = await studentRepo.FetchAsync(x => x.Id == id);
        return entity is null ? null : Mapper.Map<StudentDto>(entity);
    }

    /// <inheritdoc />
    public async Task<PageModelDto<StudentDto>> GetPagedAsync(StudentSearchPagedDto input)
    {
        input.TrimStringFields();
        var whereExpr = ExpressionCreator
            .New<Student>()
            .AndIf(input.Keywords.IsNotNullOrWhiteSpace(), x => EF.Functions.Like(x.Name, $"{input.Keywords}%"));

        var total = await studentRepo.CountAsync(whereExpr);
        if (total == 0)
        {
            return new PageModelDto<StudentDto>(input);
        }

        var entities = await studentRepo
                                        .Where(whereExpr)
                                        .OrderByDescending(x => x.Id)
                                        .Skip(input.SkipRows())
                                        .Take(input.PageSize)
                                        .ToListAsync();
        var dtos = Mapper.Map<List<StudentDto>>(entities);
        return new PageModelDto<StudentDto>(input, dtos, total);
    }
}
```

---

## 3. API layer

### 3.1 Permission Constant Definition

Permission constant definitions can be added in the `Api\Consts.cs` file (optional):

```csharp
/// <summary>
/// Defines permission codes for student management.
/// </summary>
public static class Student
{
    public const string Create = "student-create";
    public const string Update = "student-update";
    public const string Delete = "student-delete";
    public const string Search = "student-search";
    public const string Get = "student-get";
}
```

### 3.2 Writing Controller

Create a new Controller class in the `Api\Controllers` directory:

1. The controller class must inherit AdncControllerBase.
2. Each method can add permission attributes as needed (optional):

```csharp
using Adnc.Demo.Admin.Application.Contracts.Dtos.Student;

namespace Adnc.Demo.Admin.Api.Controllers;

/// <summary>
/// Manages students.
/// </summary>
[Route($"{RouteConsts.AdminRoot}/students")]
[ApiController]
public class StudentController(IStudentService studentService) : AdncControllerBase
{
    /// <summary>
    /// Creates a student.
    /// </summary>
    /// <param name="input">The student to create.</param>
    /// <returns>The ID of the created student.</returns>
    [HttpPost]
    [AdncAuthorize(PermissionConsts.Student.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<IdDto>> CreateAsync([FromBody] StudentCreationDto input)
        => CreatedResult(await studentService.CreateAsync(input));

    /// <summary>
    /// Updates a student.
    /// </summary>
    /// <param name="id">The student ID.</param>
    /// <param name="input">The student changes.</param>
    /// <returns>A result indicating whether the student was updated.</returns>
    [HttpPut("{id}")]
    [AdncAuthorize(PermissionConsts.Student.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<long>> UpdateAsync([FromRoute] long id, [FromBody] StudentUpdationDto input)
        => Result(await studentService.UpdateAsync(id, input));

    /// <summary>
    /// Deletes one or more student records.
    /// </summary>
    /// <param name="ids">The comma-separated student IDs.</param>
    /// <returns>A result indicating whether the records were deleted.</returns>
    [HttpDelete("{ids}")]
    [AdncAuthorize(PermissionConsts.Student.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteAsync([FromRoute] string ids)
    {
        var idArr = ids.Split(',').Select(long.Parse).ToArray();
        return Result(await studentService.DeleteAsync(idArr));
    }

    /// <summary>
    /// Gets a paged list of student records.
    /// </summary>
    /// <param name="input">The paging and filtering criteria.</param>
    /// <returns>A paged list of student records.</returns>
    [HttpGet("page")]
    [AdncAuthorize(PermissionConsts.Student.Search)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PageModelDto<StudentDto>>> GetPagedAsync([FromQuery] StudentSearchPagedDto input)
        => await studentService.GetPagedAsync(input);

    /// <summary>
    /// Gets a student by ID.
    /// </summary>
    /// <param name="id">The student ID.</param>
    /// <returns>The requested student.</returns>
    [HttpGet("{id}")]
    [AdncAuthorize([PermissionConsts.Student.Get, PermissionConsts.Student.Update])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetAsync([FromRoute] long id)
    {
        var dto = await studentService.GetAsync(id);
        return dto is null ? NotFound() : dto;
    }
}
```

---

## 4. Summary

Following the process above, the complete Student entity functionality can be implemented consistently and efficiently under the ADNC architecture. During development, note that all write operations should return Problem consistently when business validation fails, rather than throwing exceptions. This keeps business exception handling consistent and front-end/back-end interactions predictable, improving maintainability and scalability.
