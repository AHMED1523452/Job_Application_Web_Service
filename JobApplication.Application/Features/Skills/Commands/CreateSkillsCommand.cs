using JobApplication.Application.Features.Skills.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Skills.Commands
{
    public sealed record CreateSkillsCommand(CreateSkillsRequestDTO requestDTO) : IRequest<GenericResult<CreateSkillsResponseDTO>>;
}
