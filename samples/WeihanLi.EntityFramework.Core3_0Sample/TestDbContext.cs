using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            foreach (var entityEntry in ChangeTracker.Entries())
            {
                if (entityEntry.State == EntityState.Detached || entityEntry.State == EntityState.Unchanged)
                {
                    continue;
                }

                if (entityEntry.State == EntityState.Added)
                {
                    Console.WriteLine($"new entity added, entityType:{entityEntry.Entity.GetType()}, entity:{entityEntry.Entity.ToJson()}");
                }
                else if (entityEntry.State == EntityState.Deleted)
                {
                    Console.WriteLine($"entity({entityEntry.Entity}) deleted, entityType:{entityEntry.Entity?.GetType()}, entity key:{this.GetKeyValues(entityEntry.Entity).ToJson()}");
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    Console.WriteLine($"entity({entityEntry.Entity}) updated, entityType:{entityEntry.Entity?.GetType()}, entity key:{this.GetKeyValues(entityEntry.Entity).ToJson()}");
                    var changedProperties = entityEntry.Properties.Where(propertyEntry => propertyEntry.IsModified)
                        .Select(x => new
                        {
                            Name = x.Metadata.Name,
                            ColumnName = x.Metadata.GetColumnName(),
                            Before = x.OriginalValue.ToJsonOrString(),
                            After = x.CurrentValue.ToJsonOrString()
                        })
                        .ToArray();
                    Console.WriteLine(changedProperties.Select(x => x.ToJson()).StringJoin(Environment.NewLine));
                }
                // ... auto logging update
                //var originValue = entityEntry.OriginalValues.ToJson();
                //var currentValue = entityEntry.CurrentValues.ToJson();
                //Console.WriteLine($"originValue: {originValue}, currentValue: {currentValue}");
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

        public DateTime CreatedAt { get; set; }
    }
}
