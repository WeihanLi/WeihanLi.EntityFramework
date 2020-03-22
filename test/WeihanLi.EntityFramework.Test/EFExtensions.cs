using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework.Test
{
    internal static class EFExtensions
    {
        public static bool CleanData<TDbContext>([NotNull]this TDbContext dbContext) where TDbContext : DbContext
        {
            if (dbContext.Database.IsInMemory())
            {
                return dbContext.Database.EnsureDeleted();
            }
            else
            {
                dbContext.Database.EnsureCreated();
                dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE TestEntities");
            }

            return true;
        }
    }
}
