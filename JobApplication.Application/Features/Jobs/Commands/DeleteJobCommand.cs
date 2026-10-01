using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands
{
    public sealed record DeleteJobCommand(Guid Id) : IRequest<GenericResult<Guid>>;
}
