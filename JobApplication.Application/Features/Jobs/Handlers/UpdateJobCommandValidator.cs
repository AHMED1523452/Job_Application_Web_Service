using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Interfaces;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
    {
        public UpdateJobCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).NotNull().SetValidator(new JobRequestDTOValidator(unitOfWork));
        }
    }
}
