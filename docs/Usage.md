# WeihanLi.EntityFramework Usage Guide

## Installation

To install the `WeihanLi.EntityFramework` package, you can use the NuGet Package Manager, .NET CLI, or PackageReference in your project file.

### .NET CLI

```bash
dotnet add package WeihanLi.EntityFramework
```

### Package Manager Console

```powershell
Install-Package WeihanLi.EntityFramework
```

### PackageReference

```xml
<PackageReference Include="WeihanLi.EntityFramework" Version="8.0.0" />
```

> **Version Compatibility**
> - For EF 8 and above: use 8.x or above major-version matched versions
> - For EF 7: use 3.x
> - For EF Core 5/6: use 2.x
> - For EF Core 3.x: use 1.5.0 above, and 2.0.0 below
> - For EF Core 2.x: use 1.4.x and below

## Quick Start

### Basic Configuration

Configure the services in your `Startup.cs` or `Program.cs` file:

```csharp
// Program.cs (minimal hosting model)
var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add WeihanLi.EntityFramework services
builder.Services.AddEFRepository();
builder.Services.AddEFAutoUpdateInterceptor();
builder.Services.AddEFAutoAudit(builder =>
{
    builder.WithUserIdProvider<CustomUserIdProvider>()
           .EnrichWithProperty("MachineName", Environment.MachineName)
           .WithStore<AuditConsoleStore>();
});

var app = builder.Build();
```

### Complete Configuration Example

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure DbContext with interceptors
    services.AddDbContext<MyDbContext>((provider, options) =>
    {
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(
                   provider.GetRequiredService<AutoUpdateInterceptor>(),
                   provider.GetRequiredService<AuditInterceptor>()
               );
    });

    // Add repository services
    services.AddEFRepository();
    
    // Add auto-update interceptor for automatic CreatedAt/UpdatedAt handling
    services.AddEFAutoUpdateInterceptor();
    
    // Configure audit system
    services.AddEFAutoAudit(builder =>
    {
        builder.WithUserIdProvider<CustomUserIdProvider>()
               .EnrichWithProperty("MachineName", Environment.MachineName)
               .EnrichWithProperty("ApplicationName", "MyApp")
               .WithStore<AuditDatabaseStore>()
               .IgnoreEntity<AuditRecord>()
               .IgnoreProperty("CreatedAt");
    });
}
```

```

### Custom User ID Provider

```csharp
public class CustomUserIdProvider : IUserIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CustomUserIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    }
}
```

## Features and Examples

### 1. Repository Pattern

The `WeihanLi.EntityFramework` package provides a clean repository pattern implementation for Entity Framework Core.

#### Basic Repository Usage

```csharp
public class ProductService
{
    private readonly IEFRepository<MyDbContext, Product> _repository;

    public ProductService(IEFRepository<MyDbContext, Product> repository)
    {
        _repository = repository;
    }

    // Find by primary key
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _repository.FindAsync(id);
    }

    // Insert single entity
    public async Task<Product> CreateProductAsync(Product product)
    {
        return await _repository.InsertAsync(product);
    }

    // Insert multiple entities
    public async Task<int> CreateProductsAsync(List<Product> products)
    {
        return await _repository.InsertAsync(products);
    }

    // Update entity
    public async Task<int> UpdateProductAsync(Product product)
    {
        return await _repository.UpdateAsync(product);
    }

    // Update specific columns only
    public async Task<int> UpdateProductNameAsync(int id, string newName)
    {
        return await _repository.UpdateAsync(
            new Product { Id = id, Name = newName },
            p => p.Name  // Only update Name property
        );
    }

    // Update without specific columns
    public async Task<int> UpdateProductWithoutTimestampAsync(Product product)
    {
        return await _repository.UpdateWithoutAsync(product, p => p.UpdatedAt);
    }

    // Delete by primary key
    public async Task<int> DeleteProductAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    // Delete entity
    public async Task<int> DeleteProductAsync(Product product)
    {
        return await _repository.DeleteAsync(product);
    }
}
```

#### Advanced Query Operations

