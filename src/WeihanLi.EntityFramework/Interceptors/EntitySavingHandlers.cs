using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using WeihanLi.Common.Models;
using WeihanLi.Common.Services;

namespace WeihanLi.EntityFramework.Interceptors;

/// <summary>
/// Handle soft delete
/// </summary>
public sealed class SoftDeleteEntitySavingHandler : IEntitySavingHandler
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

/// <summary>
/// Auto update CreateAt/UpdatedAt
/// </summary>
public sealed class UpdatedAtEntityFieldSavingHandler : IEntitySavingHandler
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

        if (entityEntry.Entity is not IEntityWithCreatedUpdatedAt updatedAtEntity)
        {
            return;
        }

        if (entityEntry.State is EntityState.Added)
        {
            updatedAtEntity.CreatedAt = DateTimeOffset.Now;
        }

        updatedAtEntity.UpdatedAt = DateTimeOffset.Now;
    }
}

/// <summary>
/// Auto update CreateBy/UpdatedBy
/// </summary>
public sealed class UpdatedByEntityFieldSavingHandler(IUserIdProvider userIdProvider) : IEntitySavingHandler
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

        if (entityEntry.Entity is not IEntityWithCreatedUpdatedBy updatedByEntity)
        {
            return;
        }

        var userId = userIdProvider.GetUserId() ?? DefaultUserId;

        if (entityEntry.State is EntityState.Added)
        {
            updatedByEntity.CreatedBy = userId;
        }

        updatedByEntity.UpdatedBy = userId;
    }
}
