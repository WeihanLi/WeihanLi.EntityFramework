using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;

namespace WeihanLi.EntityFramework
{
    public static class EFExtensions
    {
        public static EntityEntry<TEntity> Update<TEntity>(this DbContext dbContext, TEntity entity, params string[] propNames) where TEntity : class
        {
            if (propNames == null || propNames.Length == 0)
            {
                return dbContext.Update(entity);
            }

            var entry = dbContext.GetEntityEntry(entity);
            entry.State = EntityState.Unchanged;
            foreach (var expression in propNames)
            {
                entry.Property(expression).IsModified = true;
            }

            return entry;
        }

        public static EntityEntry<TEntity> UpdateWithout<TEntity>(this DbContext dbContext, TEntity entity, params string[] propNames) where TEntity : class
        {
            if (propNames == null || propNames.Length == 0)
            {
                return dbContext.Update(entity);
            }

            var entry = dbContext.GetEntityEntry(entity);
            entry.State = EntityState.Modified;
            foreach (var expression in propNames)
            {
                entry.Property(expression).IsModified = false;
            }

            return entry;
        }

        public static EntityEntry<TEntity> Update<TEntity>(this DbContext dbContext, TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions) where TEntity : class
        {
            if (propertyExpressions == null || propertyExpressions.Length == 0)
            {
                return dbContext.Update(entity);
            }

            var entry = dbContext.GetEntityEntry(entity);
            entry.State = EntityState.Unchanged;
            foreach (var expression in propertyExpressions)
            {
                entry.Property(expression).IsModified = true;
            }

            return entry;
        }

        public static EntityEntry<TEntity> UpdateWithout<TEntity>(this DbContext dbContext, TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions) where TEntity : class
        {
            if (propertyExpressions == null || propertyExpressions.Length == 0)
            {
                return dbContext.Update(entity);
            }

            var entry = dbContext.GetEntityEntry(entity);

            entry.State = EntityState.Modified;
            foreach (var expression in propertyExpressions)
            {
                entry.Property(expression).IsModified = false;
            }

            return entry;
        }

        private static EntityEntry<TEntity> GetEntityEntry<TEntity>(this DbContext dbContext, TEntity entity)
      where TEntity : class
        {
            var internalEntry = dbContext.GetDependencies().StateManager.GetOrCreateEntry(entity);
            return (EntityEntry<TEntity>)internalEntry.ToEntityEntry();
        }
    }
}
