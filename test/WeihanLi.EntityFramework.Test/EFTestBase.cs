using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace WeihanLi.EntityFramework.Test;

public class EFTestBase : IClassFixture<EFTestFixture>
{
    public IServiceProvider Services { get; }
    public IEFRepository<TestDbContext, TestEntity> Repository { get; }

    public EFTestBase(EFTestFixture fixture)
    {
        Services = fixture.Services;
        Repository = fixture.Services
            .GetRequiredService<IEFRepository<TestDbContext, TestEntity>>();
        Repository.DbContext.CleanData();
    }
}
