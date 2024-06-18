using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework.Audit;

public abstract class AuditDbContextBase(DbContextOptions dbContextOptions, IServiceProvider serviceProvider)
    : DbContextBase(dbContextOptions)
{
    private readonly IAuditStore[] _auditStores = serviceProvider.GetServices<IAuditStore>().ToArray();
    private readonly IAuditPropertyEnricher[] _auditPropertyEnrichers =
        serviceProvider.GetServices<IAuditPropertyEnricher>().ToArray();
    private readonly string? _auditUser =
        AuditConfig.Options.UserIdProviderFactory?.Invoke(serviceProvider)?.GetUserId();

    protected List<AuditEntry>? AuditEntries { get; set; }

    protected override Task BeforeSaveChanges()
    {
        if (!AuditConfig.Options.AuditEnabled || _auditStores.Length <= 0) return Task.CompletedTask;

        AuditEntries = new List<AuditEntry>();
        foreach (var entityEntry in ChangeTracker.Entries())
        {
            if (entityEntry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }
            //
            if (AuditConfig.Options.EntityFilters.Any(entityFilter =>
                    entityFilter.Invoke(entityEntry) == false))
            {
                continue;
            }
            
            AuditEntries.Add(new InternalAuditEntry(entityEntry));
        }

        return Task.CompletedTask;
    }

    protected override async Task AfterSaveChanges()
    {
        if (AuditEntries is { Count: > 0 })
        {
            var now = DateTimeOffset.Now;
            
            foreach (var entry in AuditEntries)
            {
                if (entry is InternalAuditEntry { TemporaryProperties.Count: > 0 } auditEntry)
                // update TemporaryProperties
                {
                    foreach (var temporaryProperty in auditEntry.TemporaryProperties)
                    {
                        var colName = temporaryProperty.GetColumnName();
                        if (temporaryProperty.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[colName] = temporaryProperty.CurrentValue;
                        }

                        switch (auditEntry.OperationType)
                        {
                            case DataOperationType.Add:
                                auditEntry.NewValues![colName] = temporaryProperty.CurrentValue;
                                break;

                            case DataOperationType.Delete:
                                auditEntry.OriginalValues![colName] = temporaryProperty.OriginalValue;
                                break;

                            case DataOperationType.Update:
                                auditEntry.OriginalValues![colName] = temporaryProperty.OriginalValue;
                                auditEntry.NewValues![colName] = temporaryProperty.CurrentValue;
                                break;
                        }
                    }
                    // set to null
                    auditEntry.TemporaryProperties = null;
                }

                // apply enricher
                foreach (var enricher in _auditPropertyEnrichers)
                {
                    enricher.Enrich(entry);
                }

                entry.UpdatedAt = now;
                entry.UpdatedBy = _auditUser;
            }

            await Task.WhenAll(_auditStores.Select(store => store.Save(AuditEntries)));
        }
    }
}

public abstract class AuditDbContext(DbContextOptions dbContextOptions, IServiceProvider serviceProvider)
    : AuditDbContextBase(dbContextOptions, serviceProvider)
{
    public virtual DbSet<AuditRecord> AuditRecords { get; set; } = null!;

    protected override Task BeforeSaveChanges()
    {
        if (!AuditConfig.Options.AuditEnabled) return Task.CompletedTask;

        AuditEntries = new List<AuditEntry>();
        foreach (var entityEntry in ChangeTracker.Entries())
        {
            if (entityEntry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            if (entityEntry.Entity is AuditRecord)
            {
                continue;
            }

            //entityFilters
            if (AuditConfig.Options.EntityFilters.Any(entityFilter =>
                    entityFilter.Invoke(entityEntry) == false))
            {
                continue;
            }

            AuditEntries.Add(new InternalAuditEntry(entityEntry));
        }

        return Task.CompletedTask;
    }

    protected override async Task AfterSaveChanges()
    {
        if (AuditEntries is { Count: > 0 })
        {
            await base.AfterSaveChanges();
            AuditRecords.AddRange(AuditEntries.Select(a => a.ToAuditRecord()));
            await base.SaveChangesAsync();
        }
    }
}
