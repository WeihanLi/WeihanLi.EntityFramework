using System;

namespace WeihanLi.EntityFramework.Audit
{
    public interface IAuditPropertyEnricher
    {
        void Enrich(AuditEntry auditEntry);
    }

    public class AuditPropertyEnricher : IAuditPropertyEnricher
    {
        private readonly string _propertyName;
        private readonly Func<AuditEntry, object> _propertyValueFactory;
        private readonly bool _overwrite;
        private readonly Func<AuditEntry, bool> _auditPropertyPredict = null;

        public AuditPropertyEnricher(string propertyName, object propertyValue, bool overwrite = false)
            : this(propertyName, (auditEntry) => propertyValue, overwrite)
        {
        }

        public AuditPropertyEnricher(string propertyName, Func<AuditEntry, object> propertyValueFactory, bool overwrite = false)
            : this(propertyName, propertyValueFactory, null, overwrite)
        {
        }

        public AuditPropertyEnricher(
            string propertyName,
            Func<AuditEntry, object> propertyValueFactory,
            Func<AuditEntry, bool> auditPropertyPredict,
            bool overwrite = false)
        {
            _propertyName = propertyName;
            _propertyValueFactory = propertyValueFactory;
            _auditPropertyPredict = auditPropertyPredict;
            _overwrite = overwrite;
        }

        public void Enrich(AuditEntry auditEntry)
        {
            if (_auditPropertyPredict?.Invoke(auditEntry) != false)
            {
                auditEntry.WithProperty(_propertyName, _propertyValueFactory, _overwrite);
            }
        }
    }
}
