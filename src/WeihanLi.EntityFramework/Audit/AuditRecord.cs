using System;
using System.ComponentModel.DataAnnotations;

namespace WeihanLi.EntityFramework.Audit
{
    public class AuditRecord
    {
        public long Id { get; set; }

        [Required]
        [StringLength(128)]
        public string TableName { get; set; }

        public OperationType OperationType { get; set; }

        [StringLength(256)]
        public string ObjectId { get; set; }

        public string OriginValue { get; set; }

        public string NewValue { get; set; }

        public string Extra { get; set; }

        [StringLength(128)]
        public string UpdatedBy { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
