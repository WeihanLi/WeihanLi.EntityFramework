using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeihanLi.Common;
using WeihanLi.Common.Data;
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

            DependencyResolver.Current.TryInvokeService<TestDbContext>(db =>
            {
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

            //
            Console.ReadLine();
        }
    }
}