```csharp
public class ProductQueryService
{
    private readonly IEFRepository<MyDbContext, Product> _repository;

    public ProductQueryService(IEFRepository<MyDbContext, Product> repository)
    {
        _repository = repository;
    }

    // Get first product with condition
    public async Task<Product?> GetFirstActiveProductAsync()
    {
        return await _repository.FirstOrDefaultAsync(
            queryBuilder => queryBuilder.WithPredict(p => p.IsActive)
        );
    }

    // Get products with paging
    public async Task<IPagedListModel<Product>> GetProductsPagedAsync(int page, int size)
    {
        return await _repository.GetPagedListAsync(
            queryBuilder => queryBuilder
                .WithPredict(p => p.IsActive)
                .WithOrderBy(q => q.OrderByDescending(p => p.CreatedAt)),
            page,
            size
        );
    }

    // Get products with custom projection
    public async Task<List<ProductDto>> GetProductSummariesAsync()
    {
        return await _repository.GetResultAsync(
            p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            },
            queryBuilder => queryBuilder.WithPredict(p => p.IsActive)
        );
    }

    // Count products
    public async Task<int> GetActiveProductCountAsync()
    {
        return await _repository.CountAsync(
            queryBuilder => queryBuilder.WithPredict(p => p.IsActive)
        );
    }

    // Check if any products exist
    public async Task<bool> HasExpensiveProductsAsync()
    {
        return await _repository.AnyAsync(
            queryBuilder => queryBuilder.WithPredict(p => p.Price > 1000)
        );
    }

    // Raw query access
    public IQueryable<Product> GetProductsQuery()
    {
        return _repository.Query(
            queryBuilder => queryBuilder
                .WithPredict(p => p.IsActive)
                .WithOrderBy(q => q.OrderBy(p => p.Name))
        );
    }
}
```

#### Bulk Operations

```csharp
public class ProductBulkService
{
    private readonly IEFRepository<MyDbContext, Product> _repository;

    public ProductBulkService(IEFRepository<MyDbContext, Product> repository)
    {
        _repository = repository;
    }

    // Bulk update using expression
    public async Task<int> UpdatePricesAsync(decimal multiplier)
    {
        return await _repository.UpdateAsync(
            setters => setters.SetProperty(p => p.Price, p => p.Price * multiplier),
            queryBuilder => queryBuilder.WithPredict(p => p.IsActive)
        );
    }

    // Bulk delete with condition
    public async Task<int> DeleteInactiveProductsAsync()
    {
        return await _repository.DeleteAsync(
            queryBuilder => queryBuilder.WithPredict(p => !p.IsActive)
        );
    }
}
```

### 2. Unit of Work Pattern

The Unit of Work pattern helps manage transactions and ensures data consistency across multiple repository operations.

#### Basic Unit of Work Usage

```csharp
public class OrderService
{
    private readonly IEFUnitOfWork<MyDbContext> _unitOfWork;

    public OrderService(IEFUnitOfWork<MyDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Order> CreateOrderWithItemsAsync(Order order, List<OrderItem> items)
    {
        // Get repositories through unit of work
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var itemRepo = _unitOfWork.GetRepository<OrderItem>();

        // All operations within the same transaction
        var createdOrder = await orderRepo.InsertAsync(order);
        
        foreach (var item in items)
        {
            item.OrderId = createdOrder.Id;
            await itemRepo.InsertAsync(item);
        }

        // Commit all changes in a single transaction
        await _unitOfWork.CommitAsync();
        
        return createdOrder;
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var auditRepo = _unitOfWork.GetRepository<OrderAudit>();

        var order = await orderRepo.FindAsync(orderId);
        if (order != null)
        {
            var oldStatus = order.Status;
            order.Status = newStatus;
            
            await orderRepo.UpdateAsync(order);
            
            // Add audit record
            await auditRepo.InsertAsync(new OrderAudit
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.UtcNow
            });

            await _unitOfWork.CommitAsync();
        }
    }

    public async Task TransferProductsBetweenWarehousesAsync(
        int fromWarehouseId, 
        int toWarehouseId, 
        List<ProductTransfer> transfers)
    {
        var inventoryRepo = _unitOfWork.GetRepository<InventoryItem>();
        var transferRepo = _unitOfWork.GetRepository<ProductTransfer>();

        foreach (var transfer in transfers)
        {
            // Reduce inventory in source warehouse
            await inventoryRepo.UpdateAsync(
                setters => setters.SetProperty(i => i.Quantity, i => i.Quantity - transfer.Quantity),
                queryBuilder => queryBuilder.WithPredict(i => 
                    i.WarehouseId == fromWarehouseId && i.ProductId == transfer.ProductId)
            );

            // Increase inventory in destination warehouse
            await inventoryRepo.UpdateAsync(
                setters => setters.SetProperty(i => i.Quantity, i => i.Quantity + transfer.Quantity),
                queryBuilder => queryBuilder.WithPredict(i => 
                    i.WarehouseId == toWarehouseId && i.ProductId == transfer.ProductId)
            );

            // Record the transfer
            transfer.TransferDate = DateTime.UtcNow;
            await transferRepo.InsertAsync(transfer);
        }

        await _unitOfWork.CommitAsync();
    }
}
```

