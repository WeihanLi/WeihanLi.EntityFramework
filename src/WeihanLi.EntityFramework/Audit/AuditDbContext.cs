using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Models;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Audit;

public abstract class AuditDbContextBase : DbContextBase
{
    protected AuditDbContextBase()
    {
    }

    protected AuditDbContextBase(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    protected List<AuditEntry>? AuditEntries { get; set; }

    protected override Task BeforeSaveChanges()
    {
        if (AuditConfig.AuditConfigOptions.AuditEnabled && AuditConfig.AuditConfigOptions.Stores.Count > 0)
        {
            AuditEntries = new List<AuditEntry>();
            foreach (var entityEntry in ChangeTracker.Entries())
            {
                if (entityEntry.State == EntityState.Detached || entityEntry.State == EntityState.Unchanged)
                {
                    continue;
                }
                //
                if (AuditConfig.AuditConfigOptions.EntityFilters.Any(entityFilter =>
                    entityFilter.Invoke(entityEntry) == false))
                {
                    continue;
                }
                AuditEntries.Add(new InternalAuditEntry(entityEntry));
            }
        }

        return Task.CompletedTask;
    }

    protected override async Task AfterSaveChanges()
    {
        if (AuditEntries is { Count: > 0 })
        {
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
                foreach (var enricher in AuditConfig.AuditConfigOptions.Enrichers)
                {
                    enricher.Enrich(entry);
                }

                entry.UpdatedAt = DateTimeOffset.Now;
                entry.UpdatedBy = AuditConfig.AuditConfigOptions.UserIdProvider
                    ?.GetUserId();
            }

            await Task.WhenAll(
                    AuditConfig.AuditConfigOptions.Stores
                    .Select(store => store.Save(AuditEntries))
                );
        }
    }
}

public abstract class AuditDbContext : AuditDbContextBase
{
    protected AuditDbContext()
    {
    }

    protected AuditDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    public virtual DbSet<AuditRecord> AuditRecords { get; set; } = null!;

    protected override Task BeforeSaveChanges()
    {
        if (!AuditConfig.AuditConfigOptions.AuditEnabled) return Task.CompletedTask;
        
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
            if (AuditConfig.AuditConfigOptions.EntityFilters.Any(entityFilter =>
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
        if (AuditEntries?.Count > 0)
        {
            await base.AfterSaveChanges();
            AuditRecords.AddRange(AuditEntries.Select(a => new AuditRecord()
            {
                TableName = a.TableName,
                OperationType = a.OperationType,
                Extra = a.Properties.Count == 0 ? null : a.Properties.ToJson(),
                OriginValue = a.OriginalValues?.ToJson(),
                NewValue = a.NewValues?.ToJson(),
                ObjectId = a.KeyValues.ToJson(),
                UpdatedAt = a.UpdatedAt,
                UpdatedBy = a.UpdatedBy,
            }));
            await base.SaveChangesAsync();
        }
    }
}
