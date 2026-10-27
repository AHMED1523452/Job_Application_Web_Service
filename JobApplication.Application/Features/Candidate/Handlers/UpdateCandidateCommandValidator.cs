using FluentValidation;
using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Interfaces;
namespace JobApplication.Application.Features.Candidate.Handlers
{
    public class UpdateCandidateCommandValidator : AbstractValidator<UpdateCandidateCommand>
    {
        public UpdateCandidateCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).NotNull()
                        .SetValidator(new UpdateCandidateRequestDTOValidator(unitOfWork));
        }
    }
}