#### Advanced Unit of Work with Error Handling

```csharp
public class AdvancedOrderService
{
    private readonly IEFUnitOfWork<MyDbContext> _unitOfWork;
    private readonly ILogger<AdvancedOrderService> _logger;

    public AdvancedOrderService(
        IEFUnitOfWork<MyDbContext> unitOfWork,
        ILogger<AdvancedOrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> ProcessComplexOrderAsync(ComplexOrder complexOrder)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Step 1: Create order
            var orderRepo = _unitOfWork.GetRepository<Order>();
            var order = await orderRepo.InsertAsync(complexOrder.Order);

            // Step 2: Process payment
            var paymentRepo = _unitOfWork.GetRepository<Payment>();
            var payment = await paymentRepo.InsertAsync(new Payment
            {
                OrderId = order.Id,
                Amount = complexOrder.TotalAmount,
                PaymentMethod = complexOrder.PaymentMethod
            });

            // Step 3: Update inventory
            var inventoryRepo = _unitOfWork.GetRepository<InventoryItem>();
            foreach (var item in complexOrder.Items)
            {
                var inventoryItem = await inventoryRepo.FirstOrDefaultAsync(
                    qb => qb.WithPredict(i => i.ProductId == item.ProductId)
                );

                if (inventoryItem == null || inventoryItem.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient inventory for product {item.ProductId}");
                }

                inventoryItem.Quantity -= item.Quantity;
                await inventoryRepo.UpdateAsync(inventoryItem);
            }

            // Step 4: Send notifications (simulated)
            await SendOrderConfirmationAsync(order.Id);

            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing complex order");
            await _unitOfWork.RollbackAsync();
            return false;
        }
    }

    private async Task SendOrderConfirmationAsync(int orderId)
    {
        // Simulate notification service
        await Task.Delay(100);
        _logger.LogInformation($"Order confirmation sent for order {orderId}");
    }
}
```

### 3. Audit System

The audit system automatically tracks changes to your entities, providing a complete history of data modifications.

#### Setting up Audit DbContext

```csharp
public class MyDbContext : AuditDbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options, IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<AuditRecord> AuditRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure your entities
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });
    }
}
```

#### Audit Configuration Options

```csharp
// Basic audit configuration
services.AddEFAutoAudit(builder =>
{
    builder.WithUserIdProvider<CustomUserIdProvider>()
           .WithStore<AuditDatabaseStore>();
});

// Advanced audit configuration
services.AddEFAutoAudit(builder =>
{
    builder
        // Configure user ID provider
        .WithUserIdProvider<CustomUserIdProvider>()
        
        // Add custom properties to audit records
        .EnrichWithProperty("ApplicationName", "MyApplication")
        .EnrichWithProperty("MachineName", Environment.MachineName)
        .EnrichWithProperty("Version", Assembly.GetExecutingAssembly().GetName().Version?.ToString())
        
        // Configure storage
        .WithStore<AuditConsoleStore>()           // Console output
        .WithStore<AuditFileStore>()              // File storage
        .WithStore<AuditDatabaseStore>()          // Database storage
        
        // Ignore specific entities
        .IgnoreEntity<AuditRecord>()
        .IgnoreEntity<SystemLog>()
        
        // Ignore specific properties
        .IgnoreProperty<Product>(p => p.InternalNotes)
        .IgnoreProperty("Password")               // Ignore by property name
        .IgnoreProperty("CreatedAt")
        
        // Include unmodified properties (default is false)
        .WithUnModifiedProperty();
});
```

