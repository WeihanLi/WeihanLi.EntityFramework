using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework
{
    public static class EFExtensions
    {
        /// <summary>
        /// is relational database used now
        /// Please use dbContext.Database.IsRelational()
        ///     <para>
        ///         Returns true if the database provider currently in use is a relational database.
        ///     </para>
        /// </summary>
        /// <param name="dbContext"> The DbContext <see cref="DbContext.Database" />. </param>
        /// <returns> True if a relational database provider is being used; false otherwise. </returns>
        public static bool IsRelationalDatabase([NotNull] this DbContext dbContext)
        {
            if (null == dbContext)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }
            return dbContext.GetInfrastructure()
                .GetService<IRelationalConnection>() != null;
        }

        public static IEFRepository<TDbContext, TEntity> GetRepository<TDbContext, TEntity>([NotNull] this TDbContext dbContext)
            where TEntity : class
            where TDbContext : DbContext
        {
            return new EFRepository<TDbContext, TEntity>(dbContext);
        }

        public static IEFUnitOfWork<TDbContext> GetUnitOfWork<TDbContext>([NotNull] this TDbContext dbContext)
            where TDbContext : DbContext
        {
            return new EFUnitOfWork<TDbContext>(dbContext);
        }

        public static EntityEntry<TEntity> Remove<TEntity>([NotNull] this DbContext dbContext, params object[] keyValues) where TEntity : class
        {
            var entity = dbContext.Find<TEntity>(keyValues);
            if (entity == null)
            {
                return null;
            }

            return dbContext.Remove(entity);
        }

        public static EntityEntry<TEntity> Update<TEntity>([NotNull] this DbContext dbContext, TEntity entity, params string[] propNames) where TEntity : class
        {
            if (propNames == null || propNames.Length == 0)
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

        public static EntityEntry<TEntity> UpdateWithout<TEntity>([NotNull] this DbContext dbContext, TEntity entity, params string[] propNames) where TEntity : class
        {
            if (propNames == null || propNames.Length == 0)
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

        public static EntityEntry<TEntity> Update<TEntity>([NotNull] this DbContext dbContext, TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions) where TEntity : class
        {
            if (propertyExpressions == null || propertyExpressions.Length == 0)
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

        public static EntityEntry<TEntity> UpdateWithout<TEntity>([NotNull] this DbContext dbContext, TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions) where TEntity : class
        {
            if (propertyExpressions == null || propertyExpressions.Length == 0)
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

        private static EntityEntry<TEntity> GetEntityEntry<TEntity>([NotNull] this DbContext dbContext, TEntity entity, out bool existBefore)
      where TEntity : class
        {
            var type = typeof(TEntity);

            var entityType = dbContext.Model.FindEntityType(type);

            var keysGetter = entityType.FindPrimaryKey().Properties
                .Select(x => x.PropertyInfo.GetValueGetter<TEntity>())
                .ToArray();

            var keyValues = keysGetter
                .Select(x => x.Invoke(entity))
                .ToArray();

            var originalEntity = dbContext.Set<TEntity>().Local
                .FirstOrDefault(x => GetEntityKeyValues(keysGetter, x).SequenceEqual(keyValues));

            EntityEntry<TEntity> entityEntry;
            if (null == originalEntity)
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

        private static object[] GetEntityKeyValues<TEntity>(Func<TEntity, object>[] keyValueGetter, TEntity entity)
        {
            var keyValues = keyValueGetter.Select(x => x.Invoke(entity)).ToArray();
            return keyValues;
        }
    }
}
