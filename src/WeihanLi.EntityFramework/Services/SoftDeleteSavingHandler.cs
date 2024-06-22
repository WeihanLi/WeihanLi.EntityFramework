using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework.Services;

/// <summary>
/// Handle soft delete
/// </summary>
internal sealed class SoftDeleteEntitySavingHandler : IEntitySavingHandler
{
    public const string DefaultIsDeletedPropertyName = "IsDeleted";
    public void Handle(EntityEntry entityEntry)
    {
        if (entityEntry is not { State: EntityState.Deleted, Entity: ISoftDeleteEntity })
            return;

        if (entityEntry.Entity is ISoftDeleteEntityWithDeleted softDeleteEntityWithDeleted)
        {
            softDeleteEntityWithDeleted.IsDeleted = true;
        }
        else
        {
            var prop = entityEntry.Property(DefaultIsDeletedPropertyName);
            prop.CurrentValue = true;
        }
        entityEntry.State = EntityState.Modified;
        foreach (var property in entityEntry.Properties)
        {
            property.IsModified = property.Metadata.Name == DefaultIsDeletedPropertyName;
        }
    }
}