#### Custom Audit Store

```csharp
public class CustomAuditStore : IAuditStore
{
    private readonly ILogger<CustomAuditStore> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CustomAuditStore(ILogger<CustomAuditStore> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task Save(ICollection<AuditEntry> auditEntries)
    {
        foreach (var entry in auditEntries)
        {
            _logger.LogInformation("Audit: {Operation} on {EntityType} with Id {EntityId} by {UserId}",
                entry.OperationType,
                entry.EntityType,
                entry.EntityId,
                entry.UserId);

            // Custom storage logic (e.g., send to external system)
            await SendToExternalAuditSystemAsync(entry);
        }
    }

    private async Task SendToExternalAuditSystemAsync(AuditEntry entry)
    {
        // Implement custom audit storage logic
        await Task.CompletedTask;
    }
}
```

#### Using Audit Interceptor Manually

```csharp
services.AddDbContext<MyDbContext>((provider, options) =>
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(provider.GetRequiredService<AuditInterceptor>());
});

// Register the interceptor
services.AddSingleton<AuditInterceptor>();
```

#### Querying Audit Records

```csharp
public class AuditQueryService
{
    private readonly IEFRepository<MyDbContext, AuditRecord> _auditRepository;

    public AuditQueryService(IEFRepository<MyDbContext, AuditRecord> auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<List<AuditRecord>> GetUserActionsAsync(string userId, DateTime from, DateTime to)
    {
        return await _auditRepository.GetListAsync(
            queryBuilder => queryBuilder
                .WithPredict(a => a.UserId == userId && a.DateTime >= from && a.DateTime <= to)
                .WithOrderBy(q => q.OrderByDescending(a => a.DateTime))
        );
    }

    public async Task<List<AuditRecord>> GetEntityHistoryAsync(string entityType, string entityId)
    {
        return await _auditRepository.GetListAsync(
            queryBuilder => queryBuilder
                .WithPredict(a => a.EntityType == entityType && a.EntityId == entityId)
                .WithOrderBy(q => q.OrderBy(a => a.DateTime))
        );
    }

    public async Task<Dictionary<string, int>> GetDailyOperationStatsAsync(DateTime date)
    {
        var records = await _auditRepository.GetListAsync(
            queryBuilder => queryBuilder.WithPredict(a => a.DateTime.Date == date.Date)
        );

        return records.GroupBy(r => r.OperationType.ToString())
                     .ToDictionary(g => g.Key, g => g.Count());
    }
}
```

### 4. Soft Delete

The soft delete feature marks entities as deleted without actually removing them from the database, allowing for data recovery and audit trails.

#### Entity Configuration for Soft Delete

```csharp
// Option 1: Using ISoftDeleteEntityWithDeleted interface
public class Product : ISoftDeleteEntityWithDeleted
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; }  // Required by interface
}

// Option 2: Using ISoftDeleteEntity interface (with custom deleted property name)
public class Category : ISoftDeleteEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Deleted { get; set; }  // Custom property name
}

// Option 3: Custom soft delete implementation
public class CustomSoftDeleteEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
    public bool IsActive => DeletedAt == null;
}
```

#### DbContext Configuration for Soft Delete

```csharp
public class SoftDeleteDbContext : DbContext
{
    public SoftDeleteDbContext(DbContextOptions<SoftDeleteDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure global query filter for soft delete
        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<Category>()
            .HasQueryFilter(c => !c.Deleted);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Add the soft delete interceptor
        optionsBuilder.AddInterceptors(new SoftDeleteInterceptor());
        base.OnConfiguring(optionsBuilder);
    }
}
```

#### Using Soft Delete with Services

```csharp
// Configure soft delete in Startup.cs
services.AddSingleton<IUserIdProvider, EnvironmentUserIdProvider>();
services.AddEFAutoUpdateInterceptor();

services.AddDbContext<SoftDeleteDbContext>((provider, options) =>
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(provider.GetRequiredService<AutoUpdateInterceptor>());
});
```

