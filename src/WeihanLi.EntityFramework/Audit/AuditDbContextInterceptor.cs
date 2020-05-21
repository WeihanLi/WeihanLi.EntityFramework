using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Aspect;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework.Audit
{
    public sealed class AuditDbContextInterceptor : IInterceptor
    {
        public async Task Invoke(IInvocation invocation, Func<Task> next)
        {
            if (invocation.Target is DbContext dbContext && AuditConfig.AuditConfigOptions.AuditEnabled)
            {
                var auditEntries = new List<AuditEntry>();
                foreach (var entityEntry in dbContext.ChangeTracker.Entries())
                {
                    if (entityEntry.State == EntityState.Detached || entityEntry.State == EntityState.Unchanged)
                    {
                        continue;
                    }
                    if (AuditConfig.AuditConfigOptions.EntityFilters
                        .Any(entityFilter =>
                            entityFilter.Invoke(entityEntry) == false))
                    {
                        continue;
                    }
                    auditEntries.Add(new InternalAuditEntry(entityEntry));
                }
                await next();
                //
                if (auditEntries.Count > 0)
                {
                    foreach (var auditEntry in auditEntries)
                    {
                        if (auditEntry is InternalAuditEntry internalAuditEntry)
                        {
                            // update TemporaryProperties
                            if (internalAuditEntry.TemporaryProperties != null && internalAuditEntry.TemporaryProperties.Count > 0)
                            {
                                foreach (var temporaryProperty in internalAuditEntry.TemporaryProperties)
                                {
                                    var colName = temporaryProperty.Metadata.GetColumnName();
                                    if (temporaryProperty.Metadata.IsPrimaryKey())
                                    {
                                        auditEntry.KeyValues[colName] = temporaryProperty.CurrentValue;
                                    }

                                    switch (auditEntry.OperationType)
                                    {
                                        case DataOperationType.Add:
                                            auditEntry.NewValues[colName] = temporaryProperty.CurrentValue;
                                            break;

                                        case DataOperationType.Delete:
                                            auditEntry.OriginalValues[colName] = temporaryProperty.OriginalValue;
                                            break;

                                        case DataOperationType.Update:
                                            auditEntry.OriginalValues[colName] = temporaryProperty.OriginalValue;
                                            auditEntry.NewValues[colName] = temporaryProperty.CurrentValue;
                                            break;
                                    }
                                }
                                // set to null
                                internalAuditEntry.TemporaryProperties = null;
                            }
                        }

                        // apply enricher
                        foreach (var enricher in AuditConfig.AuditConfigOptions.Enrichers)
                        {
                            enricher.Enrich(auditEntry);
                        }

                        auditEntry.UpdatedAt = DateTimeOffset.UtcNow;
                        auditEntry.UpdatedBy = AuditConfig.AuditConfigOptions.UserIdProvider
                            ?.GetUserId();
                    }

                    await Task.WhenAll(
                            AuditConfig.AuditConfigOptions.Stores
                            .Select(store => store.Save(auditEntries))
                        );
                }
            }
            else
            {
                await next();
            }
        }
    }
}
