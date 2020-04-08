using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WeihanLi.EntityFramework.Audit;

namespace WeihanLi.EntityFramework.Samples
{
    public class TestDbContext : AuditDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(() => DbFunctions.JsonValue(default, default));
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
