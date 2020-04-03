using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WeihanLi.EntityFramework.Audit;

// ReSharper disable once CheckNamespace
namespace WeihanLi.EntityFramework
{
    public class TestDbContext : AuditDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
    }

    [Table("tabTestEntities")]
    public class TestEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("PKID")]
        public int Id { get; set; }

        [Column("ExtraSettings")]
        public string Extra { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
