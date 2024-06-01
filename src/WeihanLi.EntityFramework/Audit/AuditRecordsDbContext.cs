using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework.Audit;

public sealed class AuditRecordsDbContext(DbContextOptions<AuditRecordsDbContext> options)
    : DbContext(options)
{
    public DbSet<AuditRecord> AuditRecords { get; set; } = null!;
}
