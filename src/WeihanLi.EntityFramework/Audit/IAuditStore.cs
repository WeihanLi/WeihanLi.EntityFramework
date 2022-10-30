using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers.PeriodBatching;

namespace WeihanLi.EntityFramework.Audit;

public interface IAuditStore
{
    Task Save(ICollection<AuditEntry> auditEntries);
}

public class PeriodBatchingAuditStore : PeriodicBatching<AuditEntry>, IAuditStore
{
    public PeriodBatchingAuditStore(int batchSizeLimit, TimeSpan period) : base(batchSizeLimit, period)
    {
    }

    public PeriodBatchingAuditStore(int batchSizeLimit, TimeSpan period, int queueLimit) : base(batchSizeLimit, period, queueLimit)
    {
    }

    public Task Save(ICollection<AuditEntry> auditEntries)
    {
        foreach (var entry in auditEntries)
        {
            Emit(entry);
        }
        return Task.CompletedTask;
    }
}
