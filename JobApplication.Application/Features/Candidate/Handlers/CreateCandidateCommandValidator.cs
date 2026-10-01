using FluentValidation;
using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Interfaces;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class CreateCandidateCommandValidator : AbstractValidator<CreateCandidateCommand>
{
    public CreateCandidateCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Request).NotNull().SetValidator(new CandidateRequestDTOValidator(unitOfWork));
    }
}