#### Soft Delete Operations

```csharp
public class ProductService
{
    private readonly IEFRepository<SoftDeleteDbContext, Product> _repository;
    private readonly SoftDeleteDbContext _context;

    public ProductService(
        IEFRepository<SoftDeleteDbContext, Product> repository,
        SoftDeleteDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    // Regular delete - will soft delete the entity
    public async Task<int> DeleteProductAsync(int productId)
    {
        return await _repository.DeleteAsync(productId);
    }

    // Get active products (soft deleted items are filtered out automatically)
    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _repository.GetListAsync();
    }

    // Get all products including soft deleted ones
    public async Task<List<Product>> GetAllProductsIncludingDeletedAsync()
    {
        return await _context.Products
            .IgnoreQueryFilters()  // Ignore the soft delete filter
            .ToListAsync();
    }

    // Get only soft deleted products
    public async Task<List<Product>> GetDeletedProductsAsync()
    {
        return await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.IsDeleted)
            .ToListAsync();
    }

    // Restore a soft deleted product
    public async Task<int> RestoreProductAsync(int productId)
    {
        var product = await _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsDeleted);

        if (product != null)
        {
            product.IsDeleted = false;
            return await _context.SaveChangesAsync();
        }

        return 0;
    }

    // Permanently delete a soft deleted product
    public async Task<int> PermanentlyDeleteProductAsync(int productId)
    {
        var product = await _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsDeleted);

        if (product != null)
        {
            _context.Products.Remove(product);
            return await _context.SaveChangesAsync();
        }

        return 0;
    }

    // Bulk soft delete
    public async Task<int> BulkDeleteInactiveProductsAsync()
    {
        return await _repository.UpdateAsync(
            setters => setters.SetProperty(p => p.IsDeleted, true),
            queryBuilder => queryBuilder.WithPredict(p => !p.IsActive)
        );
    }
}
```

### 5. Auto Update Features

The auto-update system automatically manages timestamp and user fields when entities are created or modified.

#### Entity Interfaces for Auto Update

```csharp
// For automatic CreatedAt/UpdatedAt timestamps
public class Product : IEntityWithCreatedUpdatedAt
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; }    // Automatically set on insert
    public DateTimeOffset UpdatedAt { get; set; }    // Automatically updated on modify
}

// For automatic CreatedBy/UpdatedBy user tracking
public class Order : IEntityWithCreatedUpdatedBy
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;  // Automatically set on insert
    public string UpdatedBy { get; set; } = string.Empty;  // Automatically updated on modify
}

// Combined timestamp and user tracking
public class Customer : IEntityWithCreatedUpdatedAt, IEntityWithCreatedUpdatedBy
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Timestamp fields
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    // User tracking fields
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

// Custom auto-update entity
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    [AutoUpdate(AutoUpdateOperation.Insert)]
    public DateTime PublishedAt { get; set; }
    
    [AutoUpdate(AutoUpdateOperation.Update)]
    public DateTime LastModified { get; set; }
    
    [AutoUpdate(AutoUpdateOperation.Insert | AutoUpdateOperation.Update)]
    public string ModifiedBy { get; set; } = string.Empty;
}
```

#### Configuration

```csharp
// Register auto-update services
services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
services.AddEFAutoUpdateInterceptor();

// Configure DbContext with auto-update interceptor
services.AddDbContext<MyDbContext>((provider, options) =>
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(provider.GetRequiredService<AutoUpdateInterceptor>());
});
```

#### Custom User ID Provider Examples

```csharp
// HTTP Context based user provider
public class HttpContextUserIdProvider : IUserIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextUserIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? "Anonymous";
    }
}

// Environment based user provider
public class EnvironmentUserIdProvider : IUserIdProvider
{
    public string GetUserId()
    {
        return Environment.UserName;
    }
}

// Static user provider for testing
public class StaticUserIdProvider : IUserIdProvider
{
    private readonly string _userId;

    public StaticUserIdProvider(string userId)
    {
        _userId = userId;
    }

    public string GetUserId() => _userId;
}
```

#### Auto Update in Action

