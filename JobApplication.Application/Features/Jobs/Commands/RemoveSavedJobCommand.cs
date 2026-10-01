using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands
{
    public sealed record RemoveSavedJobCommand(Guid Id) : IRequest<GenericResult<Guid>>;
}
