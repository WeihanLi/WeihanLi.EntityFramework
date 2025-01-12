using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;
using System.Linq.Expressions;
using WeihanLi.Common.Models;
using WeihanLi.Common.Services;
using WeihanLi.EntityFramework.Audit;
using WeihanLi.EntityFramework.Interceptors;
using WeihanLi.EntityFramework.Services;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework;

public static class EFExtensions
{
    public static IEFRepository<TDbContext, TEntity> GetRepository<TDbContext, TEntity>(this TDbContext dbContext)
        where TEntity : class
        where TDbContext : DbContext
    {
        return new EFRepository<TDbContext, TEntity>(dbContext);
    }

    public static IEFUnitOfWork<TDbContext> GetUnitOfWork<TDbContext>(this TDbContext dbContext)
        where TDbContext : DbContext
    {
        return new EFUnitOfWork<TDbContext>(dbContext);
    }

    public static IEFUnitOfWork<TDbContext> GetUnitOfWork<TDbContext>(TDbContext dbContext, IsolationLevel isolationLevel)
        where TDbContext : DbContext
    {
        return new EFUnitOfWork<TDbContext>(dbContext, isolationLevel);
    }

    public static EntityEntry<TEntity>? Remove<TEntity>(this DbContext dbContext, params object[] keyValues) where TEntity : class
    {
        var entity = dbContext.Find<TEntity>(keyValues);
        if (entity == null)
        {
            return null;
        }

        return dbContext.Remove(entity);
    }

    public static EntityEntry<TEntity> Update<TEntity>(this DbContext dbContext, TEntity entity, params string[] propNames) where TEntity : class
    {
        if (propNames.IsNullOrEmpty())
        {
            return dbContext.Update(entity);
        }
        var entry = dbContext.GetEntityEntry(entity, out var existBefore);
        if (existBefore)
        {
            foreach (var propEntry in entry.Properties)
            {
                if (!propNames.Contains(propEntry.Metadata.Name))
                {
                    propEntry.IsModified = false;
                }
            }
        }
        else
        {
            entry.State = EntityState.Unchanged;
            foreach (var propName in propNames)
            {
                entry.Property(propName).IsModified = true;
            }
        }

        return entry;
    }

    public static EntityEntry<TEntity> UpdateWithout<TEntity>(this DbContext dbContext, TEntity entity, params string[] propNames) where TEntity : class
    {
        if (propNames.IsNullOrEmpty())
        {
            return dbContext.Update(entity);
        }
        var entry = dbContext.GetEntityEntry(entity, out _);
        entry.State = EntityState.Modified;
        foreach (var expression in propNames)
        {
            entry.Property(expression).IsModified = false;
        }

        return entry;
    }

    public static EntityEntry<TEntity> Update<TEntity>(this DbContext dbContext, TEntity entity, params Expression<Func<TEntity, object?>>[] propertyExpressions) where TEntity : class
    {
        if (propertyExpressions.IsNullOrEmpty())
        {
            return dbContext.Update(entity);
        }

        var entry = dbContext.GetEntityEntry(entity, out var existBefore);

        if (existBefore)
        {
            var propNames = propertyExpressions.Select(x => x.GetMemberName()).ToArray();

            foreach (var propEntry in entry.Properties)
            {
                if (!propNames.Contains(propEntry.Metadata.Name))
                {
                    propEntry.IsModified = false;
                }
            }
        }
        else
        {
            entry.State = EntityState.Unchanged;
            foreach (var expression in propertyExpressions)
            {
                entry.Property(expression).IsModified = true;
            }
        }

        return entry;
    }

    public static EntityEntry<TEntity> UpdateWithout<TEntity>(this DbContext dbContext, TEntity entity, params Expression<Func<TEntity, object?>>[] propertyExpressions) where TEntity : class
    {
        if (propertyExpressions.IsNullOrEmpty())
        {
            return dbContext.Update(entity);
        }

        var entry = dbContext.GetEntityEntry(entity, out _);

        entry.State = EntityState.Modified;
        foreach (var expression in propertyExpressions)
        {
            entry.Property(expression).IsModified = false;
        }

        return entry;
    }

