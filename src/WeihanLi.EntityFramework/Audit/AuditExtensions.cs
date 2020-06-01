using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WeihanLi.Common.Aspect;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Services;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Audit
{
    public static class AuditExtensions
    {
        #region AuditEntry

        public static bool WithProperty(this AuditEntry auditEntry, string propertyName,
    object propertyValue, bool overwrite = false)
        {
            if (null == auditEntry)
            {
                throw new ArgumentNullException(nameof(auditEntry));
            }

            if (auditEntry.Properties.ContainsKey(propertyName) && overwrite == false)
            {
                return false;
            }

            auditEntry.Properties[propertyName] = propertyValue;
            return true;
        }

        public static bool WithProperty(this AuditEntry auditEntry, string propertyName,
            Func<AuditEntry, object> propertyValueFactory, bool overwrite = false)
        {
            if (null == auditEntry)
            {
                throw new ArgumentNullException(nameof(auditEntry));
            }

            if (auditEntry.Properties.ContainsKey(propertyName) && overwrite == false)
            {
                return false;
            }

            auditEntry.Properties[propertyName] = propertyValueFactory?.Invoke(auditEntry);
            return true;
        }

        #endregion AuditEntry

        #region IAuditConfigBuilder

        public static IAuditConfigBuilder IgnoreEntity(this IAuditConfigBuilder configBuilder, Type entityType)
        {
            configBuilder.WithEntityFilter(entityEntry => entityEntry.Entity.GetType() != entityType);
            return configBuilder;
        }

        public static IAuditConfigBuilder IgnoreEntity<TEntity>(this IAuditConfigBuilder configBuilder) where TEntity : class
        {
            configBuilder.WithEntityFilter(entityEntry => entityEntry.Entity.GetType() != typeof(TEntity));
            return configBuilder;
        }

        public static IAuditConfigBuilder IgnoreTable(this IAuditConfigBuilder configBuilder, string tableName)
        {
            configBuilder.WithEntityFilter(entityEntry => entityEntry.Metadata.GetTableName() != tableName);
            return configBuilder;
        }

        public static IAuditConfigBuilder WithEntityFilter(this IAuditConfigBuilder configBuilder, Func<EntityEntry, bool> filterFunc)
        {
            configBuilder.WithEntityFilter(filterFunc);
            return configBuilder;
        }

        public static IAuditConfigBuilder IgnoreProperty<TEntity>(this IAuditConfigBuilder configBuilder, Expression<Func<TEntity, object>> propertyExpression) where TEntity : class
        {
            var propertyName = propertyExpression.GetMemberName();
            configBuilder.WithPropertyFilter(propertyEntry => propertyEntry.Metadata.Name != propertyName);
            return configBuilder;
        }

        public static IAuditConfigBuilder IgnoreProperty(this IAuditConfigBuilder configBuilder, string propertyName)
        {
            configBuilder.WithPropertyFilter(propertyEntry => propertyEntry.Metadata.Name != propertyName);
            return configBuilder;
        }

        public static IAuditConfigBuilder IgnoreColumn(this IAuditConfigBuilder configBuilder, string columnName)
        {
            configBuilder.WithPropertyFilter(propertyEntry => propertyEntry.Metadata.GetColumnName() != columnName);
            return configBuilder;
        }

        public static IAuditConfigBuilder IgnoreColumn(this IAuditConfigBuilder configBuilder, string tableName, string columnName)
        {
            configBuilder.WithPropertyFilter((entityEntry, propertyEntry) => entityEntry.Metadata.GetTableName() != tableName
                                                                            && propertyEntry.Metadata.GetColumnName() != columnName);
            return configBuilder;
        }

        public static IAuditConfigBuilder WithPropertyFilter(this IAuditConfigBuilder configBuilder, Func<PropertyEntry, bool> filterFunc)
        {
            configBuilder.WithPropertyFilter((entity, prop) => filterFunc.Invoke(prop));
            return configBuilder;
        }

        public static IAuditConfigBuilder WithUserIdProvider<TUserIdProvider>(this IAuditConfigBuilder configBuilder) where TUserIdProvider : IUserIdProvider, new()
        {
            configBuilder.WithUserIdProvider(new TUserIdProvider());
            return configBuilder;
        }

        public static IAuditConfigBuilder WithEnricher<TEnricher>(this IAuditConfigBuilder configBuilder) where TEnricher : IAuditPropertyEnricher, new()
        {
            configBuilder.WithEnricher(new TEnricher());
            return configBuilder;
        }

        public static IAuditConfigBuilder WithEnricher<TEnricher>(this IAuditConfigBuilder configBuilder, params object[] parameters) where TEnricher : IAuditPropertyEnricher
        {
            configBuilder.WithEnricher(ActivatorHelper.CreateInstance<TEnricher>(parameters));
            return configBuilder;
        }

        public static IAuditConfigBuilder WithStore<TAuditStore>(this IAuditConfigBuilder configBuilder) where TAuditStore : IAuditStore, new()
        {
            configBuilder.WithStore(new TAuditStore());
            return configBuilder;
        }

        public static IAuditConfigBuilder WithStore<TAuditStore>(this IAuditConfigBuilder configBuilder, params object[] parameters) where TAuditStore : IAuditStore
        {
            configBuilder.WithStore(ActivatorHelper.CreateInstance<TAuditStore>(parameters));
            return configBuilder;
        }

        public static IAuditConfigBuilder EnrichWithProperty(this IAuditConfigBuilder configBuilder, string propertyName, object value, bool overwrite = false)
        {
            configBuilder.WithEnricher(new AuditPropertyEnricher(propertyName, value, overwrite));
            return configBuilder;
        }

        public static IAuditConfigBuilder EnrichWithProperty(this IAuditConfigBuilder configBuilder, string propertyName, Func<AuditEntry> valueFactory, bool overwrite = false)
        {
            configBuilder.WithEnricher(new AuditPropertyEnricher(propertyName, valueFactory, overwrite));
            return configBuilder;
        }

        public static IAuditConfigBuilder EnrichWithProperty(this IAuditConfigBuilder configBuilder, string propertyName, object value, Func<AuditEntry, bool> predict, bool overwrite = false)
        {
            configBuilder.WithEnricher(new AuditPropertyEnricher(propertyName, e => value, predict, overwrite));
            return configBuilder;
        }

        public static IAuditConfigBuilder EnrichWithProperty(this IAuditConfigBuilder configBuilder, string propertyName, Func<AuditEntry, object> valueFactory, Func<AuditEntry, bool> predict, bool overwrite = false)
        {
            configBuilder.WithEnricher(new AuditPropertyEnricher(propertyName, valueFactory, predict, overwrite));
            return configBuilder;
        }

        #endregion IAuditConfigBuilder

        #region FluentAspectOptions

        public static IInterceptionConfiguration InterceptDbContextSave(this FluentAspectOptions options)
        {
            return options.InterceptMethod<DbContext>(m =>
                    m.Name == nameof(DbContext.SaveChanges)
                    || m.Name == nameof(DbContext.SaveChangesAsync));
        }

        public static IInterceptionConfiguration InterceptDbContextSave<TDbContext>(this FluentAspectOptions options) where TDbContext : DbContext
        {
            return options.Intercept(c => c.Target?.GetType().IsAssignableTo<TDbContext>() == true
                                   &&
                                   (c.ProxyMethod.Name == nameof(DbContext.SaveChanges)
                                    || c.ProxyMethod.Name == nameof(DbContext.SaveChangesAsync)
                                   )
            );
        }

        public static FluentAspectOptions InterceptDbContextSaveWithAudit(this FluentAspectOptions options)
        {
            options.InterceptMethod<DbContext>(m =>
                    m.Name == nameof(DbContext.SaveChanges)
                    || m.Name == nameof(DbContext.SaveChangesAsync))
                .With<AuditDbContextInterceptor>();
            return options;
        }

        public static FluentAspectOptions InterceptDbContextSaveWithAudit<TDbContext>(this FluentAspectOptions options) where TDbContext : DbContext
        {
            options.InterceptDbContextSave<TDbContext>()
                .With<AuditDbContextInterceptor>();
            return options;
        }

        #endregion FluentAspectOptions
    }
}
