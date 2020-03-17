using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WeihanLi.EntityFramework.Test
{
    public class EFTestBase : IClassFixture<EFTestFixture>
    {
        public IServiceProvider Services { get; }
        public IEFRepository<TestDbContext, TestEntity> Repository { get; }

        public EFTestBase(EFTestFixture fixture)
        {
            Services = fixture.Services;
            Repository = fixture.Services
                .GetRequiredService<IEFRepository<TestDbContext, TestEntity>>();

            //
            if (Repository.DbContext.Database.IsInMemory())
            {
                Repository.DbContext.Database.EnsureDeleted();
            }
            Repository.DbContext.Database.EnsureCreated();
        }
    }
}
