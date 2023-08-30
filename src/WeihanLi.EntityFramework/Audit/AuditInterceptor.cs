using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework.Audit;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private List<AuditEntry>? AuditEntries { get; set; }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        PreSaveChanges(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        PreSaveChanges(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void PreSaveChanges(DbContext dbContext)
    {
        if (AuditConfig.AuditConfigOptions.AuditEnabled && AuditConfig.AuditConfigOptions.Stores.Count > 0)
        {
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
                if (entityEntry.State == EntityState.Detached || entityEntry.State == EntityState.Unchanged)
                {
                    continue;
                }
                if (AuditConfig.AuditConfigOptions.EntityFilters.Any(entityFilter =>
                    entityFilter.Invoke(entityEntry) == false))
                {
                    continue;
                }
                AuditEntries.Add(new InternalAuditEntry(entityEntry));
            }
        }
    }

    private async Task PostSaveChanges()
    {
        if (AuditEntries is { Count: > 0 })
        {
            foreach (var entry in AuditEntries)
            {
                if (entry is InternalAuditEntry auditEntry)
                {
                    // update TemporaryProperties
                    if (auditEntry.TemporaryProperties is { Count: > 0 })
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
                }

                // apply enricher
                foreach (var enricher in AuditConfig.AuditConfigOptions.Enrichers)
                {
                    enricher.Enrich(entry);
                }

                entry.UpdatedAt = DateTimeOffset.UtcNow;
                entry.UpdatedBy = AuditConfig.AuditConfigOptions.UserIdProvider
                    ?.GetUserId();
            }

            await Task.WhenAll(
                    AuditConfig.AuditConfigOptions.Stores
                    .Select(store => store.Save(AuditEntries))
                );
        }
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PostSaveChanges().GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        await PostSaveChanges();
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
