using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class SaveJobCommandValidator : AbstractValidator<SaveJobCommand>
    {
        public SaveJobCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).NotNull();
            RuleFor(x => x.Request.Notes).MaximumLength(500).When(x => x.Request is not null);
        }
    }
}