```csharp
public class CustomerService
{
    private readonly IEFRepository<MyDbContext, Customer> _repository;

    public CustomerService(IEFRepository<MyDbContext, Customer> repository)
    {
        _repository = repository;
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email)
    {
        var customer = new Customer
        {
            Name = name,
            Email = email
            // CreatedAt, CreatedBy will be set automatically
        };

        return await _repository.InsertAsync(customer);
    }

    public async Task<int> UpdateCustomerEmailAsync(int customerId, string newEmail)
    {
        var customer = await _repository.FindAsync(customerId);
        if (customer != null)
        {
            customer.Email = newEmail;
            // UpdatedAt, UpdatedBy will be set automatically
            return await _repository.UpdateAsync(customer);
        }
        return 0;
    }
}
```

#### Manual Auto Update Control

```csharp
public class ManualAutoUpdateService
{
    private readonly MyDbContext _context;

    public ManualAutoUpdateService(MyDbContext context)
    {
        _context = context;
    }

    public async Task UpdateWithCustomTimestampAsync(int customerId, string newName)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer != null)
        {
            customer.Name = newName;
            customer.UpdatedAt = DateTimeOffset.Now.AddHours(-1); // Custom timestamp
            
            // Disable auto-update for this save operation
            _context.ChangeTracker.Entries<Customer>()
                .Where(e => e.Entity.Id == customerId)
                .ForEach(e => e.Property(nameof(Customer.UpdatedAt)).IsModified = false);
            
            await _context.SaveChangesAsync();
        }
    }
}
```

### 6. Database Extensions

The package provides useful database extensions for common operations.

#### Update Extensions

```csharp
public class ProductUpdateService
{
    private readonly MyDbContext _context;

    public ProductUpdateService(MyDbContext context)
    {
        _context = context;
    }

    // Update specific columns using DbContext extension
    public async Task<int> UpdateProductPricesAsync(decimal priceMultiplier)
    {
        return await _context.Update<Product>(
            setters => setters.SetProperty(p => p.Price, p => p.Price * priceMultiplier),
            queryBuilder => queryBuilder.WithPredict(p => p.IsActive)
        );
    }

    // Update without specific columns
    public async Task<int> UpdateProductWithoutTimestampAsync(Product product)
    {
        return await _context.UpdateWithout(product, p => p.UpdatedAt);
    }

    // Bulk update with dictionary
    public async Task<int> BulkUpdateProductStatusAsync(List<int> productIds, bool isActive)
    {
        return await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.IsActive, isActive));
    }
}
```

#### Query Extensions

```csharp
public class ProductQueryExtensions
{
    private readonly MyDbContext _context;

    public ProductQueryExtensions(MyDbContext context)
    {
        _context = context;
    }

    // Get table name
    public string GetProductTableName()
    {
        return _context.GetTableName<Product>();
    }

    // Get column name
    public string GetProductNameColumnName()
    {
        return _context.GetColumnName<Product>(p => p.Name);
    }

    // Check if using relational database
    public bool IsUsingRelationalDatabase()
    {
        return _context.Database.IsRelational();
    }

    // Paged list extension
    public async Task<IPagedListModel<Product>> GetProductsPagedAsync(int page, int size)
    {
        var query = _context.Products.Where(p => p.IsActive);
        return await query.ToPagedListAsync(page, size);
    }
}
```

### 7. Database Functions

The package provides database-specific functions for enhanced querying capabilities.

#### JSON Functions (SQL Server)

```csharp
public class JsonQueryService
{
    private readonly MyDbContext _context;

    public JsonQueryService(MyDbContext context)
    {
        _context = context;
    }

    // Using JSON_VALUE function for SQL Server
    public async Task<List<Product>> GetProductsByJsonPropertyAsync(string propertyValue)
    {
        return await _context.Products
            .Where(p => DbFunctions.JsonValue(p.JsonData, "$.PropertyName") == propertyValue)
            .ToListAsync();
    }

    // Example with complex JSON queries
    public async Task<List<Product>> GetProductsWithJsonFiltersAsync()
    {
        return await _context.Products
            .Where(p => 
                DbFunctions.JsonValue(p.Metadata, "$.category") == "Electronics" &&
                DbFunctions.JsonValue(p.Metadata, "$.inStock") == "true")
            .ToListAsync();
    }
}

// Entity with JSON column
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JsonData { get; set; } = string.Empty;  // JSON column
    public string Metadata { get; set; } = string.Empty;  // Another JSON column
}
```

