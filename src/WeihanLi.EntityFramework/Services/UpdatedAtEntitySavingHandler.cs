using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework.Services;

/// <summary>
/// Auto update CreateAt/UpdatedAt
/// </summary>
internal sealed class UpdatedAtEntitySavingHandler : IEntitySavingHandler
{
    public void Handle(EntityEntry entityEntry)
    {
        if (entityEntry is not
            {
                State: EntityState.Added or EntityState.Modified
            })
        {
            return;
        }

        if (entityEntry.Entity is not IEntityWithUpdatedAt updatedAtEntity)
        {
            return;
        }

        if (entityEntry.State is EntityState.Added && entityEntry.Entity is IEntityWithCreatedUpdatedAt createdUpdatedAtEntity)
        {
            createdUpdatedAtEntity.CreatedAt = DateTimeOffset.Now;
        }

        updatedAtEntity.UpdatedAt = DateTimeOffset.Now;
    }
}
