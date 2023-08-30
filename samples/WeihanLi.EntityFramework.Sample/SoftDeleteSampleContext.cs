using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Models;

namespace WeihanLi.EntityFramework.Sample;

public class SoftDeleteSampleContext : DbContext
{
    public SoftDeleteSampleContext(DbContextOptions<SoftDeleteSampleContext> options) : base(options)
    {
    }

    public virtual DbSet<SoftDeleteEntity> TestEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SoftDeleteEntity>().WithSoftDeleteFilter();
        base.OnModelCreating(modelBuilder);
    }
}

public class SoftDeleteEntity : ISoftDeleteEntityWithDeleted
{
    public int Id { get; set; }
    public string Name { get; set; } = "test";
    public bool IsDeleted { get; set; }
}
