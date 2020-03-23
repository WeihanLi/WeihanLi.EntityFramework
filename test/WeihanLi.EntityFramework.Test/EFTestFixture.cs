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

        private const string DbConnectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public EFTestFixture()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<TestDbContext>(options =>
            {
                options.UseLoggerFactory(LoggerFactory.Create(loggingBuilder =>
                {
                    loggingBuilder.AddLog4Net();
                    //loggingBuilder.AddProvider(new DelegateLoggerProvider((category, logLevel, exception, msg) =>
                    //{
                    //    _outputHelper.WriteLine($"{category}:[{logLevel}] {msg}\n {exception}");
                    //}));
                }));
                options.UseSqlServer(DbConnectionString);
                //options.UseInMemoryDatabase("Tests");
                options.EnableDetailedErrors();
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

    [CollectionDefinition("EFTest", DisableParallelization = true)]
    public class EFTestCollection : ICollectionFixture<EFTestFixture>
    {
    }
}
