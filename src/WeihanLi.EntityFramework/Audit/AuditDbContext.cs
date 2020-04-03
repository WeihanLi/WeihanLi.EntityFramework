using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Audit
{
    public class AuditDbContext : DbContextBase
    {
        protected AuditDbContext()
        {
        }

        protected AuditDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<AuditRecord> AuditRecords { get; set; }

        private List<AuditEntry> AuditEntries { get; set; }

        protected override Task BeforeSaveChanges()
        {
            AuditEntries = new List<AuditEntry>();
            foreach (var entityEntry in ChangeTracker.Entries())
            {
                if (entityEntry.State == EntityState.Detached || entityEntry.State == EntityState.Unchanged)
                {
                    continue;
                }
                if (entityEntry.Entity.GetType() == typeof(AuditRecord))
                {
                    continue;
                }
                //
                if (AuditConfig.AuditConfigOptions.EntityFilters.Any(entityFilter =>
                        entityFilter.Invoke(entityEntry) == false))
                {
                    continue;
                }
                AuditEntries.Add(new AuditEntry(entityEntry));
            }

            return Task.CompletedTask;
        }

        protected override async Task AfterSaveChanges()
        {
            if (null != AuditEntries && AuditEntries.Count > 0)
            {
                foreach (var auditEntry in AuditEntries)
                {
                    // update TemporaryProperties
                    if (auditEntry.TemporaryProperties != null && auditEntry.TemporaryProperties.Count > 0)
                    {
                        foreach (var temporaryProperty in auditEntry.TemporaryProperties)
                        {
                            var colName = temporaryProperty.Metadata.GetColumnName();
                            if (temporaryProperty.Metadata.IsPrimaryKey())
                            {
                                auditEntry.KeyValues[colName] = temporaryProperty.CurrentValue;
                            }

                            switch (auditEntry.OperationType)
                            {
                                case OperationType.Add:
                                    auditEntry.NewValues[colName] = temporaryProperty.CurrentValue;
                                    break;

                                case OperationType.Delete:
                                    auditEntry.OriginalValues[colName] = temporaryProperty.OriginalValue;
                                    break;

                                case OperationType.Update:
                                    auditEntry.OriginalValues[colName] = temporaryProperty.OriginalValue;
                                    auditEntry.NewValues[colName] = temporaryProperty.CurrentValue;
                                    break;
                            }
                        }
                        // set to null
                        auditEntry.TemporaryProperties = null;
                    }

                    // apply enricher
                    foreach (var enricher in AuditConfig.AuditConfigOptions.Enrichers)
                    {
                        enricher.Enrich(auditEntry);
                    }

                    // to record
                    var auditRecord = new AuditRecord()
                    {
                        OperationType = auditEntry.OperationType,
                        TableName = auditEntry.TableName,
                        ObjectId = auditEntry.KeyValues.ToJson(),
                        OriginValue = auditEntry.OriginalValues?.ToJson(),
                        NewValue = auditEntry.NewValues?.ToJson(),
                        Details = auditEntry.Extra.Count == 0 ? null : auditEntry.Extra.ToJson(),
                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = AuditConfig.AuditConfigOptions.UserIdProvider?.GetUserId(),
                    };
                    AuditRecords.Add(auditRecord);
                }

                await base.SaveChangesAsync();
            }
        }
    }
}
