
using FluentValidation;
using JobApplication.Application.Features.Skills.Commands;

namespace JobApplication.Application.Features.Skills.Handlers
{
    public class DeleteSkillCommandValidator : AbstractValidator<DeleteSkillCommand>
    {
        public DeleteSkillCommandValidator()
        {
            RuleFor(x => x.Id)
           .NotEmpty();
        }
    }
}
