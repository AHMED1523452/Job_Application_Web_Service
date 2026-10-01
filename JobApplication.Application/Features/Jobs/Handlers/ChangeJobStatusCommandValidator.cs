using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class ChangeJobStatusCommandValidator : AbstractValidator<ChangeJobStatusCommand>
    {
        public ChangeJobStatusCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).NotNull();
            RuleFor(x => x.Request.Status).IsInEnum().When(x => x.Request is not null);
            RuleFor(x => x.Request.Status).Must(status => status != JobStatus.Draft).When(x => x.Request is not null)
                .WithMessage("A job cannot be moved back to Draft after creation.");
        }
    }
}
