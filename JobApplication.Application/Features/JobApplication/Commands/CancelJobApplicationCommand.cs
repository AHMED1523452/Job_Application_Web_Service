using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplication.Commands
{
    public sealed record CancelJobApplicationCommand(Guid candidateProfileId, Guid appId)
        : IRequest<GenericResult<CancelJobApplicationResponse>>;
}
