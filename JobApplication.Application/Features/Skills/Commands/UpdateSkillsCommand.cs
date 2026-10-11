using JobApplication.Application.Features.Skills.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Skills.Commands
{
    public sealed record UpdateSkillsCommand(Guid Id, CreateSkillsRequestDTO requestDTO) : IRequest<GenericResult<CreateSkillsResponseDTO>>;
}
