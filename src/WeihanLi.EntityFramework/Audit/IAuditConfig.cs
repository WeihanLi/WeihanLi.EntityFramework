using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace WeihanLi.EntityFramework.Audit
{
    public interface IAuditConfigBuilder
    {
        IAuditConfigBuilder WithUserIdProvider(IAuditUserIdProvider auditUserProvider);

        IAuditConfigBuilder WithUnModifiedProperty(bool saveUnModifiedProperty = true);

        IAuditConfigBuilder WithEntityFilter(Func<EntityEntry, bool> entityFilter);

        IAuditConfigBuilder WithPropertyFilter(Func<PropertyEntry, bool> propertyFilter);

        IAuditConfigBuilder WithEnricher(IAuditPropertyEnricher enricher);
    }

    internal class AuditConfigBuilder : IAuditConfigBuilder
    {
        private IAuditUserIdProvider _auditUserProvider = EnvironmentAuditUserIdProvider.Instance.Value;
        private readonly List<IAuditPropertyEnricher> _auditPropertyEnrichers = new List<IAuditPropertyEnricher>(8);
        private readonly List<Func<EntityEntry, bool>> _entityFilters = new List<Func<EntityEntry, bool>>();
        private readonly List<Func<PropertyEntry, bool>> _propertyFilters = new List<Func<PropertyEntry, bool>>();

        private bool _saveUnModifiedProperty;

        public IAuditConfigBuilder WithUserIdProvider(IAuditUserIdProvider auditUserProvider)
        {
            _auditUserProvider = auditUserProvider;
            return this;
        }

        public IAuditConfigBuilder WithUnModifiedProperty(bool saveUnModifiedProperty = true)
        {
            _saveUnModifiedProperty = saveUnModifiedProperty;
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

        public IAuditConfigBuilder WithPropertyFilter(Func<PropertyEntry, bool> propertyFilter)
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
            return new AuditConfigOptions()
            {
                Enrichers = _auditPropertyEnrichers,
                EntityFilters = _entityFilters,
                PropertyFilters = _propertyFilters,
                UserProvider = _auditUserProvider,
                SaveUnModifiedProperties = _saveUnModifiedProperty,
            };
        }
    }

    internal class AuditConfigOptions
    {
        public bool SaveUnModifiedProperties { get; set; }

        private IReadOnlyCollection<IAuditPropertyEnricher> _enrichers = Array.Empty<IAuditPropertyEnricher>();
        private IReadOnlyCollection<Func<EntityEntry, bool>> _entityFilters = Array.Empty<Func<EntityEntry, bool>>();
        private IReadOnlyCollection<Func<PropertyEntry, bool>> _propertyFilters = Array.Empty<Func<PropertyEntry, bool>>();

        public IAuditUserIdProvider UserProvider { get; set; }

        public IReadOnlyCollection<IAuditPropertyEnricher> Enrichers
        {
            get => _enrichers;
            set
            {
                if (value != null)
                    _enrichers = value;
            }
        }

        public IReadOnlyCollection<Func<EntityEntry, bool>> EntityFilters
        {
            get => _entityFilters;
            set
            {
                if (value != null)
                    _entityFilters = value;
            }
        }

        public IReadOnlyCollection<Func<PropertyEntry, bool>> PropertyFilters
        {
            get => _propertyFilters;
            set
            {
                if (value != null)
                    _propertyFilters = value;
            }
        }
    }

    public class AuditConfig
    {
        internal static AuditConfigOptions AuditConfigOptions = new AuditConfigOptions();

        public static void Configure(Action<IAuditConfigBuilder> configAction)
        {
            if (null == configAction) return;
            var builder = new AuditConfigBuilder();
            configAction.Invoke(builder);
            AuditConfigOptions = builder.Build();
        }
    }
}