    public static string GetTableName<TEntity>(this DbContext dbContext)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        ArgumentNullException.ThrowIfNull(entityType);
        return entityType.GetTableName()!;
    }

    public static KeyEntry[] GetKeyValues(this EntityEntry entityEntry)
    {
        if (!entityEntry.IsKeySet)
            return [];

        var keyProps = entityEntry.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .ToArray();
        if (keyProps.Length == 0)
            return [];

        var keyEntries = new KeyEntry[keyProps.Length];
        for (var i = 0; i < keyProps.Length; i++)
        {
            keyEntries[i] = new KeyEntry()
            {
                PropertyName = keyProps[i].Metadata.Name,
                ColumnName = keyProps[i].GetColumnName(),
                Value = keyProps[i].CurrentValue,
            };
        }

        return keyEntries;
    }

    public static IServiceCollection AddEFAutoUpdateInterceptor(this IServiceCollection services)
        => services.AddEFAutoUpdateInterceptor(ServiceLifetime.Scoped);

    public static IServiceCollection AddEFAutoUpdateInterceptor(this IServiceCollection services,
        ServiceLifetime userProviderLifetime)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<AutoUpdateInterceptor>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEntitySavingHandler, SoftDeleteEntitySavingHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IEntitySavingHandler, UpdatedAtEntitySavingHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IEntitySavingHandler), typeof(UpdatedBySavingHandler), userProviderLifetime));

        return services;
    }

    public static IServiceCollection AddEFAutoUpdateInterceptor<TUserProvider>(this IServiceCollection services,
        ServiceLifetime userProviderLifetime = ServiceLifetime.Scoped)
        where TUserProvider : class, IUserIdProvider
    {
        services.AddEFAutoUpdateInterceptor(userProviderLifetime)
            .TryAdd(ServiceDescriptor.Describe(typeof(IUserIdProvider), typeof(TUserProvider), userProviderLifetime));
        return services;
    }

    public static IServiceCollection AddEFAutoAudit(this IServiceCollection services,
        Action<IAuditConfigBuilder> configAction)
    {
        ArgumentNullException.ThrowIfNull(configAction);

        services.TryAddScoped<AuditInterceptor>();
        AuditConfig.Configure(services, configAction);
        return services;
    }

    public static EntityTypeBuilder<TEntity> WithSoftDeleteFilter<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder)
        where TEntity : class, ISoftDeleteEntityWithDeleted
    {
        ArgumentNullException.ThrowIfNull(entityTypeBuilder);
        return entityTypeBuilder.HasQueryFilter(x => x.IsDeleted == false);
    }

    public static IServiceCollection AddDbContextInterceptor<TContext, TInterceptor>(
        this IServiceCollection services,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped
    )
        where TContext : DbContext
        where TInterceptor : IInterceptor
    {
        ArgumentNullException.ThrowIfNull(services);
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction = (sp, builder) =>
        {
            builder.AddInterceptors(sp.GetRequiredService<TInterceptor>());
        };
        services.TryAdd(ServiceDescriptor.Describe(typeof(TInterceptor), typeof(TInterceptor), optionsLifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IDbContextOptionsConfiguration<TContext>),
            _ => new DbContextOptionsConfiguration<TContext>(optionsAction), optionsLifetime));
        return services;
    }

    private static EntityEntry<TEntity> GetEntityEntry<TEntity>(this DbContext dbContext, TEntity entity, out bool existBefore)
  where TEntity : class
    {
        var type = typeof(TEntity);

        var entityType = dbContext.Model.FindEntityType(type);
        var key = entityType?.FindPrimaryKey();
        if (key is null)
        {
            throw new InvalidOperationException($"Type {type.FullName} had no primary key");
        }

        var keysGetter = key.Properties
            .Select(x => x.PropertyInfo!.GetValueGetter<TEntity>())
            .ToArray();
        var keyValues = keysGetter
            .Select(x => x?.Invoke(entity))
            .ToArray();

        var originalEntity = dbContext.Set<TEntity>().Local
            .FirstOrDefault(x => GetEntityKeyValues(keysGetter, x).SequenceEqual(keyValues));

        EntityEntry<TEntity> entityEntry;
        if (originalEntity is null)
        {
            existBefore = false;
            entityEntry = dbContext.Attach(entity);
        }
        else
        {
            existBefore = true;
            entityEntry = dbContext.Entry(originalEntity);
            entityEntry.CurrentValues.SetValues(entity);
        }

        return entityEntry;
    }

    private static object?[] GetEntityKeyValues<TEntity>(Func<TEntity, object?>?[] keyValueGetter, TEntity entity)
    {
        var keyValues = keyValueGetter.Select(x => x?.Invoke(entity)).ToArray();
        return keyValues;
    }
}
