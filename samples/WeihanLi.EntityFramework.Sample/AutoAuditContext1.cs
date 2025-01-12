using Microsoft.EntityFrameworkCore;
using WeihanLi.EntityFramework.Audit;

namespace WeihanLi.EntityFramework.Sample;

public class AutoAuditContext1(DbContextOptions dbContextOptions, IServiceProvider serviceProvider)
    : AuditDbContext(dbContextOptions, serviceProvider)
{
    public DbSet<TestJobEntity> Jobs { get; set; } = null!;
}


public class AutoAuditContext2(DbContextOptions<AutoAuditContext2> options) : DbContext(options)
{
    public DbSet<TestJobEntity> Jobs { get; set; } = null!;
}

public class TestJobEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
