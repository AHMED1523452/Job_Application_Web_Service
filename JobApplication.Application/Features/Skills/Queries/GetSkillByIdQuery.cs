using JobApplication.Application.Features.Skills.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Skills.Queries
{
    public sealed record GetSkillByIdQuery(Guid Id) : IRequest<GenericResult<SkillsDTO>>;
}
