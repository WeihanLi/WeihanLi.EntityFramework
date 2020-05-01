using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeihanLi.Common;
using WeihanLi.Common.Aspect;
using WeihanLi.Common.Data;
using WeihanLi.Common.Helpers;
using WeihanLi.EntityFramework.Audit;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Core3_Sample
{
    public class Program
    {
        private const string DbConnectionString =
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TestDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        public static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddLog4Net();

            var services = new ServiceCollection();
            //services.AddProxyDbContext<TestDbContext>(options =>
            //{
            //    options
            //        .UseLoggerFactory(loggerFactory)
            //        //.EnableDetailedErrors()
            //        //.EnableSensitiveDataLogging()
            //        // .UseInMemoryDatabase("Tests")
            //        .UseSqlServer(DbConnectionString)
            //        //.AddInterceptors(new QueryWithNoLockDbCommandInterceptor())
            //        ;
            //});

            services.AddProxyDbContextPool<TestDbContext>(options =>
            {
                options
                    .UseLoggerFactory(loggerFactory)
                    //.EnableDetailedErrors()
                    //.EnableSensitiveDataLogging()
                    // .UseInMemoryDatabase("Tests")
                    .UseSqlServer(DbConnectionString)
                    //.AddInterceptors(new QueryWithNoLockDbCommandInterceptor())
                    ;
            });
            services.AddEFRepository();
            services.AddFluentAspects(options =>
            {
                options.NoInterceptMethod<DbContext>(m =>
                    m.Name != nameof(DbContext.SaveChanges)
                    && m.Name != nameof(DbContext.SaveChangesAsync));

                options.InterceptMethod<DbContext>(m =>
                        m.Name == nameof(DbContext.SaveChanges)
                        || m.Name == nameof(DbContext.SaveChangesAsync))
                    .With<AuditDbContextInterceptor>()
                    ;
            });
            DependencyResolver.SetDependencyResolver(services);

            AutoAuditTest();

            Console.WriteLine("completed");
            Console.ReadLine();
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

        private static void RepositoryTest()
        {
            DependencyResolver.Current.TryInvokeService<TestDbContext>(db =>
            {
                db.Database.EnsureCreated();
                var tableName = db.GetTableName<TestEntity>();

                var conn = db.Database.GetDbConnection();
                conn.Execute($@"TRUNCATE TABLE {tableName}");

                db.GetRepository<TestDbContext, TestEntity>().Insert(new TestEntity()
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    Extra = "{\"Name\": \"Tom\"}"
                });

                //                conn.Execute($@"
                //INSERT INTO {tableName}
                //(
                //Extra,
                //CreatedAt
                //)
                //VALUES
                //(
                //'{{""Name"":""AA""}}',
                //GETUTCDATE()
                //)
                //");

                var abc = db.TestEntities.AsNoTracking().ToArray();
                Console.WriteLine($"{string.Join(Environment.NewLine, abc.Select(_ => _.ToJson()))}");
            });

            DependencyResolver.Current.TryInvokeService<IEFRepositoryFactory<TestDbContext>>(repoFactory =>
            {
                var repo = repoFactory.GetRepository<TestEntity>();
                var count = repo.Count();
                Console.WriteLine(count);
            });

            DependencyResolver.Current.TryInvokeService<IEFRepository<TestDbContext, TestEntity>>(repo =>
            {
                var ids0 = repo.GetResult(_ => _.Id).ToArray();
                Console.WriteLine($"Ids: {ids0.StringJoin(",")}");

                var list0 = repo.GetResult(_ => _.Id, queryBuilder => queryBuilder.WithPredict(t => t.Id > 0)).ToArray();
                Console.WriteLine($"Ids: {list0.StringJoin(",")}");

                repo.Insert(new TestEntity() { Extra = "{}", CreatedAt = DateTime.UtcNow, });
                repo.Insert(new TestEntity() { Extra = "{}", CreatedAt = DateTime.UtcNow, });

                var foundEntity = repo.Find(1);

                var whereExpression = ExpressionHelper.True<TestEntity>();
                Expression<Func<TestEntity, bool>> idExp = t => t.Id > 0;
                var whereExpression1 = whereExpression
                    .And(t => t.Id > 0)
                    .And(ExpressionHelper.True<TestEntity>())
                    .And(t => t.Id > -1);

                var abcExp = Expression.Lambda<Func<TestEntity, bool>>
                    (Expression.AndAlso(idExp.Body, whereExpression.Body), idExp.Parameters);

                var list00 = repo.GetResult(_ => _.Id, queryBuilder =>
                    queryBuilder.WithPredict(whereExpression1)).ToArray();
                var list01 = repo.GetResult(_ => _.Id, queryBuilder =>
                    queryBuilder.WithPredict(abcExp)).ToArray();
                Console.WriteLine($"Ids: {list00.StringJoin(",")}");

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

                Console.WriteLine($"Count: {repo.Count()}");
            });

            DependencyResolver.Current.TryInvokeService<IEFUnitOfWork<TestDbContext>>(uow =>
            {
                var originColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine("********** UnitOfWork ************");
                Console.WriteLine($"uow count0: {uow.DbSet<TestEntity>().Count()}");

                uow.DbSet<TestEntity>().Add(new TestEntity() { CreatedAt = DateTime.UtcNow, Extra = "1212", });

                Console.WriteLine($"uow count1: {uow.DbSet<TestEntity>().Count()}");

                uow.DbSet<TestEntity>().Add(new TestEntity() { CreatedAt = DateTime.UtcNow, Extra = "1212", });

                uow.GetRepository<TestEntity>().Delete(uow.DbContext.TestEntities.First());

                Console.ForegroundColor = originColor;

                uow.Commit();

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"uow count2: {uow.DbSet<TestEntity>().Count()}");
                Console.WriteLine("********** UnitOfWork ************");

                Console.ForegroundColor = originColor;
            });

            DependencyResolver.Current.TryInvokeService<TestDbContext>(db =>
            {
                var tableName = db.GetTableName<TestEntity>();
                var conn = db.Database.GetDbConnection();
                conn.Execute($@"
TRUNCATE TABLE {tableName}
");
            });
        }
    }
}
