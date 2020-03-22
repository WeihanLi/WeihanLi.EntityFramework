using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            IServiceScope scope1 = null;
            try
            {
                _semaphore.Wait();
                scope1 = Services.CreateScope();
                var repository = scope1.ServiceProvider.GetRequiredService<IEFRepository<TestDbContext, TestEntity>>();

                _output.WriteLine($"----- TransactionTest Begin {DateTime.UtcNow.Ticks} -----");

                repository.DbContext.Database.EnsureCreated();

                repository.Insert(new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xss",
                });
                repository.Update(new TestEntity()
                {
                    Id = 1,
                    Name = new string('x', 6)
                }, "Name");
                using (var scope = Services.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IEFRepository<TestDbContext, TestEntity>>();
                    repo.Insert(new TestEntity()
                    {
                        CreatedAt = DateTime.UtcNow,
                        Name = new string('y', 6)
                    });
                }

                var beforeCount = repository.Count();

                using var uow = repository.GetUnitOfWork();
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

                var beforeCommitCount = repository.Count();
                Assert.Equal(beforeCount, beforeCommitCount);

                uow.Commit();

                var committedCount = repository.Count();
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
                if (Repository.DbContext.Database.IsInMemory())
                {
                    Repository.DbContext.Database.EnsureDeleted();
                }
                else
                {
                    Repository.DbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE TestEntities");
                }

                _semaphore.Release();
                _output.WriteLine($"----- TransactionTest End {DateTime.UtcNow.Ticks} -----");
            }
        }

        [Fact]
        public async Task TransactionAsyncTest()
        {
            IServiceScope scope1 = null;
            try
            {
                await _semaphore.WaitAsync();
                scope1 = Services.CreateScope();
                var repository = scope1.ServiceProvider.GetRequiredService<IEFRepositoryFactory<TestDbContext>>()
                    .GetRepository<TestEntity>();

                _output.WriteLine($"----- TransactionAsyncTest Begin {DateTime.UtcNow.Ticks}-----");

                repository.DbContext.Database.EnsureCreated();

                //for (var i = 0; i < 3; i++)
                //{
                //    await Repository.InsertAsync(new TestEntity()
                //    {
                //        CreatedAt = DateTime.UtcNow,
                //        Name = $"xss-{i}",
                //    });
                //}

                await repository.InsertAsync(new[]
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

                var beforeCount = await repository.CountAsync();
                using var uow = repository.GetUnitOfWork();
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

                var beforeCommitCount = await repository.CountAsync();
                Assert.Equal(beforeCount, beforeCommitCount);

                await uow.CommitAsync();

                var committedCount = await repository.CountAsync();
                Assert.Equal(committedCount, beforeCount + 1);

                entity = await repository.DbContext.FindAsync<TestEntity>(3);
                Assert.Equal(new string('3', 6), entity.Name);

                entity = await repository.DbContext.FindAsync<TestEntity>(new object[] { 4 }, CancellationToken.None);
                Assert.Equal(new string('4', 6), entity.Name);

                Assert.Equal(1, await Repository.DeleteAsync(1));
            }
            finally
            {
                if (Repository.DbContext.Database.IsInMemory())
                {
                    Repository.DbContext.Database.EnsureDeleted();
                }
                else
                {
                    Repository.DbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE TestEntities");
                }
                _output.WriteLine($"----- TransactionAsyncTest End {DateTime.UtcNow.Ticks} -----");

                _semaphore.Release();
                scope1?.Dispose();
            }
        }

        [Fact]
        public void RollbackTest()
        {
            try
            {
                _semaphore.Wait();

                using (var scope = Services.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider
                        .GetRequiredService<IEFUnitOfWork<TestDbContext>>();
                    unitOfWork.DbContext.TestEntities
                        .Add(new TestEntity() { CreatedAt = DateTime.UtcNow, Name = "saa" });
                    unitOfWork.DbContext.TestEntities
                        .Add(new TestEntity() { CreatedAt = DateTime.UtcNow, Name = "saa" });
                    unitOfWork.Commit();
                }
                using (var scope = Services.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider
                        .GetRequiredService<IEFUnitOfWork<TestDbContext>>();
                    var count = unitOfWork.DbContext.TestEntities.Count();

                    unitOfWork.DbContext.TestEntities.Add(new TestEntity() { Name = "xxx", CreatedAt = DateTime.UtcNow });
                    unitOfWork.DbContext.TestEntities.Add(new TestEntity() { Name = "xxx", CreatedAt = DateTime.UtcNow });

                    unitOfWork.Rollback();

                    var count2 = unitOfWork.DbContext.TestEntities.Count();
                    Assert.Equal(count, count2);

                    if (unitOfWork.DbContext.IsRelationalDatabase())
                    {
                        // can not recommit when rollback already
                        Assert.Throws<InvalidOperationException>(() => unitOfWork.Commit());
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [Fact]
        public async Task RollbackAsyncTest()
        {
            try
            {
                await _semaphore.WaitAsync();

                using (var scope = Services.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider
                        .GetRequiredService<IEFUnitOfWork<TestDbContext>>();
                    unitOfWork.DbContext.TestEntities
                        .Add(new TestEntity() { CreatedAt = DateTime.UtcNow, Name = "saa" });
                    unitOfWork.DbContext.TestEntities
                        .Add(new TestEntity() { CreatedAt = DateTime.UtcNow, Name = "saa" });
                    await unitOfWork.CommitAsync();
                }
                using (var scope = Services.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider
                        .GetRequiredService<IEFUnitOfWork<TestDbContext>>();

                    var count = unitOfWork.DbContext.TestEntities.Count();

                    unitOfWork.DbContext.TestEntities.Add(new TestEntity() { Name = "xxx", CreatedAt = DateTime.UtcNow });
                    unitOfWork.DbContext.TestEntities.Add(new TestEntity() { Name = "xxx", CreatedAt = DateTime.UtcNow });

                    await unitOfWork.RollbackAsync();

                    var count2 = unitOfWork.DbContext.TestEntities.Count();
                    Assert.Equal(count, count2);

                    if (unitOfWork.DbContext.IsRelationalDatabase())
                    {
                        // can not recommit when rollback already
                        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.CommitAsync());
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [Fact]
        public void HybridTest()
        {
            try
            {
                _semaphore.Wait();

                using (var scope = Services.CreateScope())
                {
                    Repository.Insert(new TestEntity()
                    {
                        Name = "_00"
                    });

                    var repository = scope.ServiceProvider.GetRequiredService<IEFRepository<TestDbContext, TestEntity>>();
                    repository.Insert(new TestEntity() { Name = "x111", CreatedAt = DateTime.UtcNow, });

                    var count0 = repository.Count();

                    using var uow = scope.ServiceProvider.GetRequiredService<IEFUnitOfWork<TestDbContext>>();
                    uow.DbContext.Add(new TestEntity() { CreatedAt = DateTime.UtcNow, Name = "xx" });
                    uow.DbContext.Add(new TestEntity() { CreatedAt = DateTime.UtcNow, Name = "xx" });

                    var result = repository.Insert(new TestEntity() { CreatedAt = DateTime.UtcNow, Name = "yyyy" });

                    result = repository.Insert(new TestEntity() { CreatedAt = DateTime.UtcNow, Name = "yyyy" });

                    var count1 = repository.Count();

                    Repository.Insert(new TestEntity() { Name = "_111", CreatedAt = DateTime.UtcNow, });

                    var count2 = repository.Count();

                    uow.Rollback();

                    var count3 = repository.Count();

                    result = repository.Insert(new TestEntity() { CreatedAt = DateTime.UtcNow, Name = "yyyy" });

                    var count4 = repository.Count();

                    //uow.Commit();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
