using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.Json;
using WeihanLi.Common;
using WeihanLi.Common.Data;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Services;
using WeihanLi.EntityFramework.Audit;
using WeihanLi.EntityFramework.Interceptors;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Sample;

public static class Program
{
    public static void Main(string[] args)
    {
        // SoftDeleteTest();
        // RepositoryTest();
        AutoAuditTest();

        Console.WriteLine("completed");
        Console.ReadLine();
    }

    private static void AutoAuditTest()
    {
        // {
        //     var services = new ServiceCollection();
        //     services.AddLogging(builder => builder.AddDefaultDelegateLogger());
        //     services.AddDbContext<AutoAuditContext1>(options =>
        //     {
        //         options.UseSqlite("Data Source=AutoAuditTest1.db");
        //     });
        //     services.AddEFAutoAudit(builder =>
        //     {
        //         builder
        //             .WithUserIdProvider(new DelegateUserIdProvider(() => "AutoAuditTest1"))
        //             .EnrichWithProperty("MachineName", Environment.MachineName)
        //             .EnrichWithProperty(nameof(ApplicationHelper.ApplicationName), ApplicationHelper.ApplicationName)
        //             // 保存到自定义的存储
        //             .WithStore<AuditConsoleStore>("logs0.log")
        //             // 忽略指定实体
        //             .IgnoreEntity<AuditRecord>();
        //     });
        //     using var serviceProvider = services.BuildServiceProvider();
        //     using var scope = serviceProvider.CreateScope();
        //     var context = scope.ServiceProvider.GetRequiredService<AutoAuditContext1>();
        //     context.Database.EnsureDeleted();
        //     context.Database.EnsureCreated();
        //     context.Jobs.Add(new TestJobEntity() { Name = "test1" });
        //     context.SaveChanges();
        //     var job = context.Jobs.Find(1);
        //     if (job is not null)
        //     {
        //         context.Jobs.Remove(job);
        //         context.SaveChanges();
        //     }
        //
        //     var auditRecords = context.AuditRecords.AsNoTracking().ToArray();
        //     Console.WriteLine(auditRecords.ToJson());
        // }
        // ConsoleHelper.ReadLineWithPrompt();
        // {
        //     var services = new ServiceCollection();
        //     services.AddLogging(builder => builder.AddDefaultDelegateLogger());
        //     services.AddDbContext<AutoAuditContext2>((provider, options) =>
        //     {
        //         options.UseSqlite("Data Source=AutoAuditTest2.db");
        //         options.AddInterceptors(provider.GetRequiredService<AuditInterceptor>());
        //     });
        //     services.AddEFAutoAudit(builder =>
        //     {
        //         builder.EnrichWithProperty("AutoAudit", "EntityFramework")
        //             .EnrichWithProperty(nameof(ApplicationHelper.ApplicationName), ApplicationHelper.ApplicationName)
        //             .WithStore<AuditConsoleStore>()
        //             .WithAuditRecordsDbContextStore(options =>
        //             {
        //                 options.UseSqlite("Data Source=AutoAuditAuditRecords.db");
        //             });
        //     });
        //     using var serviceProvider = services.BuildServiceProvider();
        //     using var scope = serviceProvider.CreateScope();
        //     var context = scope.ServiceProvider.GetRequiredService<AutoAuditContext2>();
        //     context.Database.EnsureDeleted();
        //     context.Database.EnsureCreated();
        //     context.Jobs.Add(new TestJobEntity() { Name = "test1" });
        //     context.SaveChanges();
        //     var job = context.Jobs.Find(1);
        //     if (job is not null)
        //     {
        //         context.Jobs.Remove(job);
        //         context.SaveChanges();
        //     }
        //
        //     var auditRecordsContext = scope.ServiceProvider.GetRequiredService<AuditRecordsDbContext>();
        //     var auditRecords = auditRecordsContext.AuditRecords.AsNoTracking().ToArray();
        //     Console.WriteLine(auditRecords.ToJson());
        // }

        {
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddDefaultDelegateLogger();
            });
            services.AddDbContext<TestDbContext>((provider, options) =>
            {
                options
                    .UseSqlite("Data Source=Test.db")
                    // .AddInterceptors(ActivatorUtilities.GetServiceOrCreateInstance<AuditInterceptor>(provider))
                    ;
            });
            services.AddDbContextInterceptor<TestDbContext, AuditInterceptor>();

