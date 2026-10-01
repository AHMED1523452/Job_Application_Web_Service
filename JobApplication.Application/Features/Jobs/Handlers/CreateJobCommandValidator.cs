using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Interfaces;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
    {
        public CreateJobCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Request).NotNull().SetValidator(new JobRequestDTOValidator(unitOfWork));
        }
    }
}
