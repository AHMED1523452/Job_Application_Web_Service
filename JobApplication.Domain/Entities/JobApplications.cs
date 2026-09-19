using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class JobApplications : baseClass
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public JobApplicationStatus status { get; set; }
        public Guid JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Jobs? Job { get; set; }
        public Guid CandidateId { get; set; }
        [ForeignKey(nameof(CandidateId))]
        public Candidate? Candidate { get; set; }

    }
}
