using JobApplication.Application.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.JobApplication.Commands
{
    public sealed record CancelJobApplicationCommand(Guid appId) : IRequest<GenericResult<CancelJobApplicationResponse>>;
}
