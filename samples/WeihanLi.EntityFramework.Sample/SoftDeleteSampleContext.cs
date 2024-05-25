using Microsoft.EntityFrameworkCore;
using System;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework.Sample;

public class SoftDeleteSampleContext : DbContext
{
    public SoftDeleteSampleContext(DbContextOptions<SoftDeleteSampleContext> options) : base(options)
    {
    }

    public virtual DbSet<SoftDeleteEntity> TestEntities { get; set; } = null!;

    public virtual DbSet<SoftDeleteEntity2> TestEntities2 { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SoftDeleteEntity>().HasQueryFilter(x => x.IsDeleted == false);

        modelBuilder.Entity<SoftDeleteEntity2>().Property<bool>("IsDeleted");
        modelBuilder.Entity<SoftDeleteEntity2>().HasQueryFilter(x => EF.Property<bool>(x, "IsDeleted") == false);

        base.OnModelCreating(modelBuilder);
    }
}

public class SoftDeleteEntity : ISoftDeleteEntityWithDeleted
{
    public int Id { get; set; }
    public string Name { get; set; } = "test";
    public bool IsDeleted { get; set; }
}

public class SoftDeleteEntity2 : ISoftDeleteEntity, IEntityWithCreatedUpdatedAt
{
    public int Id { get; set; }
    public string Name { get; set; } = "test";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
