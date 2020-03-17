using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace WeihanLi.EntityFramework.Test
{
    public class EFTestFixture : IDisposable
    {
        private readonly IServiceScope _serviceScope;
        public IServiceProvider Services { get; }

        public EFTestFixture()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<TestDbContext>(options =>
            {
                options.UseLoggerFactory(LoggerFactory.Create(loggingBuilder =>
                {
                }));
                options.EnableDetailedErrors();
                options.UseInMemoryDatabase("Tests");
            });
            serviceCollection.AddEFRepository();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            _serviceScope = serviceProvider.CreateScope();

            Services = _serviceScope.ServiceProvider;
        }

        public void Dispose()
        {
            var dbContext = Services.GetRequiredService<TestDbContext>();
            if (dbContext.Database.IsInMemory())
            {
                dbContext.Database.EnsureDeleted();
            }
            _serviceScope.Dispose();
        }
    }

    [CollectionDefinition("EFTest")]
    public class EFTestCollection : ICollectionFixture<EFTestFixture>
    {
    }
}
