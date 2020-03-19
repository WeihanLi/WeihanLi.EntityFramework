using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Data;
using Xunit;
using Xunit.Abstractions;

namespace WeihanLi.EntityFramework.Test
{
    public class EFUnitOfWorkTest : EFTestBase
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ITestOutputHelper _output;

        public EFUnitOfWorkTest(EFTestFixture fixture, ITestOutputHelper outputHelper) : base(fixture)
        {
            _output = outputHelper;
        }

        [Fact]
        public void TransactionTest()
        {
            try
            {
                _semaphore.Wait();

                _output.WriteLine($"----- TransactionTest Begin {DateTime.UtcNow.Ticks} -----");

                Repository.Insert(new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xss",
                });
                Repository.Update(new TestEntity()
                {
                    Id = 1,
                    Name = new string('x', 6)
                }, "Name");
                using (var scope = Services.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IEFRepository<TestDbContext, TestEntity>>();
                    var ttt = repo.DbContext.Find<TestEntity>(1);
                    repo.Insert(new TestEntity()
                    {
                        CreatedAt = DateTime.UtcNow,
                        Name = new string('y', 6)
                    });
                }

                var beforeCount = Repository.Count();
                var uow = Repository.GetUnitOfWork();
                uow.DbContext.Update(new TestEntity()
                {
                    Id = 1,
                    Name = new string('1', 6)
                }, "Name");
                uow.DbContext.UpdateWithout(new TestEntity()
                {
                    Id = 2,
                    Name = new string('2', 6)
                }, x => x.CreatedAt);
                var entity = new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xyy1",
                };
                uow.DbSet<TestEntity>().Add(entity);
                uow.DbSet<TestEntity>().Remove(entity);
                uow.DbSet<TestEntity>().Add(new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xyy1",
                });

                var beforeCommitCount = Repository.Count();
                Assert.Equal(beforeCount, beforeCommitCount);

                uow.Commit();

                var committedCount = Repository.Count();
                Assert.Equal(committedCount, beforeCount + 1);

                using (var scope = Services.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IEFRepositoryFactory<TestDbContext>>()
                        .GetRepository<TestEntity>();
                    entity = repo.Find(1);
                    Assert.Equal(new string('1', 6), entity.Name);

                    entity = repo.Find(2);
                    Assert.Equal(new string('2', 6), entity.Name);

                    Assert.Equal(1, repo.Delete(1));
                }
            }
            finally
            {
                _output.WriteLine($"----- TransactionTest End {DateTime.UtcNow.Ticks} -----");
                _semaphore.Release();
            }
        }

        [Fact]
        public async Task TransactionAsyncTest()
        {
            try
            {
                await _semaphore.WaitAsync();

                _output.WriteLine($"----- TransactionAsyncTest Begin {DateTime.UtcNow.Ticks}-----");

                await Repository.InsertAsync(new[]
                {
                    new TestEntity()
                    {
                        CreatedAt = DateTime.UtcNow,
                        Name = "xss1",
                    },
                    new TestEntity()
                    {
                        CreatedAt = DateTime.UtcNow,
                        Name = "xss2",
                    },
                    new TestEntity()
                    {
                        CreatedAt = DateTime.UtcNow,
                        Name = "xss3",
                    }
                });

                using (var scope = Services.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IEFRepository<TestDbContext, TestEntity>>();
                    await repo.InsertAsync(new TestEntity()
                    {
                        CreatedAt = DateTime.UtcNow,
                        Name = "xxxxxx"
                    });
                }

                var beforeCount = await Repository.CountAsync();
                var uow = Repository.GetUnitOfWork();
                uow.DbContext.Update(new TestEntity()
                {
                    Id = 3,
                    Name = new string('3', 6)
                }, "Name");
                uow.DbContext.UpdateWithout(new TestEntity()
                {
                    Id = 4,
                    Name = new string('4', 6)
                }, x => x.CreatedAt);
                var entity = new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xyy1",
                };
                uow.DbSet<TestEntity>().Add(entity);
                uow.DbSet<TestEntity>().Remove(entity);
                uow.DbSet<TestEntity>().Add(new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xyy1",
                });

                var beforeCommitCount = await Repository.CountAsync();
                Assert.Equal(beforeCount, beforeCommitCount);

                await uow.CommitAsync();

                var committedCount = await Repository.CountAsync();
                Assert.Equal(committedCount, beforeCount + 1);

                entity = await Repository.DbContext.FindAsync<TestEntity>(3);
                Assert.Equal(new string('3', 6), entity.Name);

                entity = await Repository.DbContext.FindAsync<TestEntity>(new object[] { 4 }, CancellationToken.None);
                Assert.Equal(new string('4', 6), entity.Name);

                Assert.Equal(1, await Repository.DeleteAsync(1));
            }
            finally
            {
                _output.WriteLine($"----- TransactionAsyncTest End {DateTime.UtcNow.Ticks} -----");
                _semaphore.Release();
            }
        }
    }
}
