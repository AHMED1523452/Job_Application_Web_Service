using FluentValidation;
using JobApplication.Application.Features.Skills.Commands;
using JobApplication.Application.Features.Skills.DTOs;


namespace JobApplication.Application.Features.Skills.Handlers
{
    public sealed class CreateSkillCommandValidator : AbstractValidator<CreateSkillsRequestDTO>
    {
        public CreateSkillCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500);

            RuleFor(x => x.SkillCategoryId)
                .NotEmpty()
                .WithMessage("Skill Category Id is required. ");


        }
    }
}
