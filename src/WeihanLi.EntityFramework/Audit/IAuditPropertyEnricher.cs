using System;
using WeihanLi.Common.Helpers;

namespace WeihanLi.EntityFramework.Audit
{
    public interface IAuditPropertyEnricher : IEnricher<AuditEntry>
    {
    }

    public class AuditPropertyEnricher : PropertyEnricher<AuditEntry>, IAuditPropertyEnricher
    {
        public AuditPropertyEnricher(string propertyName, object propertyValue, bool overwrite = false) : base(propertyName, propertyValue, overwrite)
        {
        }

        public AuditPropertyEnricher(string propertyName, Func<AuditEntry, object> propertyValueFactory, bool overwrite = false) : base(propertyName, propertyValueFactory, overwrite)
        {
        }

        public AuditPropertyEnricher(string propertyName, Func<AuditEntry, object> propertyValueFactory, Func<AuditEntry, bool> propertyPredict, bool overwrite = false) : base(propertyName, propertyValueFactory, propertyPredict, overwrite)
        {
        }

        protected override Action<AuditEntry, string, Func<AuditEntry, object>, bool> EnrichAction =>
            (auditEntry, propertyName, valueFactory, overwrite) => auditEntry.WithProperty(propertyName, valueFactory, overwrite);
    }
}
