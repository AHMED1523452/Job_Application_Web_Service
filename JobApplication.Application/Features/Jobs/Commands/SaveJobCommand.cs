using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands
{
    public sealed record SaveJobCommand(Guid JobId)
        : IRequest<GenericResult<Guid>>;
}