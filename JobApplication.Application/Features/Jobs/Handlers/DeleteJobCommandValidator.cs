using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class DeleteJobCommandValidator : AbstractValidator<DeleteJobCommand>
    {
        public DeleteJobCommandValidator() => RuleFor(x => x.Id).NotEmpty();
    }
}
