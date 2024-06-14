using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework.Audit;

public sealed class AuditRecordsDbContext(DbContextOptions dbContextOptions)
    : DbContext(dbContextOptions)
{
    public DbSet<AuditRecord> AuditRecords { get; set; } = null!;
}

internal sealed class AuditRecordsDbContextStore(AuditRecordsDbContext dbContext) : IAuditStore
{
    public async Task Save(ICollection<AuditEntry> auditEntries)
    {
        if (auditEntries is not { Count: > 0 })
            return;

        foreach (var entry in auditEntries)
        {
            var record = entry.ToAuditRecord();
            dbContext.Add(record);
        }

        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.SaveChangesAsync();
    }
}
