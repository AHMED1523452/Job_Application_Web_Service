using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Enums
{
    public enum JobApplicationStatus
    {
        Applied,
        UnderReview,
        InterviewScheduled,
        Accepted,
        Rejected,
        Cancelled,
        Closed
    }
}
