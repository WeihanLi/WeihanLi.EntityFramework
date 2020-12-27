using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WeihanLi.Common.Services;

namespace WeihanLi.EntityFramework.Audit
{
    public interface IAuditConfigBuilder
    {
        IAuditConfigBuilder WithUserIdProvider(IUserIdProvider auditUserProvider);

        IAuditConfigBuilder WithUnmodifiedProperty(bool saveUnModifiedProperty = true);

        IAuditConfigBuilder WithStore(IAuditStore auditStore);

        IAuditConfigBuilder WithEntityFilter(Func<EntityEntry, bool> entityFilter);

        IAuditConfigBuilder WithPropertyFilter(Func<EntityEntry, PropertyEntry, bool> propertyFilter);

        IAuditConfigBuilder WithEnricher(IAuditPropertyEnricher enricher);
    }

    internal sealed class AuditConfigBuilder : IAuditConfigBuilder
    {
        private IUserIdProvider _auditUserProvider = EnvironmentUserIdProvider.Instance.Value;
        private readonly List<IAuditPropertyEnricher> _auditPropertyEnrichers = new(4);
        private readonly List<Func<EntityEntry, bool>> _entityFilters = new();
        private readonly List<Func<EntityEntry, PropertyEntry, bool>> _propertyFilters = new();
        private readonly List<IAuditStore> _auditStores = new(4);
        private bool _saveUnModifiedProperty;

        public IAuditConfigBuilder WithUserIdProvider(IUserIdProvider auditUserProvider)
        {
            _auditUserProvider = auditUserProvider;
            return this;
        }

        public IAuditConfigBuilder WithUnmodifiedProperty(bool saveUnModifiedProperty = true)
        {
            _saveUnModifiedProperty = saveUnModifiedProperty;
            return this;
        }

        public IAuditConfigBuilder WithStore(IAuditStore auditStore)
        {
            if (null != auditStore)
            {
                _auditStores.Add(auditStore);
            }

            return this;
        }

        public IAuditConfigBuilder WithEntityFilter(Func<EntityEntry, bool> entityFilter)
        {
            if (null != entityFilter)
            {
                _entityFilters.Add(entityFilter);
            }
            return this;
        }

        public IAuditConfigBuilder WithPropertyFilter(Func<EntityEntry, PropertyEntry, bool> propertyFilter)
        {
            if (null != propertyFilter)
            {
                _propertyFilters.Add(propertyFilter);
            }
            return this;
        }

        public IAuditConfigBuilder WithEnricher(IAuditPropertyEnricher enricher)
        {
            if (null != enricher)
            {
                _auditPropertyEnrichers.Add(enricher);
            }
            return this;
        }

        public AuditConfigOptions Build()
        {
            return new()
            {
                Enrichers = _auditPropertyEnrichers,
                EntityFilters = _entityFilters,
                PropertyFilters = _propertyFilters,
                UserIdProvider = _auditUserProvider,
                Stores = _auditStores,
                SaveUnModifiedProperties = _saveUnModifiedProperty,
            };
        }
    }

    internal sealed class AuditConfigOptions
    {
        public bool AuditEnabled { get; set; } = true;

        public bool SaveUnModifiedProperties { get; set; }

        public IUserIdProvider UserIdProvider { get; set; }

        private IReadOnlyCollection<IAuditStore> _stores = Array.Empty<IAuditStore>();

        public IReadOnlyCollection<IAuditStore> Stores
        {
            get => _stores;
            set
            {
                if (value != null)
                    _stores = value;
            }
        }

        private IReadOnlyCollection<IAuditPropertyEnricher> _enrichers = Array.Empty<IAuditPropertyEnricher>();

        public IReadOnlyCollection<IAuditPropertyEnricher> Enrichers
        {
            get => _enrichers;
            set
            {
                if (value != null)
                    _enrichers = value;
            }
        }

        private IReadOnlyCollection<Func<EntityEntry, bool>> _entityFilters = Array.Empty<Func<EntityEntry, bool>>();

        public IReadOnlyCollection<Func<EntityEntry, bool>> EntityFilters
        {
            get => _entityFilters;
            set
            {
                if (value != null)
                    _entityFilters = value;
            }
        }

        private IReadOnlyCollection<Func<EntityEntry, PropertyEntry, bool>> _propertyFilters = Array.Empty<Func<EntityEntry, PropertyEntry, bool>>();

        public IReadOnlyCollection<Func<EntityEntry, PropertyEntry, bool>> PropertyFilters
        {
            get => _propertyFilters;
            set
            {
                if (value != null)
                    _propertyFilters = value;
            }
        }
    }

    public sealed class AuditConfig
    {
        internal static AuditConfigOptions AuditConfigOptions = new();

        public static void EnableAudit()
        {
            AuditConfigOptions.AuditEnabled = true;
        }

        public static void DisableAudit()
        {
            AuditConfigOptions.AuditEnabled = false;
        }

        public static void Configure(Action<IAuditConfigBuilder> configAction)
        {
            if (null == configAction)
                return;

            var builder = new AuditConfigBuilder();
            configAction.Invoke(builder);
            AuditConfigOptions = builder.Build();
        }
    }
}
