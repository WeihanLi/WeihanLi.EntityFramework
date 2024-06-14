using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WeihanLi.Common.Models;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework.Audit;

public class AuditEntry
{
    public string TableName { get; set; } = null!;

    public Dictionary<string, object?>? OriginalValues { get; set; }

    public Dictionary<string, object?>? NewValues { get; set; }

    public Dictionary<string, object?> KeyValues { get; } = new();

    public DataOperationType OperationType { get; set; }

    public Dictionary<string, object?> Properties { get; } = new();

    public DateTimeOffset UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    internal AuditRecord ToAuditRecord()
    {
        return new AuditRecord()
        {
            TableName = TableName,
            OperationType = OperationType,
            Extra = Properties.Count == 0 ? null : Properties.ToJson(),
            OriginValue = OriginalValues?.ToJson(),
            NewValue = NewValues?.ToJson(),
            ObjectId = KeyValues.ToJson(),
            UpdatedAt = UpdatedAt,
            UpdatedBy = UpdatedBy
        };
    }
}

internal sealed class InternalAuditEntry : AuditEntry
{
    public List<PropertyEntry>? TemporaryProperties { get; set; }

    public InternalAuditEntry(EntityEntry entityEntry)
    {
        TableName = entityEntry.Metadata.GetTableName() ?? entityEntry.Metadata.Name;

        if (entityEntry.Properties.Any(x => x.IsTemporary))
        {
            TemporaryProperties = new List<PropertyEntry>(4);
        }

        if (entityEntry.State == EntityState.Added)
        {
            OperationType = DataOperationType.Add;
            NewValues = new Dictionary<string, object?>();
        }
        else if (entityEntry.State == EntityState.Deleted)
        {
            OperationType = DataOperationType.Delete;
            OriginalValues = new Dictionary<string, object?>();
        }
        else if (entityEntry.State == EntityState.Modified)
        {
            OperationType = DataOperationType.Update;
            OriginalValues = new Dictionary<string, object?>();
            NewValues = new Dictionary<string, object?>();
        }
        foreach (var propertyEntry in entityEntry.Properties)
        {
            if (AuditConfig.Options.PropertyFilters.Any(f => f.Invoke(entityEntry, propertyEntry) == false))
            {
                continue;
            }

            if (propertyEntry.IsTemporary)
            {
                TemporaryProperties!.Add(propertyEntry);
                continue;
            }

            var columnName = propertyEntry.GetColumnName();
            if (propertyEntry.Metadata.IsPrimaryKey())
            {
                KeyValues[columnName] = propertyEntry.CurrentValue;
            }
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    NewValues![columnName] = propertyEntry.CurrentValue;
                    break;

                case EntityState.Deleted:
                    OriginalValues![columnName] = propertyEntry.OriginalValue;
                    break;

                case EntityState.Modified:
                    if (propertyEntry.IsModified || AuditConfig.Options.SaveUnModifiedProperties)
                    {
                        OriginalValues![columnName] = propertyEntry.OriginalValue;
                        NewValues![columnName] = propertyEntry.CurrentValue;
                    }
                    break;
            }
        }
    }
}
