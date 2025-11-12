using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable once CheckNamespace
namespace WeihanLi.EntityFramework;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TestEntity>()
            // .HasQueryFilter("one-month-ago", t => t.CreatedAt > DateTime.Now.AddMonths(-1))
            .HasQueryFilter("valid-id", t => t.Id > 0)
            .HasQueryFilter("not-null", t => t.Extra != null)
            ;
    }

    public DbSet<TestEntity> TestEntities { get; set; } = null!;
}

[Table("tabTestEntities")]
public class TestEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("PKID")]
    public int Id { get; set; }

    [Column("ExtraSettings")]
    public string? Extra { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
