using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeihanLi.Common;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddLog4Net();

            EFRepositoryGenerator.GenerateRepositoryCodeFor<TestDbContext>("WeihanLi.EntityFramework.Samples.Business");

            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(options =>
            {
                options
                    .UseLoggerFactory(loggerFactory)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .UseSqlServer("server=.;database=Test;Integrated Security=True");
            });
            services.AddEFRepository();
            DependencyResolver.SetDependencyResolver(services);
            DependencyResolver.Current.TryInvokeService<TestDbContext>(db =>
            {
                var abc = db.TestEntities.AsNoTracking().ToArray();
                Console.WriteLine($"{string.Join(Environment.NewLine, abc.Select(_ => _.ToJson()))}");
                var names = db.TestEntities.AsNoTracking().Select(t => DbFunctions.JsonValue(t.Extra, "$.Name"))
                    .ToArray();
                Console.WriteLine($"Names: {names.StringJoin(",")}");
            });

            DependencyResolver.Current.TryInvokeService<IEFRepository<TestDbContext, TestEntity>>(repo =>
            {
                repo.Update(new TestEntity
                {
                    CreatedAt = DateTime.UtcNow,
                    Extra = new { Name = "Abcde", Count = 4 }.ToJson(),
                    Id = 3
                }, t => t.CreatedAt, t => t.Extra);
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
                var list = repo.Select(_ => _.Id).ToArray();
                Console.WriteLine($"Ids: {list.StringJoin(",")}");
                repo.Select(_ => _.Id, orderBy: q => q.OrderBy(_ => _.CreatedAt));

                repo.Delete(t => DbFunctions.JsonValue(t.Extra, "$.Name") == "Abcdes");
                Console.WriteLine($"Count: {repo.Count(c => true)}");
            });

            //
            Console.ReadLine();
        }
    }
}
