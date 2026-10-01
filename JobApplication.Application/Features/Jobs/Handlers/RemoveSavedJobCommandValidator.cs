using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class RemoveSavedJobCommandValidator : AbstractValidator<RemoveSavedJobCommand>
    {
        public RemoveSavedJobCommandValidator() => RuleFor(x => x.Id).NotEmpty();
    }
}
