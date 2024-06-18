using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework.Audit;

public sealed class AuditInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    private List<AuditEntry>? AuditEntries { get; set; }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        PreSaveChanges(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        PreSaveChanges(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PostSaveChanges().GetAwaiter().GetResult();
        var savedChanges = base.SavedChanges(eventData, result);
        return savedChanges;
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        await PostSaveChanges();
        var savedChanges = await base.SavedChangesAsync(eventData, result, cancellationToken);
        return savedChanges;
    }
    
    private void PreSaveChanges(DbContext dbContext)
    {
        if (!AuditConfig.Options.AuditEnabled)
            return;

        if (!serviceProvider.GetServices<IAuditStore>().Any())
            return;

        if (AuditEntries is null)
        {
            AuditEntries = new List<AuditEntry>();
        }
        else
        {
            AuditEntries.Clear();
        }

        foreach (var entityEntry in dbContext.ChangeTracker.Entries())
        {
            if (entityEntry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            if (AuditConfig.Options.EntityFilters.Any(entityFilter =>
                    entityFilter.Invoke(entityEntry) == false))
            {
                continue;
            }

            AuditEntries.Add(new InternalAuditEntry(entityEntry));
        }
    }

    private async Task PostSaveChanges()
    {
        if (AuditEntries is { Count: > 0 })
        {
            var now = DateTimeOffset.Now;

            var auditUserIdProvider = AuditConfig.Options.UserIdProviderFactory?.Invoke(serviceProvider);
            var auditUser =  auditUserIdProvider?.GetUserId();
            var enrichers = serviceProvider.GetServices<IAuditPropertyEnricher>().ToArray();

            foreach (var entry in AuditEntries)
            {
                // update TemporaryProperties
                if (entry is InternalAuditEntry { TemporaryProperties.Count: > 0 } auditEntry)
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
                    
                    // set TemporaryProperties to null
                    auditEntry.TemporaryProperties = null;
                }

                // apply enricher
                foreach (var enricher in enrichers)
                {
                    enricher.Enrich(entry);
                }
                
                entry.UpdatedBy = auditUser;
                entry.UpdatedAt = now;
            }

            await Task.WhenAll(
                    serviceProvider.GetServices<IAuditStore>()
                    .Select(store => store.Save(AuditEntries))
                );
        }
    }
}
