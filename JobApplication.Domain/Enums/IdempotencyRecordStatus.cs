using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Enums
{
    public enum IdempotencyRecordStatus
    {
        Processing,
        Completed,
        Failed
    }
}
