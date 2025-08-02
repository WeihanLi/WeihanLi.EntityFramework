# Advanced Features Guide

This guide covers advanced features and patterns for WeihanLi.EntityFramework.

## Table of Contents

- [Custom Interceptors](#custom-interceptors)
- [Advanced Audit Configuration](#advanced-audit-configuration)
- [Performance Optimization](#performance-optimization)
- [Testing Strategies](#testing-strategies)
- [Custom Query Filters](#custom-query-filters)
- [Bulk Operations](#bulk-operations)
- [Integration Patterns](#integration-patterns)

## Custom Interceptors

### Creating Custom Auto-Update Logic

```csharp
public class CustomAutoUpdateInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<CustomAutoUpdateInterceptor> _logger;
    private readonly IUserIdProvider _userIdProvider;

    public CustomAutoUpdateInterceptor(
        ILogger<CustomAutoUpdateInterceptor> logger,
        IUserIdProvider userIdProvider)
    {
        _logger = logger;
        _userIdProvider = userIdProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            HandleCustomUpdates(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void HandleCustomUpdates(DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Custom business logic
            if (entry.Entity is IVersionedEntity versionedEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    versionedEntity.Version = 1;
                }
                else if (entry.State == EntityState.Modified)
                {
                    versionedEntity.Version++;
                }
            }

            // Log entity changes
            if (entry.Entity is IAuditableEntity auditableEntity)
            {
                _logger.LogInformation("Entity {EntityType} with ID {EntityId} is being {Action}",
                    entry.Entity.GetType().Name,
                    GetEntityId(entry),
                    entry.State);
            }
        }
    }

    private object? GetEntityId(EntityEntry entry)
    {
        var keyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return keyProperty?.CurrentValue;
    }
}

// Supporting interfaces
public interface IVersionedEntity
{
    int Version { get; set; }
}

public interface IAuditableEntity
{
    // Marker interface for entities that should be logged
}
```

### Conditional Interceptors

```csharp
public class ConditionalAuditInterceptor : SaveChangesInterceptor
{
    private readonly IAuditConfig _auditConfig;
    private readonly IServiceProvider _serviceProvider;

    public ConditionalAuditInterceptor(
        IAuditConfig auditConfig,
        IServiceProvider serviceProvider)
    {
        _auditConfig = auditConfig;
        _serviceProvider = serviceProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // Only audit in production environment
        if (_auditConfig.IsEnabled && IsProductionEnvironment())
        {
            var auditInterceptor = _serviceProvider.GetRequiredService<AuditInterceptor>();
            return auditInterceptor.SavingChangesAsync(eventData, result, cancellationToken);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private bool IsProductionEnvironment()
    {
        var environment = _serviceProvider.GetService<IWebHostEnvironment>();
        return environment?.IsProduction() == true;
    }
}
```

## Advanced Audit Configuration

### Custom Audit Property Enricher

```csharp
public class CustomAuditPropertyEnricher : IAuditPropertyEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public CustomAuditPropertyEnricher(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public void Enrich(AuditEntry auditEntry)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext != null)
        {
            // Add request information
            auditEntry.ExtraProperties["UserAgent"] = httpContext.Request.Headers["User-Agent"].ToString();
            auditEntry.ExtraProperties["IpAddress"] = httpContext.Connection.RemoteIpAddress?.ToString();
            auditEntry.ExtraProperties["RequestId"] = httpContext.TraceIdentifier;
            
            // Add correlation ID if available
            if (httpContext.Request.Headers.ContainsKey("X-Correlation-ID"))
            {
                auditEntry.ExtraProperties["CorrelationId"] = 
                    httpContext.Request.Headers["X-Correlation-ID"].ToString();
            }
        }

        // Add application-specific information
        auditEntry.ExtraProperties["ApplicationVersion"] = _configuration["ApplicationVersion"];
        auditEntry.ExtraProperties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        auditEntry.ExtraProperties["ServerName"] = Environment.MachineName;
    }
}
```

### Multi-Store Audit Configuration

```csharp
public class MultiStoreAuditStore : IAuditStore
{
    private readonly List<IAuditStore> _stores;
    private readonly ILogger<MultiStoreAuditStore> _logger;

    public MultiStoreAuditStore(
        IEnumerable<IAuditStore> stores,
        ILogger<MultiStoreAuditStore> logger)
    {
        _stores = stores.ToList();
        _logger = logger;
    }

    public async Task Save(ICollection<AuditEntry> auditEntries)
    {
        var tasks = _stores.Select(async store =>
        {
            try
            {
                await store.Save(auditEntries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save audit entries to store {StoreType}", 
                    store.GetType().Name);
            }
        });

        await Task.WhenAll(tasks);
    }
}

// Configuration
services.AddEFAutoAudit(builder =>
{
    builder
        .WithUserIdProvider<HttpContextUserIdProvider>()
        .WithPropertyEnricher<CustomAuditPropertyEnricher>()
        .WithStore<AuditDatabaseStore>()      // Primary store
        .WithStore<AuditElasticsearchStore>() // Secondary store for analytics
        .WithStore<AuditFileStore>();         // Backup store
});

// Register multi-store
services.AddSingleton<IAuditStore, MultiStoreAuditStore>();
```

## Performance Optimization

### Optimized Repository Patterns

```csharp
public class OptimizedProductService
{
    private readonly IEFRepository<AppDbContext, Product> _repository;
    private readonly IMemoryCache _cache;

    public OptimizedProductService(
        IEFRepository<AppDbContext, Product> repository,
        IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    // Use projection to reduce data transfer
    public async Task<List<ProductSummary>> GetProductSummariesAsync()
    {
        return await _repository.GetResultAsync(
            p => new ProductSummary
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category.Name
            },
            queryBuilder => queryBuilder
                .WithPredict(p => p.IsActive)
                .WithInclude(p => p.Category)
        );
    }

    // Cached frequently accessed data
    public async Task<Product?> GetProductWithCacheAsync(int id)
    {
        var cacheKey = $"product_{id}";
        
        if (_cache.TryGetValue(cacheKey, out Product? cachedProduct))
        {
            return cachedProduct;
        }

        var product = await _repository.FindAsync(id);
        
        if (product != null)
        {
            _cache.Set(cacheKey, product, TimeSpan.FromMinutes(15));
        }

        return product;
    }

    // Bulk operations for better performance
    public async Task<int> BulkActivateProductsAsync(List<int> productIds)
    {
        return await _repository.UpdateAsync(
            setters => setters.SetProperty(p => p.IsActive, true),
            queryBuilder => queryBuilder.WithPredict(p => productIds.Contains(p.Id))
        );
    }

    // Streaming for large datasets
    public async IAsyncEnumerable<Product> StreamActiveProductsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _repository.Query(
            queryBuilder => queryBuilder
                .WithPredict(p => p.IsActive)
                .WithOrderBy(q => q.OrderBy(p => p.Id))
        );

        await foreach (var product in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return product;
        }
    }
}

public class ProductSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
```

### Connection and Query Optimization

```csharp
// Configure DbContext for optimal performance
services.AddDbContext<AppDbContext>((provider, options) =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure(3);
    })
    .EnableSensitiveDataLogging(isDevelopment)
    .EnableDetailedErrors(isDevelopment)
    .ConfigureWarnings(warnings =>
    {
        warnings.Ignore(CoreEventId.FirstWithoutOrderByAndFilterWarning);
    });
});

// Connection pooling
services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
}, poolSize: 128);
```

## Testing Strategies

### Unit Testing with Mocked Repositories

```csharp
public class ProductServiceTests
{
    private readonly Mock<IEFRepository<AppDbContext, Product>> _mockRepository;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IEFRepository<AppDbContext, Product>>();
        _mockCache = new Mock<IMemoryCache>();
        _service = new ProductService(_mockRepository.Object, _mockCache.Object);
    }

    [Fact]
    public async Task GetActiveProductsAsync_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        var activeProducts = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", IsActive = true },
            new() { Id = 2, Name = "Product 2", IsActive = true }
        };

        _mockRepository
            .Setup(r => r.GetListAsync(It.IsAny<Action<EFRepositoryQueryBuilder<Product>>>()))
            .ReturnsAsync(activeProducts);

        // Act
        var result = await _service.GetActiveProductsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.True(p.IsActive));
        
        _mockRepository.Verify(
            r => r.GetListAsync(It.IsAny<Action<EFRepositoryQueryBuilder<Product>>>()), 
            Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldSetCorrectProperties()
    {
        // Arrange
        var productName = "New Product";
        var productPrice = 99.99m;
        var expectedProduct = new Product 
        { 
            Id = 1, 
            Name = productName, 
            Price = productPrice,
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.InsertAsync(It.IsAny<Product>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _service.CreateProductAsync(productName, productPrice);

        // Assert
        Assert.Equal(expectedProduct.Id, result.Id);
        Assert.Equal(productName, result.Name);
        Assert.Equal(productPrice, result.Price);
        Assert.True(result.IsActive);

        _mockRepository.Verify(
            r => r.InsertAsync(It.Is<Product>(p => 
                p.Name == productName && 
                p.Price == productPrice && 
                p.IsActive)), 
            Times.Once);
    }
}
```

### Integration Testing with In-Memory Database

```csharp
public class ProductServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAndRetrieveProduct_ShouldWorkEndToEnd()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

        await context.Database.EnsureCreatedAsync();

        // Act
        var createdProduct = await productService.CreateProductAsync("Test Product", 50.00m);
        var retrievedProduct = await productService.GetProductByIdAsync(createdProduct.Id);

        // Assert
        Assert.NotNull(retrievedProduct);
        Assert.Equal("Test Product", retrievedProduct.Name);
        Assert.Equal(50.00m, retrievedProduct.Price);
        Assert.True(retrievedProduct.IsActive);
        Assert.True(retrievedProduct.CreatedAt > DateTimeOffset.MinValue);
    }
}

// Test-specific configuration
public class TestStartup : Startup
{
    public TestStartup(IConfiguration configuration) : base(configuration) { }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        // Replace real database with in-memory
        services.Remove(services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)));
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseInMemoryDatabase("TestDatabase");
        });

        // Use test user provider
        services.AddSingleton<IUserIdProvider>(new StaticUserIdProvider("TestUser"));
    }
}
```

## Custom Query Filters

### Dynamic Query Filters

```csharp
public class TenantAwareRepository<TEntity> : IEFRepository<AppDbContext, TEntity>
    where TEntity : class, ITenantEntity
{
    private readonly IEFRepository<AppDbContext, TEntity> _baseRepository;
    private readonly ITenantProvider _tenantProvider;

    public TenantAwareRepository(
        IEFRepository<AppDbContext, TEntity> baseRepository,
        ITenantProvider tenantProvider)
    {
        _baseRepository = baseRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<TEntity>> GetListAsync(
        Action<EFRepositoryQueryBuilder<TEntity>>? queryBuilderAction = null)
    {
        return await _baseRepository.GetListAsync(queryBuilder =>
        {
            // Always apply tenant filter
            queryBuilder.WithPredict(e => e.TenantId == _tenantProvider.GetCurrentTenantId());
            
            // Apply additional filters
            queryBuilderAction?.Invoke(queryBuilder);
        });
    }

    // Implement other methods with tenant filtering...
}

public interface ITenantEntity
{
    string TenantId { get; set; }
}

public interface ITenantProvider
{
    string GetCurrentTenantId();
}
```

### Security-Based Filters

```csharp
public class SecureProductRepository : IProductRepository
{
    private readonly IEFRepository<AppDbContext, Product> _repository;
    private readonly ICurrentUserService _currentUserService;

    public SecureProductRepository(
        IEFRepository<AppDbContext, Product> repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<List<Product>> GetAccessibleProductsAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        
        return await _repository.GetListAsync(queryBuilder =>
        {
            if (currentUser.Role == UserRole.Admin)
            {
                // Admins see all products
                queryBuilder.WithPredict(p => true);
            }
            else if (currentUser.Role == UserRole.Manager)
            {
                // Managers see products in their department
                queryBuilder.WithPredict(p => p.DepartmentId == currentUser.DepartmentId);
            }
            else
            {
                // Regular users see only active products they created
                queryBuilder.WithPredict(p => 
                    p.IsActive && p.CreatedBy == currentUser.Id);
            }
        });
    }
}
```

## Bulk Operations

### Advanced Bulk Processing

```csharp
public class BulkOperationService
{
    private readonly IEFRepository<AppDbContext, Product> _productRepository;
    private readonly ILogger<BulkOperationService> _logger;

    public BulkOperationService(
        IEFRepository<AppDbContext, Product> productRepository,
        ILogger<BulkOperationService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<BulkOperationResult> BulkUpdatePricesAsync(
        Dictionary<int, decimal> priceUpdates,
        CancellationToken cancellationToken = default)
    {
        var result = new BulkOperationResult();
        const int batchSize = 1000;

        var batches = priceUpdates
            .Select((item, index) => new { item, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.item).ToList());

        foreach (var batch in batches)
        {
            try
            {
                var productIds = batch.Select(b => b.Key).ToList();
                
                foreach (var update in batch)
                {
                    var updated = await _productRepository.UpdateAsync(
                        setters => setters.SetProperty(p => p.Price, update.Value),
                        queryBuilder => queryBuilder.WithPredict(p => p.Id == update.Key)
                    );

                    if (updated > 0)
                    {
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.FailedIds.Add(update.Key);
                        result.FailureCount++;
                    }
                }

                _logger.LogInformation("Processed batch of {Count} price updates", batch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process batch of price updates");
                result.FailedIds.AddRange(batch.Select(b => b.Key));
                result.FailureCount += batch.Count;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        return result;
    }

    public async Task<int> BulkArchiveOldProductsAsync(DateTime cutoffDate)
    {
        return await _productRepository.UpdateAsync(
            setters => setters
                .SetProperty(p => p.IsActive, false)
                .SetProperty(p => p.ArchivedAt, DateTime.UtcNow),
            queryBuilder => queryBuilder.WithPredict(p => 
                p.IsActive && p.CreatedAt < cutoffDate)
        );
    }
}

public class BulkOperationResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<int> FailedIds { get; set; } = new();
    
    public bool HasFailures => FailureCount > 0;
    public double SuccessRate => 
        SuccessCount + FailureCount > 0 
            ? (double)SuccessCount / (SuccessCount + FailureCount) 
            : 0;
}
```

## Integration Patterns

### Event-Driven Architecture

```csharp
public class EventDrivenProductService
{
    private readonly IEFRepository<AppDbContext, Product> _repository;
    private readonly IEventPublisher _eventPublisher;

    public EventDrivenProductService(
        IEFRepository<AppDbContext, Product> repository,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Product> CreateProductAsync(CreateProductCommand command)
    {
        var product = new Product
        {
            Name = command.Name,
            Price = command.Price,
            CategoryId = command.CategoryId
        };

        var createdProduct = await _repository.InsertAsync(product);

        // Publish domain event
        await _eventPublisher.PublishAsync(new ProductCreatedEvent
        {
            ProductId = createdProduct.Id,
            Name = createdProduct.Name,
            Price = createdProduct.Price,
            CreatedAt = createdProduct.CreatedAt,
            CreatedBy = createdProduct.CreatedBy
        });

        return createdProduct;
    }
}

public class ProductCreatedEvent
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

// Event handler for side effects
public class ProductCreatedEventHandler : IEventHandler<ProductCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;

    public ProductCreatedEventHandler(
        IEmailService emailService,
        ICacheService cacheService)
    {
        _emailService = emailService;
        _cacheService = cacheService;
    }

    public async Task HandleAsync(ProductCreatedEvent @event)
    {
        // Send notification
        await _emailService.SendProductCreatedNotificationAsync(@event);

        // Invalidate cache
        await _cacheService.InvalidateAsync("products_*");

        // Update search index, analytics, etc.
    }
}
```

### CQRS Pattern Integration

```csharp
// Command side - uses repositories for writes
public class UpdateProductCommandHandler
{
    private readonly IEFRepository<AppDbContext, Product> _repository;
    private readonly IEventStore _eventStore;

    public UpdateProductCommandHandler(
        IEFRepository<AppDbContext, Product> repository,
        IEventStore eventStore)
    {
        _repository = repository;
        _eventStore = eventStore;
    }

    public async Task<UpdateProductResult> HandleAsync(UpdateProductCommand command)
    {
        var product = await _repository.FindAsync(command.ProductId);
        if (product == null)
        {
            return UpdateProductResult.NotFound();
        }

        // Business logic
        var oldPrice = product.Price;
        product.Name = command.Name;
        product.Price = command.Price;

        await _repository.UpdateAsync(product);

        // Store event
        if (oldPrice != command.Price)
        {
            await _eventStore.AppendAsync(new ProductPriceChangedEvent
            {
                ProductId = product.Id,
                OldPrice = oldPrice,
                NewPrice = command.Price,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = command.UserId
            });
        }

        return UpdateProductResult.Success();
    }
}

// Query side - uses raw EF for reads
public class ProductQueryService
{
    private readonly AppDbContext _context;

    public ProductQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDetailsView?> GetProductDetailsAsync(int productId)
    {
        return await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => new ProductDetailsView
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category.Name,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ReviewCount = p.Reviews.Count(),
                AverageRating = p.Reviews.Average(r => r.Rating)
            })
            .FirstOrDefaultAsync();
    }
}
```

These advanced patterns help you build robust, scalable applications while leveraging the full power of WeihanLi.EntityFramework.

## Related Documentation

- 🚀 [Getting Started Guide](GettingStarted.md) - New to WeihanLi.EntityFramework? Start here
- 📖 [Complete Usage Guide](Usage.md) - Comprehensive documentation with examples for all features  
- 📋 [Release Notes](ReleaseNotes.md) - Version history and breaking changes