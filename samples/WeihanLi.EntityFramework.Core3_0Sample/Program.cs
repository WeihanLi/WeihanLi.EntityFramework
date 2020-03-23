using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeihanLi.Common;
using WeihanLi.Common.Data;
using WeihanLi.Common.Helpers;
using WeihanLi.EntityFramework.Samples;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Core3_0Sample
{
    public class Program
    {
        private const string DbConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TestDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;"
        //"server=.;database=TestDb;uid=sa;pwd=Admin888"
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
                    //.EnableDetailedErrors()
                    //.EnableSensitiveDataLogging()
                    .UseSqlServer(DbConnectionString)
                    //.AddInterceptors(new QueryWithNoLockDbCommandInterceptor())
                    ;
            });

            services.AddEFRepository()
                ;

            DependencyResolver.SetDependencyResolver(services);

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

                repo.FindAsync(1).AsTask().Wait();

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

                Console.ForegroundColor = originColor;

                uow.Commit();

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"uow count2: {uow.DbSet<TestEntity>().Count()}");
                Console.WriteLine("********** UnitOfWork ************");

                Console.ForegroundColor = originColor;
            });

            DependencyResolver.Current.TryInvokeService<TestDbContext>(db =>
            {
                var conn = db.Database.GetDbConnection();
                conn.Execute($@"
TRUNCATE TABLE TestEntities
");
            });

            Console.WriteLine("completed");
            Console.ReadLine();
        }
    }
}
