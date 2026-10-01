using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Chat.DTOs
{
    public sealed class ApplicationChatContext
    {
        public Guid CandidateProfileId { get; init; }
        public Guid CandidateUserId { get; init; }
        public Guid HrProfileId { get; init; }
        public Guid RecruiterUserId { get; init; }
        public JobApplicationStatus Status { get; init; }
    }
}