### 8. Repository Generator

The repository generator automatically creates repository instances for your entities.

#### Configuration

```csharp
// Register repository generator
services.AddEFRepositoryGenerator();

// Register specific repositories
services.AddEFRepository<MyDbContext>();
```

#### Using Repository Generator

```csharp
public class MultiRepositoryService
{
    private readonly IEFRepositoryGenerator<MyDbContext> _repositoryGenerator;

    public MultiRepositoryService(IEFRepositoryGenerator<MyDbContext> repositoryGenerator)
    {
        _repositoryGenerator = repositoryGenerator;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        // Get repositories dynamically
        var orderRepo = _repositoryGenerator.GetRepository<Order>();
        var customerRepo = _repositoryGenerator.GetRepository<Customer>();
        var productRepo = _repositoryGenerator.GetRepository<Product>();

        // Validate customer
        var customer = await customerRepo.FindAsync(order.CustomerId);
        if (customer == null)
        {
            throw new InvalidOperationException("Customer not found");
        }

        // Validate products
        foreach (var item in order.Items)
        {
            var product = await productRepo.FindAsync(item.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product {item.ProductId} not found");
            }
        }

        // Save order
        await orderRepo.InsertAsync(order);
    }
}
```

### 9. Advanced Query Building

The package provides a fluent query builder for complex scenarios.

#### Query Builder Examples

```csharp
public class AdvancedQueryService
{
    private readonly IEFRepository<MyDbContext, Product> _repository;

    public AdvancedQueryService(IEFRepository<MyDbContext, Product> repository)
    {
        _repository = repository;
    }

    // Complex filtering with query builder
    public async Task<List<Product>> GetFilteredProductsAsync(ProductFilter filter)
    {
        return await _repository.GetListAsync(queryBuilder =>
        {
            // Base query
            queryBuilder.WithPredict(p => p.IsActive);

            // Conditional filters
            if (!string.IsNullOrEmpty(filter.Name))
            {
                queryBuilder.WithPredict(p => p.Name.Contains(filter.Name));
            }

            if (filter.MinPrice.HasValue)
            {
                queryBuilder.WithPredict(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                queryBuilder.WithPredict(p => p.Price <= filter.MaxPrice.Value);
            }

            if (filter.CategoryIds?.Any() == true)
            {
                queryBuilder.WithPredict(p => filter.CategoryIds.Contains(p.CategoryId));
            }

            // Ordering
            switch (filter.SortBy?.ToLower())
            {
                case "name":
                    queryBuilder.WithOrderBy(q => filter.SortDescending 
                        ? q.OrderByDescending(p => p.Name) 
                        : q.OrderBy(p => p.Name));
                    break;
                case "price":
                    queryBuilder.WithOrderBy(q => filter.SortDescending 
                        ? q.OrderByDescending(p => p.Price) 
                        : q.OrderBy(p => p.Price));
                    break;
                default:
                    queryBuilder.WithOrderBy(q => q.OrderByDescending(p => p.CreatedAt));
                    break;
            }

            // Include related data
            if (filter.IncludeCategory)
            {
                queryBuilder.WithInclude(p => p.Category);
            }

            if (filter.IncludeReviews)
            {
                queryBuilder.WithInclude(p => p.Reviews);
            }
        });
    }

    // Dynamic query building
    public async Task<List<TResult>> GetDynamicResultsAsync<TResult>(
        Expression<Func<Product, TResult>> selector,
        List<Expression<Func<Product, bool>>> predicates,
        Expression<Func<Product, object>>? orderBy = null,
        bool descending = false)
    {
        return await _repository.GetResultAsync(selector, queryBuilder =>
        {
            // Apply all predicates
            foreach (var predicate in predicates)
            {
                queryBuilder.WithPredict(predicate);
            }

            // Apply ordering
            if (orderBy != null)
            {
                queryBuilder.WithOrderBy(q => descending 
                    ? q.OrderByDescending(orderBy)
                    : q.OrderBy(orderBy));
            }
        });
    }
}

public class ProductFilter
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<int>? CategoryIds { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public bool IncludeCategory { get; set; }
    public bool IncludeReviews { get; set; }
}
```

