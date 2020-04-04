using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeihanLi.EntityFramework.Audit
{
    public interface IAuditStore
    {
        Task Save(ICollection<AuditEntry> auditEntries);
    }
}
