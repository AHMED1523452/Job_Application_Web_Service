using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Skills.Commands
{
    public sealed record DeleteSkillCommand(Guid Id) : IRequest<GenericResult<string>>;
}