## Best Practices

### 1. Performance Considerations

```csharp
// Use projection for large datasets
var productSummaries = await _repository.GetResultAsync(
    p => new { p.Id, p.Name, p.Price },
    queryBuilder => queryBuilder.WithPredict(p => p.IsActive)
);

// Use paging for large result sets
var pagedProducts = await _repository.GetPagedListAsync(
    queryBuilder => queryBuilder
        .WithPredict(p => p.IsActive)
        .WithOrderBy(q => q.OrderBy(p => p.Name)),
    pageNumber: 1,
    pageSize: 50
);

// Use bulk operations for multiple updates
await _repository.UpdateAsync(
    setters => setters.SetProperty(p => p.IsActive, false),
    queryBuilder => queryBuilder.WithPredict(p => p.LastSoldDate < DateTime.Now.AddYears(-1))
);
```

### 2. Error Handling

```csharp
public class SafeProductService
{
    private readonly IEFRepository<MyDbContext, Product> _repository;
    private readonly ILogger<SafeProductService> _logger;

    public SafeProductService(
        IEFRepository<MyDbContext, Product> repository,
        ILogger<SafeProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Product?> SafeGetProductAsync(int id)
    {
        try
        {
            return await _repository.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return null;
        }
    }

    public async Task<bool> SafeUpdateProductAsync(Product product)
    {
        try
        {
            var result = await _repository.UpdateAsync(product);
            return result > 0;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating product {ProductId}", product.Id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", product.Id);
            return false;
        }
    }
}
```

### 3. Testing

```csharp
public class ProductServiceTests
{
    private readonly Mock<IEFRepository<TestDbContext, Product>> _mockRepository;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IEFRepository<TestDbContext, Product>>();
        _service = new ProductService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetProductAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = 1;
        var expectedProduct = new Product { Id = productId, Name = "Test Product" };
        _mockRepository.Setup(r => r.FindAsync(productId))
                      .ReturnsAsync(expectedProduct);

        // Act
        var result = await _service.GetProductAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProduct.Id, result.Id);
        Assert.Equal(expectedProduct.Name, result.Name);
    }
}
```

## Troubleshooting

### Common Issues

#### 1. Interceptor Not Working

**Problem**: Auto-update or audit interceptors are not being triggered.

**Solution**: Ensure interceptors are properly registered and added to DbContext:

```csharp
// Correct registration
services.AddEFAutoUpdateInterceptor();
services.AddDbContext<MyDbContext>((provider, options) =>
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(provider.GetRequiredService<AutoUpdateInterceptor>());
});
```

#### 2. Soft Delete Filter Not Applied

**Problem**: Soft deleted entities are still appearing in queries.

**Solution**: Ensure global query filters are configured:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .HasQueryFilter(p => !p.IsDeleted);
}
```

#### 3. Repository Not Found

**Problem**: `IEFRepository<TDbContext, TEntity>` cannot be resolved.

**Solution**: Register repository services:

```csharp
services.AddEFRepository();
```

#### 4. Audit Records Not Saved

**Problem**: Audit entries are created but not persisted.

**Solution**: Ensure audit store is properly configured:

```csharp
services.AddEFAutoAudit(builder =>
{
    builder.WithStore<AuditDatabaseStore>(); // Make sure store is configured
});
```

### Performance Tips

1. **Use projections** for read-only operations to reduce data transfer
2. **Implement paging** for large datasets
3. **Use bulk operations** for multiple entity updates
4. **Configure appropriate indexes** for frequently queried properties
5. **Consider using `AsNoTracking()`** for read-only queries

### Migration Notes

When upgrading between major versions, review the [Release Notes](ReleaseNotes.md) for breaking changes and migration guidance.

## Related Documentation

- 🚀 [Getting Started Guide](GettingStarted.md) - New to WeihanLi.EntityFramework? Start here
- ⚡ [Advanced Features Guide](AdvancedFeatures.md) - Custom interceptors, performance optimization, and integration patterns
- 📋 [Release Notes](ReleaseNotes.md) - Version history and breaking changes
