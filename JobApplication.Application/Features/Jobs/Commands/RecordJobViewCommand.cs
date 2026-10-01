using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands
{
    public sealed record RecordJobViewCommand(Guid Id) : IRequest<GenericResult<int>>;
}
