# Getting Started with WeihanLi.EntityFramework

This guide will help you get up and running with WeihanLi.EntityFramework quickly.

## Prerequisites

- .NET 8.0 or later
- Entity Framework Core 8.0 or later
- A supported database provider (SQL Server, SQLite, PostgreSQL, etc.)

## Step 1: Installation

Add the package to your project:

```bash
dotnet add package WeihanLi.EntityFramework
```

## Step 2: Define Your Entities

Create your entity classes and implement the desired interfaces for automatic features:

```csharp
using WeihanLi.EntityFramework;

// Basic entity with auto-update timestamps
public class Product : IEntityWithCreatedUpdatedAt
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    
    // These will be automatically managed
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

// Entity with soft delete capability
public class Category : ISoftDeleteEntityWithDeleted
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Soft delete property
    public bool IsDeleted { get; set; }
}

// Entity with full audit trail
public class Order : IEntityWithCreatedUpdatedAt, IEntityWithCreatedUpdatedBy
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    
    // Auto-managed timestamp fields
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    // Auto-managed user tracking fields
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public List<OrderItem> Items { get; set; } = new();
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
```

## Step 3: Configure Your DbContext

Set up your DbContext with the necessary configurations:

```csharp
using Microsoft.EntityFrameworkCore;
using WeihanLi.EntityFramework.Audit;

public class AppDbContext : AuditDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider) 
        : base(options, serviceProvider)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<AuditRecord> AuditRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasIndex(e => e.Name);
        });

        // Configure Category with soft delete filter
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasQueryFilter(c => !c.IsDeleted); // Global soft delete filter
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasMany(e => e.Items)
                  .WithOne()
                  .HasForeignKey("OrderId");
        });
    }
}
```

## Step 4: Configure Services

Set up dependency injection in your `Program.cs`:

```csharp
using WeihanLi.EntityFramework;
using WeihanLi.EntityFramework.Audit;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<AppDbContext>((provider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(
               provider.GetRequiredService<AutoUpdateInterceptor>(),
               provider.GetRequiredService<AuditInterceptor>()
           );
});

// Add WeihanLi.EntityFramework services
builder.Services.AddEFRepository();
builder.Services.AddEFAutoUpdateInterceptor();

// Configure user ID provider for audit and auto-update
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUserIdProvider, HttpContextUserIdProvider>();

// Configure audit system
builder.Services.AddEFAutoAudit(auditBuilder =>
{
    auditBuilder
        .WithUserIdProvider<HttpContextUserIdProvider>()
        .EnrichWithProperty("ApplicationName", "MyApplication")
        .EnrichWithProperty("MachineName", Environment.MachineName)
        .WithStore<AuditDatabaseStore>() // Store audit records in database
        .IgnoreEntity<AuditRecord>(); // Don't audit the audit records themselves
});

var app = builder.Build();

// Ensure database is created (for development)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
```

## Step 5: Create a User ID Provider

Implement a user ID provider to track who makes changes:

```csharp
using System.Security.Claims;
using WeihanLi.Common.Services;

public class HttpContextUserIdProvider : IUserIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextUserIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        // Try to get user ID from claims
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user?.FindFirst("sub")?.Value
                  ?? user?.Identity?.Name;

        return userId ?? "Anonymous";
    }
}
```

## Step 6: Create Your First Service

Create a service that uses the repository pattern:

```csharp
using WeihanLi.EntityFramework;

public class ProductService
{
    private readonly IEFRepository<AppDbContext, Product> _productRepository;
    private readonly IEFUnitOfWork<AppDbContext> _unitOfWork;

    public ProductService(
        IEFRepository<AppDbContext, Product> productRepository,
        IEFUnitOfWork<AppDbContext> unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Product> CreateProductAsync(string name, decimal price)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            IsActive = true
            // CreatedAt and UpdatedAt will be set automatically
        };

        return await _productRepository.InsertAsync(product);
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _productRepository.FindAsync(id);
    }

    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _productRepository.GetListAsync(
            queryBuilder => queryBuilder.WithPredict(p => p.IsActive)
        );
    }

    public async Task<bool> UpdateProductPriceAsync(int productId, decimal newPrice)
    {
        var product = await _productRepository.FindAsync(productId);
        if (product == null) return false;

        product.Price = newPrice;
        // UpdatedAt will be set automatically

        var result = await _productRepository.UpdateAsync(product);
        return result > 0;
    }

    public async Task<bool> DeactivateProductAsync(int productId)
    {
        // Use bulk update for efficiency
        var result = await _productRepository.UpdateAsync(
            setters => setters.SetProperty(p => p.IsActive, false),
            queryBuilder => queryBuilder.WithPredict(p => p.Id == productId)
        );

        return result > 0;
    }

    public async Task<IPagedListModel<Product>> GetProductsPagedAsync(int page, int pageSize)
    {
        return await _productRepository.GetPagedListAsync(
            queryBuilder => queryBuilder
                .WithPredict(p => p.IsActive)
                .WithOrderBy(q => q.OrderBy(p => p.Name)),
            page,
            pageSize
        );
    }
}
```

## Step 7: Create a Controller (ASP.NET Core)

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IPagedListModel<Product>>> GetProducts(
        int page = 1, 
        int pageSize = 20)
    {
        var products = await _productService.GetProductsPagedAsync(page, pageSize);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(CreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(request.Name, request.Price);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdatePrice(int id, UpdatePriceRequest request)
    {
        var success = await _productService.UpdateProductPriceAsync(id, request.Price);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UpdatePriceRequest
{
    public decimal Price { get; set; }
}
```

## What Happens Automatically

When you run this setup, WeihanLi.EntityFramework will automatically:

1. **Set timestamps**: `CreatedAt` when inserting, `UpdatedAt` when updating
2. **Track users**: `CreatedBy` and `UpdatedBy` using your user ID provider
3. **Create audit records**: Every change is logged with full details
4. **Apply soft delete filters**: Soft-deleted entities are excluded from queries
5. **Handle transactions**: Unit of Work ensures data consistency

## Next Steps

- 📖 Read the [Complete Usage Guide](Usage.md) for advanced features
- ⚡ Explore [Advanced Features Guide](AdvancedFeatures.md) for custom interceptors and optimization
- 🔍 Explore the [sample project](../samples/WeihanLi.EntityFramework.Sample/) for more examples
- 🛠️ Check out bulk operations, advanced querying, and custom audit stores
- 📋 Review [Release Notes](ReleaseNotes.md) for version-specific information

## Common Patterns

### Repository with Unit of Work

```csharp
public async Task ProcessOrderAsync(CreateOrderRequest request)
{
    var orderRepo = _unitOfWork.GetRepository<Order>();
    var productRepo = _unitOfWork.GetRepository<Product>();
    
    // Create order
    var order = await orderRepo.InsertAsync(new Order 
    { 
        CustomerId = request.CustomerId,
        TotalAmount = request.Items.Sum(i => i.Price * i.Quantity)
    });
    
    // Add order items and update inventory
    foreach (var item in request.Items)
    {
        await orderRepo.InsertAsync(new OrderItem
        {
            OrderId = order.Id,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.Price
        });
        
        // Update product inventory (example)
        await productRepo.UpdateAsync(
            setters => setters.SetProperty(p => p.Stock, p => p.Stock - item.Quantity),
            queryBuilder => queryBuilder.WithPredict(p => p.Id == item.ProductId)
        );
    }
    
    // Commit all changes in a single transaction
    await _unitOfWork.CommitAsync();
}
```

This gets you started with the core features. The library handles the complexity while giving you clean, testable code!