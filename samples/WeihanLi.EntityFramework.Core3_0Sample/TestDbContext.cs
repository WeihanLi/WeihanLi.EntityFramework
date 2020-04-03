using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeihanLi.EntityFramework.Models;
using WeihanLi.Extensions;

// ReSharper disable once CheckNamespace
namespace WeihanLi.EntityFramework
{
    public class TestDbContext : DbContextBase
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }

        protected override Task BeforeSaveChanges()
        {
            var updates = new List<UpdateRecord>();
            foreach (var entityEntry in ChangeTracker.Entries())
            {
                if (entityEntry.State == EntityState.Detached || entityEntry.State == EntityState.Unchanged)
                {
                    continue;
                }

                if (entityEntry.State == EntityState.Added)
                {
                    updates.Add(new UpdateRecord()
                    {
                        Details = entityEntry.Entity.ToJson(),
                        TableName = entityEntry.Metadata.GetTableName(),
                        OperationType = OperationType.Add,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = "",
                        ObjectId = this.GetKeyValues(entityEntry).ToJson()
                    });
                    // Console.WriteLine($"new entity added, entityType:{entityEntry.Entity.GetType()}, tableName:{entityEntry.Metadata.GetTableName()}, entity:{entityEntry.Entity.ToJson()}");
                }
                else if (entityEntry.State == EntityState.Deleted)
                {
                    updates.Add(new UpdateRecord()
                    {
                        TableName = entityEntry.Metadata.GetTableName(),
                        OperationType = OperationType.Delete,

                        ObjectId = this.GetKeyValues(entityEntry).ToJson(),
                        Details = entityEntry.Entity?.ToJson(),

                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = "",
                    });
                    // Console.WriteLine($"entity({entityEntry.Entity}) deleted, entityType:{entityEntry.Entity?.GetType()}, tableName:{entityEntry.Metadata.GetTableName()}, entity key:{this.GetKeyValues(entityEntry.Entity).ToJson()}");
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    var changedProperties = entityEntry.Properties
                        .Where(propertyEntry => propertyEntry.IsModified)
                        .Select(x => new
                        {
                            ColumnName = x.Metadata.GetColumnName(),
                            Before = x.OriginalValue.ToJsonOrString(),
                            After = x.CurrentValue.ToJsonOrString()
                        })
                        .ToArray();

                    // Console.WriteLine($"entity({entityEntry.Entity}) updated, entityType:{entityEntry.Entity?.GetType()}, tableName:{entityEntry.Metadata.GetTableName()}, entity key:{this.GetKeyValues(entityEntry.Entity).ToJson()}");
                    // Console.WriteLine(changedProperties.Select(x => x.ToJson()).StringJoin(Environment.NewLine));

                    updates.Add(new UpdateRecord()
                    {
                        TableName = entityEntry.Metadata.GetTableName(),
                        OperationType = OperationType.Update,

                        ObjectId = this.GetKeyValues(entityEntry).ToJson(),
                        Details = changedProperties.ToJson(),

                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = "",
                    });
                }
            }

            if (updates.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"updates:{Environment.NewLine}----------------------{Environment.NewLine}{updates.ToJson()}");
                Console.WriteLine();
            }

            return Task.CompletedTask;
        }
    }

    public class TestEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Extra { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
