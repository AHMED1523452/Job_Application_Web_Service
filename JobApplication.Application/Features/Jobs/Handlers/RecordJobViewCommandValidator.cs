using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class RecordJobViewCommandValidator : AbstractValidator<RecordJobViewCommand>
    {
        public RecordJobViewCommandValidator() => RuleFor(x => x.Id).NotEmpty();
    }
}