            services.AddEFAutoAudit(builder =>
            {
                builder
                    // 配置操作用户获取方式
                    .WithUserIdProvider(new DelegateUserIdProvider(() => "EFTest"))
                    //.WithUnModifiedProperty() // 保存未修改的属性,默认只保存发生修改的属性
                    // 保存更多属性
                    .EnrichWithProperty("MachineName", Environment.MachineName)
                    .EnrichWithProperty(nameof(ApplicationHelper.ApplicationName), ApplicationHelper.ApplicationName)
                    // 保存到自定义的存储
                    .WithStore<AuditConsoleStore>()
                    // 忽略指定实体
                    .IgnoreEntity<AuditRecord>()
                    // 忽略指定实体的某个属性
                    .IgnoreProperty<TestEntity>(t => t.CreatedAt)
                    // 忽略所有属性名称为 CreatedAt 的属性
                    .IgnoreProperty("CreatedAt")
                    ;
            });
            DependencyResolver.SetDependencyResolver(services);
            DependencyResolver.TryInvoke<TestDbContext>(dbContext =>
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                var testEntity = new TestEntity()
                {
                    Extra = new { Name = "Tom" }.ToJson(),
                    CreatedAt = DateTimeOffset.Now,
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
                    CreatedAt = DateTimeOffset.Now,
                };
                dbContext.TestEntities.Add(testEntity1);
                var testEntity2 = new TestEntity()
                {
                    Extra = new { Name = "Tom2" }.ToJson(),
                    CreatedAt = DateTimeOffset.Now,
                };
                dbContext.TestEntities.Add(testEntity2);
                dbContext.SaveChanges();
            });
            DependencyResolver.TryInvokeAsync<TestDbContext>(async dbContext =>
            {
                dbContext.Remove(new TestEntity() { Id = 2 });
                await dbContext.SaveChangesAsync();
            }).Wait();
        }
    }

    private static void RepositoryTest()
    {
        var services = new ServiceCollection();
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddDefaultDelegateLogger();
        });
        services.AddDbContext<TestDbContext>((provider, options) =>
        {
            options
                //.EnableDetailedErrors()
                //.EnableSensitiveDataLogging()
                //.UseInMemoryDatabase("Tests")
                .UseSqlite("Data Source=Test.db")
                .AddInterceptors(ActivatorUtilities.GetServiceOrCreateInstance<AuditInterceptor>(provider))
                //.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TestDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;")
                ;
        });
        services.AddEFRepository();
        DependencyResolver.SetDependencyResolver(services);

        DependencyResolver.Current.TryInvokeService<TestDbContext>(db =>
        {
            db.Database.EnsureCreated();
            var tableName = db.GetTableName<TestEntity>();

            if (db.Database.IsRelational())
            {
                var conn = db.Database.GetDbConnection();
                try
                {
                    conn.Execute($@"TRUNCATE TABLE {tableName}");
                }
                catch
                {
                    db.Set<TestEntity>().ExecuteDelete();
                }
            }

            var repo = db.GetRepository<TestDbContext, TestEntity>();
            repo.Insert(new TestEntity()
            {
                CreatedAt = DateTimeOffset.Now,
                Extra = "{\"Name\": \"Tom\"}"
            });

            repo.Update(x => x.Extra != null, x => x.Extra, new { Date = DateTimeOffset.Now }.ToJson());
            System.Console.WriteLine("Extra updated");

            // TODO: this is not working for now
            repo.Update(x => x.Extra != null, new Dictionary<string, object?>()
            {
                { "Extra", "12345"}
            });

            repo.Update(x => x.SetProperty(_ => _.Extra, _ => "{}"), q => q.IgnoreQueryFilters());

            var abc = db.TestEntities.AsNoTracking().ToArray();
            Console.WriteLine($"{string.Join(Environment.NewLine, abc.Select(_ => _.ToJson()))}");

            var data = repo.Query(q => q.WithPredictIf(f => f.Id > 0, false)).ToArray();
            Console.WriteLine(JsonSerializer.Serialize(data));

            repo.Delete(x => x.Id > 0);
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
                Id = list00[0]
            }, t => t.CreatedAt, t => t.Extra);

            repo.UpdateWithout(new TestEntity() { Id = list00[1], Extra = new { Name = "ADDDDD" }.ToJson() }, x => x.CreatedAt);

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
            if (db.Database.IsRelational())
            {
                var conn = db.Database.GetDbConnection();
                try
                {
                    conn.Execute($@"TRUNCATE TABLE {tableName}");
                }
                catch
                {
                    db.Set<TestEntity>().ExecuteDelete();
                }
            }
        });
    }

    private static void SoftDeleteTest()
    {
        var services = new ServiceCollection();
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddDefaultDelegateLogger();
        });

        services.AddSingleton<IUserIdProvider, EnvironmentUserIdProvider>();
        services.AddEFAutoUpdateInterceptor();

        services.AddDbContext<SoftDeleteSampleContext>((provider, options) =>
        {
            options
                .UseSqlite("Data Source=SoftDeleteTest.db")
                .AddInterceptors(provider.GetRequiredService<AutoUpdateInterceptor>());
        });
        using var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SoftDeleteSampleContext>();
        context.Database.EnsureDeleted();
        // initialize
        context.Database.EnsureCreated();
        // delete all in case of before db not got clean up
        context.TestEntities.IgnoreQueryFilters().ExecuteDelete();
        context.SaveChanges();

        // add test data
        context.TestEntities.Add(new SoftDeleteEntity()
        {
            Id = 1,
            Name = "test"
        });
        context.SaveChanges();

        // remove data test
        var testEntity = context.TestEntities.Find(1);
        ArgumentNullException.ThrowIfNull(testEntity);
        context.TestEntities.Remove(testEntity);
        context.SaveChanges();


        context.TestEntities2.Add(new SoftDeleteEntity2()
        {
            Id = 1,
            Name = "test"
        });
        context.SaveChanges();
        var testEntities = context.TestEntities2.AsNoTracking().ToArray();
        var testEntity2 = context.TestEntities2.Find(1);
        ArgumentNullException.ThrowIfNull(testEntity2);
        context.TestEntities2.Remove(testEntity2);
        context.SaveChanges();

        // get all data
        var entities = context.TestEntities.AsNoTracking().ToArray();
        Console.WriteLine(entities.ToJson());

        // get all data without global query filter
        entities = context.TestEntities.AsNoTracking().IgnoreQueryFilters().ToArray();
        Console.WriteLine(entities.ToJson());

        context.Database.EnsureDeleted();
    }

    private static IServiceCollection AddDbContextInterceptor<TContext, TInterceptor>(
        this IServiceCollection services,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped
        )
        where TContext : DbContext
        where TInterceptor : IInterceptor
    {
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction = (sp, builder) =>
        {
            builder.AddInterceptors(sp.GetRequiredService<TInterceptor>());
        };
        services.Add(ServiceDescriptor.Describe(typeof(TInterceptor), typeof(TInterceptor), optionsLifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IDbContextOptionsConfiguration<TContext>), _ =>
            new DbContextOptionsConfiguration<TContext>(optionsAction), optionsLifetime));
        return services;
    }
}


public sealed class AuditConsoleStore : IAuditStore
{
    private readonly string _fileName;

    public AuditConsoleStore() : this("audit-logs.log")
    {
    }
    public AuditConsoleStore(string fileName)
    {
        _fileName = fileName;
    }

    public Task Save(ICollection<AuditEntry> auditEntries)
    {
        foreach (var auditEntry in auditEntries)
        {
            Console.WriteLine(auditEntry.ToJson());
        }

        return Task.CompletedTask;
    }
}

file sealed class AuditFileBatchStore : PeriodBatchingAuditStore
{
    private readonly string _fileName;

    public AuditFileBatchStore() : this(null)
    {
    }

    public AuditFileBatchStore(string? fileName) : base(100, TimeSpan.FromSeconds(10))
    {
        _fileName = fileName.GetValueOrDefault("audits.log");
    }

    protected override async Task EmitBatchAsync(IEnumerable<AuditEntry> events)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), _fileName);

        await using var fileStream = File.Exists(path)
            ? new FileStream(path, FileMode.Append)
            : File.Create(path);
        await fileStream.WriteAsync(events.ToJson().GetBytes());
    }
}
