# Usage

## Installation

To install the `WeihanLi.EntityFramework` package, you can use the NuGet Package Manager, .NET CLI, or PackageReference in your project file.

**.NET CLI**

```
dotnet add package WeihanLi.EntityFramework
```

## Configuration

To configure the `WeihanLi.EntityFramework` package, you need to add the necessary services to your `IServiceCollection` in the `Startup.cs` or `Program.cs` file.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<MyDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

    services.AddEFRepository();
    services.AddEFAutoUpdateInterceptor();
    services.AddEFAutoAudit(builder =>
    {
        builder.WithUserIdProvider<CustomUserIdProvider>()
               .EnrichWithProperty("MachineName", Environment.MachineName)
               .WithStore<AuditConsoleStore>();
    });
}
```

## Examples

### Repository Pattern

The `WeihanLi.EntityFramework` package provides a repository pattern implementation for Entity Framework Core.

```csharp
public class MyService
{
    private readonly IEFRepository<MyDbContext, MyEntity> _repository;

    public MyService(IEFRepository<MyDbContext, MyEntity> repository)
    {
        _repository = repository;
    }

    public async Task<MyEntity> GetEntityByIdAsync(int id)
    {
        return await _repository.FindAsync(id);
    }

    public async Task AddEntityAsync(MyEntity entity)
    {
        await _repository.InsertAsync(entity);
    }

    public async Task UpdateEntityAsync(MyEntity entity)
    {
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteEntityAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}
```

### Unit of Work Pattern

The `WeihanLi.EntityFramework` package also provides a unit of work pattern implementation for Entity Framework Core.

```csharp
public class MyService
{
    private readonly IEFUnitOfWork<MyDbContext> _unitOfWork;

    public MyService(IEFUnitOfWork<MyDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task SaveChangesAsync()
    {
        await _unitOfWork.CommitAsync();
    }
}
```

### Audit

The `WeihanLi.EntityFramework` package provides an audit feature to track changes in your entities.

```csharp
public class MyDbContext : AuditDbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options, IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }

    public DbSet<MyEntity> MyEntities { get; set; }
}
```

### Soft Delete

The `WeihanLi.EntityFramework` package provides a soft delete feature to mark entities as deleted without actually removing them from the database.

```csharp
public class MyEntity : ISoftDeleteEntityWithDeleted
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }
}
```

### Auto Update

The `WeihanLi.EntityFramework` package provides an auto update feature to automatically update certain properties, such as `CreatedAt`, `UpdatedAt`, `CreatedBy`, and `UpdatedBy`.

```csharp
public class MyEntity : IEntityWithCreatedUpdatedAt, IEntityWithCreatedUpdatedBy
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}
```
