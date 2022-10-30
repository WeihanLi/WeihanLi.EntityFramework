using System;
using System.Threading.Tasks;
using WeihanLi.Common;
using Xunit;
using Xunit.Abstractions;

namespace WeihanLi.EntityFramework.Test;

public class EFRepositoryTest : EFTestBase
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly AsyncLock _lock = new AsyncLock();

    public EFRepositoryTest(EFTestFixture fixture, ITestOutputHelper outputHelper) : base(fixture)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void InsertTest()
    {
        using (_lock.Lock())
        {
            DependencyResolver.TryInvoke<IEFRepository<TestDbContext, TestEntity>>(repo =>
            {
                var entity = new TestEntity() { Name = "abc1", CreatedAt = DateTime.UtcNow, Extra = "" };
                repo.Insert(entity);

                var entities = new[]
                {
                        new TestEntity() {Name = "abc2", CreatedAt = DateTime.UtcNow, Extra = ""},
                        new TestEntity() {Name = "abc3", CreatedAt = DateTime.UtcNow, Extra = ""}
                };
                repo.Insert(entities);
            });
        }
    }

    [Fact]
    public async Task InsertAsyncTest()
    {
        using (await _lock.LockAsync())
        {
            await DependencyResolver.TryInvokeAsync<IEFRepository<TestDbContext, TestEntity>>(async repo =>
            {
                var entity = new TestEntity() { Name = "abc1", CreatedAt = DateTime.UtcNow, Extra = "" };
                await repo.InsertAsync(entity);

                var entities = new[]
                {
                        new TestEntity() {Name = "abc2", CreatedAt = DateTime.UtcNow, Extra = ""},
                        new TestEntity() {Name = "abc3", CreatedAt = DateTime.UtcNow, Extra = ""}
                };
                await repo.InsertAsync(entities);
            });
        }
    }
}
