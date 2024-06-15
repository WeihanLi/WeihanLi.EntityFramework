using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WeihanLi.Common.Models;
using WeihanLi.Common.Services;

namespace WeihanLi.EntityFramework.Services;

/// <summary>
/// Auto update CreateBy/UpdatedBy
/// </summary>
internal sealed class UpdatedBySavingHandler(IUserIdProvider userIdProvider) : IEntitySavingHandler
{
    private static readonly string DefaultUserId = $"{Environment.UserName}@{Environment.MachineName}";
    public void Handle(EntityEntry entityEntry)
    {
        if (entityEntry is not
            {
                State: EntityState.Added or EntityState.Modified
            })
        {
            return;
        }

        if (entityEntry.Entity is not IEntityWithUpdatedBy updatedByEntity)
        {
            return;
        }

        var userId = userIdProvider.GetUserId() ?? DefaultUserId;

        if (entityEntry.State is EntityState.Added && entityEntry.Entity is IEntityWithCreatedUpdatedBy createdUpdatedBy)
        {
            createdUpdatedBy.CreatedBy = userId;
        }

        updatedByEntity.UpdatedBy = userId;
    }
}
