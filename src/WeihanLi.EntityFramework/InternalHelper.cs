using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using WeihanLi.Common;

namespace WeihanLi.EntityFramework;

public static class InternalHelper
{
    public static PropertyInfo[] GetDbContextSets(Type dbContextType)
    {
        return CacheUtil.GetTypeProperties(dbContextType)
            .Where(p => p.PropertyType.IsGenericType && typeof(DbSet<>) == p.PropertyType.GetGenericTypeDefinition())
            .ToArray();
    }
}
