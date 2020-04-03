using System;

namespace WeihanLi.EntityFramework.Models
{
    public class UpdateRecord
    {
        public string TableName { get; set; }

        public OperationType OperationType { get; set; }

        public string ObjectId { get; set; }

        public string Details { get; set; }

        public string UpdatedBy { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    public enum OperationType : sbyte
    {
        Update = 0,

        Add = 1,

        Delete = 2,
    }
}
