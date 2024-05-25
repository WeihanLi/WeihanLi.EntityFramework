using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework.Interceptors;

public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        OnSavingChanges(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        OnSavingChanges(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void OnSavingChanges(DbContextEventData eventData)
    {
        ArgumentNullException.ThrowIfNull(eventData.Context);
        eventData.Context.ChangeTracker.DetectChanges();
        foreach (var entityEntry in eventData.Context.ChangeTracker.Entries())
        {
            if (entityEntry is { State: EntityState.Deleted, Entity: ISoftDeleteEntityWithDeleted softDeleteEntity })
            {
                foreach (var property in entityEntry.Properties)
                {
                    property.IsModified = false;
                }
                softDeleteEntity.IsDeleted = true;
                entityEntry.State = EntityState.Modified;
                foreach (var property in entityEntry.Properties)
                {
                    property.IsModified = property.Metadata.Name == SoftDeleteEntitySavingHandler.DefaultIsDeletedPropertyName;
                }
            }
        }
    }
}
