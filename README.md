# WeihanLi.EntityFramework

[![WeihanLi.EntityFramework](https://img.shields.io/nuget/v/WeihanLi.EntityFramework.svg)](https://www.nuget.org/packages/WeihanLi.EntityFramework/)

[![WeihanLi.EntityFramework Latest](https://img.shields.io/nuget/vpre/WeihanLi.EntityFramework)](https://www.nuget.org/packages/WeihanLi.EntityFramework/absoluteLatest)

[![Pipeline Build Status](https://weihanli.visualstudio.com/Pipelines/_apis/build/status/WeihanLi.WeihanLi.EntityFramework?branchName=dev)](https://weihanli.visualstudio.com/Pipelines/_build/latest?definitionId=11&branchName=dev)

![Github Build Status](https://github.com/WeihanLi/WeihanLi.EntityFramework/workflows/default/badge.svg)

## Intro

[EntityFrameworkCore](https://github.com/dotnet/efcore) extensions that provide a comprehensive set of tools and patterns to enhance your Entity Framework Core development experience.

WeihanLi.EntityFramework offers:

- **Repository Pattern** - Clean abstraction layer for data access
- **Unit of Work Pattern** - Transaction management across multiple repositories  
- **Automatic Auditing** - Track all entity changes with flexible storage options
- **Auto-Update Features** - Automatic handling of CreatedAt/UpdatedAt timestamps and user tracking
- **Soft Delete** - Mark entities as deleted without physical removal
- **Database Extensions** - Convenient methods for bulk operations and queries
- **Database Functions** - SQL Server JSON operations and more

## Quick Start

### 1. Installation

```bash
dotnet add package WeihanLi.EntityFramework
```

### 2. Basic Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add WeihanLi.EntityFramework services
builder.Services.AddEFRepository();
builder.Services.AddEFAutoUpdateInterceptor();
builder.Services.AddEFAutoAudit(auditBuilder =>
{
    auditBuilder.WithUserIdProvider<HttpContextUserIdProvider>()
                .WithStore<AuditDatabaseStore>();
});

var app = builder.Build();
```

### 3. Define Your Entities

```csharp
public class Product : IEntityWithCreatedUpdatedAt, ISoftDeleteEntityWithDeleted
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // Auto-update properties
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    // Soft delete property
    public bool IsDeleted { get; set; }
}
```

### 4. Use Repository Pattern

```csharp
public class ProductService
{
    private readonly IEFRepository<MyDbContext, Product> _repository;

    public ProductService(IEFRepository<MyDbContext, Product> repository)
    {
        _repository = repository;
    }

    public async Task<Product> CreateProductAsync(string name, decimal price)
    {
        var product = new Product { Name = name, Price = price };
        return await _repository.InsertAsync(product);
        // CreatedAt/UpdatedAt automatically set, audit record created
    }

    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _repository.GetListAsync(
            queryBuilder => queryBuilder.WithPredict(p => p.Price > 0)
        );
        // Soft deleted products automatically filtered out
    }
}
```

## Package Release Notes

See Releases/PRs for details

- Releases: https://github.com/WeihanLi/WeihanLi.EntityFramework/releases
- PRs: https://github.com/WeihanLi/WeihanLi.EntityFramework/pulls?q=is%3Apr+is%3Aclosed+is%3Amerged+base%3Amaster

> Package Versions
>
> For EF 8 and above, use 8.x or above major-version matched versions
>
> For EF 7, use 3.x
>
> For EF Core 5/6, use 2.x
>
> For EF Core 3.x, use 1.5.0 above, and 2.0.0 below
>
> For EF Core 2.x , use 1.4.x and below

## Features

### 🏗️ Repository Pattern
- `IEFRepository<TDbContext, TEntity>` - Generic repository interface
- `EFRepository` - Full-featured repository implementation
- `EFRepositoryGenerator` - Dynamic repository creation
- **Query Builder** - Fluent API for complex queries
- **Bulk Operations** - Efficient batch updates and deletes

### 🔄 Unit of Work Pattern  
- `IEFUnitOfWork<TDbContext>` - Transaction management
- **Multi-Repository Transactions** - Coordinate changes across entities
- **Rollback Support** - Automatic error handling

### 📋 Comprehensive Auditing
- **Automatic Change Tracking** - Monitor all entity modifications
- **Flexible Storage** - Database, file, console, or custom stores
- **Property Enrichment** - Add custom metadata to audit records
- **User Tracking** - Capture who made changes
- **Configurable Filtering** - Include/exclude entities and properties

### ⚡ Auto-Update Features
- **Timestamp Management** - Automatic CreatedAt/UpdatedAt handling
- **User Tracking** - Automatic CreatedBy/UpdatedBy population
- **Soft Delete** - Mark entities as deleted without removal
- **Custom Auto-Update** - Define your own auto-update rules

### 🔧 Database Extensions
- **Column Updates** - Update specific columns only
- **Bulk Operations** - Efficient mass updates
- **Query Helpers** - Get table/column names, check database type
- **Paging Support** - Built-in pagination for large datasets

### 🗄️ Database Functions
- **JSON Support** - `JSON_VALUE` for SQL Server 2016+
- **SQL Server Functions** - Enhanced querying capabilities

## Documentation

🚀 **[Getting Started Guide](docs/GettingStarted.md)** - Step-by-step setup instructions for new users

📖 **[Complete Usage Guide](docs/Usage.md)** - Comprehensive documentation with examples for all features

📋 **[Release Notes](docs/ReleaseNotes.md)** - Version history and breaking changes

🔧 **[Sample Project](samples/WeihanLi.EntityFramework.Sample/)** - Working examples and demonstrations

## Support

💡 **Questions?** Check out the [Usage Guide](docs/Usage.md) for detailed examples

🐛 **Found a bug?** [Create an issue](https://github.com/WeihanLi/WeihanLi.EntityFramework/issues/new) with reproduction steps

💬 **Need help?** Feel free to [start a discussion](https://github.com/WeihanLi/WeihanLi.EntityFramework/discussions) or create an issue

## Usage

For detailed usage instructions, please refer to the [Usage Documentation](docs/Usage.md).
