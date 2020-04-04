using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace WeihanLi.EntityFramework.Audit
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entityEntry)
        {
            TableName = entityEntry.Metadata.GetTableName();
            KeyValues = new Dictionary<string, object>(4);
            Properties = new Dictionary<string, object>(16);
            if (entityEntry.Properties.Any(x => x.IsTemporary))
            {
                TemporaryProperties = new List<PropertyEntry>(4);
            }

            if (entityEntry.State == EntityState.Added)
            {
                OperationType = OperationType.Add;
                NewValues = new Dictionary<string, object>();
            }
            else if (entityEntry.State == EntityState.Deleted)
            {
                OperationType = OperationType.Delete;
                OriginalValues = new Dictionary<string, object>();
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                OperationType = OperationType.Update;
                OriginalValues = new Dictionary<string, object>();
                NewValues = new Dictionary<string, object>();
            }
            foreach (var propertyEntry in entityEntry.Properties)
            {
                if (AuditConfig.AuditConfigOptions.PropertyFilters.Any(f => f.Invoke(entityEntry, propertyEntry) == false))
                {
                    continue;
                }

                if (propertyEntry.IsTemporary)
                {
                    TemporaryProperties.Add(propertyEntry);
                    continue;
                }

                var columnName = propertyEntry.Metadata.GetColumnName();
                if (propertyEntry.Metadata.IsPrimaryKey())
                {
                    KeyValues[columnName] = propertyEntry.CurrentValue;
                }
                switch (entityEntry.State)
                {
                    case EntityState.Added:
                        NewValues[columnName] = propertyEntry.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        OriginalValues[columnName] = propertyEntry.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (propertyEntry.IsModified || AuditConfig.AuditConfigOptions.SaveUnModifiedProperties)
                        {
                            OriginalValues[columnName] = propertyEntry.OriginalValue;
                            NewValues[columnName] = propertyEntry.CurrentValue;
                        }
                        break;
                }
            }
        }

        public string TableName { get; }

        public Dictionary<string, object> OriginalValues { get; }

        public Dictionary<string, object> NewValues { get; }

        public Dictionary<string, object> KeyValues { get; }

        public OperationType OperationType { get; }

        public Dictionary<string, object> Properties { get; }

        internal List<PropertyEntry> TemporaryProperties { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public string UpdatedBy { get; set; }
    }
}
