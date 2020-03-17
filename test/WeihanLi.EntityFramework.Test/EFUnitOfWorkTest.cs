using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Data;
using Xunit;

namespace WeihanLi.EntityFramework.Test
{
    public class EFUnitOfWorkTest : EFTestBase
    {
        public EFUnitOfWorkTest(EFTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void TransactionTest()
        {
            Repository.Insert(new TestEntity()
            {
                CreatedAt = DateTime.UtcNow,
                Name = "xss",
            });
            using (var scope = Services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IEFRepository<TestDbContext, TestEntity>>();
                repo.Insert(new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xxxxxx"
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

            entity = Repository.DbContext.Find<TestEntity>(1);
            Assert.Equal(new string('1', 6), entity.Name);

            entity = Repository.DbContext.Find<TestEntity>(2);
            Assert.Equal(new string('2', 6), entity.Name);
        }

        [Fact]
        public async Task TransactionAsyncTest()
        {
            await Repository.InsertAsync(new[]
            {
                new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xss",
                },
                new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xss",
                },
                new TestEntity()
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = "xss",
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
        }
    }
}
