using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddLog4Net();

            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>()
                .UseLoggerFactory(loggerFactory)
                .UseSqlServer("server=.;database=Test;Integrated Security=True");

            var db = new TestDbContext(optionsBuilder.Options);

            var abc = db.TestEntities.AsNoTracking().ToArray();

            var names = db.TestEntities.AsNoTracking().Select(t => DbFunctions.JsonValue(t.Extra, "$.Name")).ToArray();

            //
            Console.ReadLine();
        }
    }
}
