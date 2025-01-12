using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Sample;

public static class DbContextInterceptorSamples
{
    public static async Task RunAsync()
    {
        await InterceptorTest2();
    }

    private static async Task InterceptorTest1()
    {
        var services = new ServiceCollection();
        services.AddScoped<SavingInterceptor>();
        services.AddDbContext<FileTestDbContext>((provider, options) =>
        {
            options.AddInterceptors(provider.GetRequiredService<SavingInterceptor>());
            options.UseInMemoryDatabase("test");
        });
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileTestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        dbContext.Entities.Add(new TestEntity { Id = 1, Name = "1" });
        await dbContext.SaveChangesAsync();
    }

    private static async Task InterceptorTest2()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FileTestDbContext>(options =>
        {
            options.UseInMemoryDatabase("test");
        });
        services.AddDbContextInterceptor<FileTestDbContext, SavingInterceptor>();
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileTestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        dbContext.Entities.Add(new TestEntity { Id = 2, Name = "1" });
        await dbContext.SaveChangesAsync();
    }
}

file sealed class FileTestDbContext(DbContextOptions<FileTestDbContext> options) : DbContext(options)
{
    public DbSet<TestEntity> Entities { get; set; }
}

file sealed class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}

file sealed class SavingInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("SavingChangesAsync");
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("SavedChangesAsync");
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
