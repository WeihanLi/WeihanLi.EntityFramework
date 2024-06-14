using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common;
using WeihanLi.Common.Services;

namespace WeihanLi.EntityFramework.Audit;

public interface IAuditConfigBuilder
{
    IServiceCollection Services { get; }

    IAuditConfigBuilder WithUserIdProvider(IUserIdProvider auditUserProvider) =>
        WithUserIdProvider(_ => auditUserProvider);

    IAuditConfigBuilder WithUserIdProvider(Func<IServiceProvider, IUserIdProvider> auditUserProviderFactory);

    IAuditConfigBuilder WithUnmodifiedProperty(bool saveUnModifiedProperty = true);

    IAuditConfigBuilder WithStore(IAuditStore auditStore);
    IAuditConfigBuilder WithStore<TStore>() where TStore : class, IAuditStore;

    IAuditConfigBuilder WithEntityFilter(Func<EntityEntry, bool> entityFilter);

    IAuditConfigBuilder WithPropertyFilter(Func<EntityEntry, PropertyEntry, bool> propertyFilter);

    IAuditConfigBuilder WithEnricher(IAuditPropertyEnricher enricher);
}

internal sealed class AuditConfigBuilder(IServiceCollection services) : IAuditConfigBuilder
{
    private Func<IServiceProvider, IUserIdProvider>? _auditUserProviderFactory =
        sp =>
        {
            var userIdProvider = sp.GetService<IUserIdProvider>();
            return userIdProvider ?? EnvironmentUserIdProvider.Instance.Value;
        };
    private readonly List<IAuditPropertyEnricher> _auditPropertyEnrichers = new();
    private readonly List<Func<EntityEntry, bool>> _entityFilters = new();
    private readonly List<Func<EntityEntry, PropertyEntry, bool>> _propertyFilters = new();
    private bool _saveUnModifiedProperty;

    public IServiceCollection Services => services;

    public IAuditConfigBuilder WithUserIdProvider(Func<IServiceProvider, IUserIdProvider>? auditUserProviderFactory)
    {
        _auditUserProviderFactory = auditUserProviderFactory;
        return this;
    }

    public IAuditConfigBuilder WithUnmodifiedProperty(bool saveUnModifiedProperty = true)
    {
        _saveUnModifiedProperty = saveUnModifiedProperty;
        return this;
    }

    public IAuditConfigBuilder WithStore(IAuditStore auditStore)
    {
        ArgumentNullException.ThrowIfNull(auditStore);

        services.AddSingleton(auditStore);
        return this;
    }

    public IAuditConfigBuilder WithStore<TStore>() where TStore : class, IAuditStore
    {
        services.AddScoped<IAuditStore, TStore>();
        return this;
    }

    public IAuditConfigBuilder WithEntityFilter(Func<EntityEntry, bool> entityFilter)
    {
        ArgumentNullException.ThrowIfNull(entityFilter);
        _entityFilters.Add(entityFilter);
        return this;
    }

    public IAuditConfigBuilder WithPropertyFilter(Func<EntityEntry, PropertyEntry, bool> propertyFilter)
    {
        ArgumentNullException.ThrowIfNull(propertyFilter);
        _propertyFilters.Add(propertyFilter);
        return this;
    }

    public IAuditConfigBuilder WithEnricher(IAuditPropertyEnricher enricher)
    {
        ArgumentNullException.ThrowIfNull(enricher);
        _auditPropertyEnrichers.Add(enricher);
        return this;
    }

    public AuditConfigOptions Build()
    {
        return new()
        {
            Enrichers = _auditPropertyEnrichers,
            EntityFilters = _entityFilters,
            PropertyFilters = _propertyFilters,
            UserIdProviderFactory = _auditUserProviderFactory,
            SaveUnModifiedProperties = _saveUnModifiedProperty,
        };
    }
}

internal sealed class AuditConfigOptions
{
    public bool AuditEnabled { get; set; } = true;

    public bool SaveUnModifiedProperties { get; set; }

    public Func<IServiceProvider, IUserIdProvider>? UserIdProviderFactory { get; set; }

    private IReadOnlyCollection<IAuditPropertyEnricher> _enrichers = Array.Empty<IAuditPropertyEnricher>();

    public IReadOnlyCollection<IAuditPropertyEnricher> Enrichers
    {
        get => _enrichers;
        set => _enrichers = Guard.NotNull(value);
    }

    private IReadOnlyCollection<Func<EntityEntry, bool>> _entityFilters = Array.Empty<Func<EntityEntry, bool>>();

    public IReadOnlyCollection<Func<EntityEntry, bool>> EntityFilters
    {
        get => _entityFilters;
        set => _entityFilters = Guard.NotNull(value);
    }

    private IReadOnlyCollection<Func<EntityEntry, PropertyEntry, bool>> _propertyFilters = Array.Empty<Func<EntityEntry, PropertyEntry, bool>>();

    public IReadOnlyCollection<Func<EntityEntry, PropertyEntry, bool>> PropertyFilters
    {
        get => _propertyFilters;
        set => _propertyFilters = Guard.NotNull(value);
    }
}

public static class AuditConfig
{
    internal static AuditConfigOptions Options = new();

    public static void EnableAudit()
    {
        Options.AuditEnabled = true;
    }

    public static void DisableAudit()
    {
        Options.AuditEnabled = false;
    }

#nullable disable

    public static void Configure(IServiceCollection services, Action<IAuditConfigBuilder> configAction)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (configAction is null)
            return;
#nullable restore

        var builder = new AuditConfigBuilder(services);
        configAction.Invoke(builder);
        Options = builder.Build();
    }

}
