using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

            if (auditEntry.Extra.ContainsKey(propertyName) && overwrite == false)
            {
                return false;
            }

            auditEntry.Extra[propertyName] = propertyValue;
            return true;
        }

        public static bool WithProperty(this AuditEntry auditEntry, string propertyName,
            Func<AuditEntry, object> propertyValueFactory, bool overwrite = false)
        {
            if (null == auditEntry)
            {
                throw new ArgumentNullException(nameof(auditEntry));
            }

            if (auditEntry.Extra.ContainsKey(propertyName) && overwrite == false)
            {
                return false;
            }

            auditEntry.Extra[propertyName] = propertyValueFactory?.Invoke(auditEntry);
            return true;
        }

        #endregion AuditEntry

        #region IAuditConfigBuilder

        public static IAuditConfigBuilder WithEntityFilter(this IAuditConfigBuilder configBuilder, Func<EntityEntry, bool> filterFunc)
        {
            configBuilder.WithEntityFilter(filterFunc);
            return configBuilder;
        }

        public static IAuditConfigBuilder WithFilter(this IAuditConfigBuilder configBuilder, Func<PropertyEntry, bool> filterFunc)
        {
            configBuilder.WithPropertyFilter(filterFunc);
            return configBuilder;
        }

        public static IAuditConfigBuilder WithUserIdProvider<TUserIdProvider>(this IAuditConfigBuilder configBuilder) where TUserIdProvider : IAuditUserIdProvider, new()
        {
            configBuilder.WithUserIdProvider(new TUserIdProvider());
            return configBuilder;
        }

        public static IAuditConfigBuilder WithEnricher<TEnricher>(this IAuditConfigBuilder configBuilder) where TEnricher : IAuditPropertyEnricher, new()
        {
            configBuilder.WithEnricher(new TEnricher());
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
    }
}
