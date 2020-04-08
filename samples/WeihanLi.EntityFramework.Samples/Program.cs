using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeihanLi.Common;
using WeihanLi.Common.Data;
using WeihanLi.Common.Helpers;
using WeihanLi.EntityFramework.Audit;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Samples
{
    public class Program
    {
        private const string DbConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TestDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;"
            // "server=.;database=Test;uid=sa;pwd=Admin888"
            ;

        public static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddLog4Net();

            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(options =>
            {
                options
                    .UseLoggerFactory(loggerFactory)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .UseSqlServer(DbConnectionString);
            });

            services.AddEFRepository()
                // .AddRepository<TestDbContext>()
                ;

            DependencyResolver.SetDependencyResolver(services);

            //DependencyResolver.Current.ResolveService<IEFRepositoryGenerator>()
            //    .GenerateRepositoryCodeFor<TestDbContext>("WeihanLi.EntityFramework.Samples.Business");

            AutoAuditTest();
            //
            Console.WriteLine("completed");
            Console.ReadLine();
        }

        private static void RepositoryTest()
        {
            DependencyResolver.Current.TryInvokeService<TestDbContext>(db =>
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var conn = db.Database.GetDbConnection();
                conn.Execute(@"TRUNCATE TABLE TestEntities");

                conn.Execute(@"
            INSERT INTO TestEntities
            (
            Extra,
            CreatedAt
            )
            VALUES
            (
            '{""Name"":""AA""}',
            GETUTCDATE()
            )
            ");

                var abc = db.TestEntities.AsNoTracking().ToArray();
                Console.WriteLine($"{string.Join(Environment.NewLine, abc.Select(_ => _.ToJson()))}");
                var names = db.TestEntities.AsNoTracking().Select(t => DbFunctions.JsonValue(t.Extra, "$.Name"))
                    .ToArray();
                Console.WriteLine($"Names: {names.StringJoin(",")}");
            });

            DependencyResolver.Current.TryInvokeService<IEFRepository<TestDbContext, TestEntity>>(repo =>
            {
                repo.Insert(new TestEntity() { Extra = "{}", CreatedAt = DateTime.UtcNow, });
                repo.Insert(new TestEntity() { Extra = "{}", CreatedAt = DateTime.UtcNow, });

                var foundEntity = repo.FindAsync(1).GetAwaiter().GetResult();

                repo.Update(new TestEntity
                {
                    Extra = new { Name = "Abcde", Count = 4 }.ToJson(),
                    CreatedAt = DateTime.UtcNow,
                    Id = 1
                }, t => t.CreatedAt, t => t.Extra);

                // repo.UpdateWithout(new TestEntity() { Id = 2, Extra = new { Name = "ADDDDD" }.ToJson() }, x => x.CreatedAt);

                repo.Insert(new[]
                {
                    new TestEntity
                    {
                        Extra = new {Name = "Abcdes"}.ToJson(),
                        CreatedAt = DateTime.Now
                    },
                    new TestEntity
                    {
                        Extra = new {Name = "Abcdes"}.ToJson(),
                        CreatedAt = DateTime.Now
                    }
                });
                var list = repo.GetResult(_ => _.Id).ToArray();
                Console.WriteLine($"Ids: {list.StringJoin(",")}");

                repo.Get(queryBuilder => queryBuilder
                    .WithOrderBy(q => q.OrderByDescending(_ => _.Id)));

                var lastItem = repo.FirstOrDefault(queryBuilder => queryBuilder
                    .WithOrderBy(q => q.OrderByDescending(_ => _.Id)));

                var list1 = repo.GetPagedListResult(x => x.Id, queryBuilder => queryBuilder
                    .WithOrderBy(query => query.OrderByDescending(q => q.Id)), 2, 2
                );

                var pagedList = repo.GetPagedListResult(x => x.Id, queryBuilder => queryBuilder
                    .WithOrderBy(query => query.OrderByDescending(q => q.Id))
                , 1, 2);
                Console.WriteLine(pagedList.ToJson());

                repo.Delete(t => DbFunctions.JsonValue(t.Extra, "$.Name") == "Abcdes");

                Console.WriteLine($"Count: {repo.Count()}");
            });

            DependencyResolver.Current.TryInvokeService<TestDbContext>(db =>
            {
                var conn = db.Database.GetDbConnection();
                conn.Execute($@"
TRUNCATE TABLE TestEntities
");
            });
        }

        private class AuditFileStore : IAuditStore
        {
            private readonly string _fileName;

            public AuditFileStore()
            {
                _fileName = "audits.log";
            }

            public AuditFileStore(string fileName)
            {
                _fileName = fileName.GetValueOrDefault("audits.log");
            }

            public async Task Save(ICollection<AuditEntry> auditEntries)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), _fileName);

                using (var fileStream = File.Exists(path)
                    ? new FileStream(path, FileMode.Append)
                    : File.Create(path)
                    )
                {
                    await fileStream.WriteAsync(auditEntries.ToJson().GetBytes());
                }
            }
        }

        private static void AutoAuditTest()
        {
            // 审计配置
            AuditConfig.Configure(builder =>
            {
                builder
                    // 配置操作用户获取方式
                    .WithUserIdProvider(EnvironmentAuditUserIdProvider.Instance.Value)
                    //.WithUnModifiedProperty() // 保存未修改的属性,默认只保存发生修改的属性
                    // 保存更多属性
                    .EnrichWithProperty("MachineName", Environment.MachineName)
                    .EnrichWithProperty(nameof(ApplicationHelper.ApplicationName), ApplicationHelper.ApplicationName)
                    // 保存到自定义的存储
                    .WithStore<AuditFileStore>()
                    .WithStore<AuditFileStore>("logs0.log")
                    // 忽略指定实体
                    .IgnoreEntity<AuditRecord>()
                    // 忽略指定实体的某个属性
                    .IgnoreProperty<TestEntity>(t => t.CreatedAt)
                    // 忽略所有属性名称为 CreatedAt 的属性
                    .IgnoreProperty("CreatedAt")
                    ;
            });

            DependencyResolver.TryInvokeService<TestDbContext>(dbContext =>
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                var testEntity = new TestEntity()
                {
                    Extra = new { Name = "Tom" }.ToJson(),
                    CreatedAt = DateTimeOffset.UtcNow,
                };
                dbContext.TestEntities.Add(testEntity);
                dbContext.SaveChanges();

                testEntity.CreatedAt = DateTimeOffset.Now;
                testEntity.Extra = new { Name = "Jerry" }.ToJson();
                dbContext.SaveChanges();

                dbContext.Remove(testEntity);
                dbContext.SaveChanges();

                var testEntity1 = new TestEntity()
                {
                    Extra = new { Name = "Tom1" }.ToJson(),
                    CreatedAt = DateTimeOffset.UtcNow,
                };
                dbContext.TestEntities.Add(testEntity1);
                var testEntity2 = new TestEntity()
                {
                    Extra = new { Name = "Tom2" }.ToJson(),
                    CreatedAt = DateTimeOffset.UtcNow,
                };
                dbContext.TestEntities.Add(testEntity2);
                dbContext.SaveChanges();
            });
            DependencyResolver.TryInvokeService<TestDbContext>(dbContext =>
            {
                dbContext.Remove(new TestEntity()
                {
                    Id = 2
                });
                dbContext.SaveChanges();
            });
            // disable audit
            AuditConfig.DisableAudit();
            // enable audit
            // AuditConfig.EnableAudit();
        }
    }
}
