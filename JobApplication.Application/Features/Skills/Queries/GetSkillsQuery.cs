using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Features.Skills.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Skills.Queries
{
    public sealed record GetSkillsQuery(int page, int pageSize, string? search) : IRequest<GenericResult<PaginatedResult<SkillsDTO>>>;
}
