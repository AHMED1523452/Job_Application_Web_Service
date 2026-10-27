using FluentValidation;
using JobApplication.Application.Features.Skills.Commands;
using JobApplication.Application.Features.Skills.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Skills.Handlers
{
    public sealed class UpdateSkillsHandler : IRequestHandler<UpdateSkillsCommand, GenericResult<CreateSkillsResponseDTO>>
    {
        private readonly ICurrentUserService userService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IApplicationCache cache;

        public UpdateSkillsHandler(ICurrentUserService userService, 
                                   IUnitOfWork  unitOfWork, 
                                   IApplicationCache  cache)
        {
            this.userService = userService;
            this.unitOfWork = unitOfWork;
            this.cache = cache;
        }
        public async Task<GenericResult<CreateSkillsResponseDTO>> Handle(UpdateSkillsCommand request, CancellationToken cancellationToken)
        {
            var userId = userService.GetCurrentUserIdOrNull();
            if (!userId.HasValue)
                return await Result.FailureAsync<CreateSkillsResponseDTO>("Authentication is required. ");

            IGenericRepository<Skill> instance = unitOfWork.Repository<Skill>()!;

            var normalizedName = request.requestDTO.Name.Trim();
            //.the one that created the skill must be the same person that will update 
            Skill existingSkill = await instance.GetByIdAsyncWithTracking(predicate: skill => skill.Id == request.Id &&
                                                                         skill.CreatedBy == userId.ToString() &&
                                                                         !skill.IsArchived &&
                                                                         skill.IsActive &&
                                                                         !skill.IsDeleted, cancellationToken);
            if (existingSkill is null) return await Result.FailureAsync<CreateSkillsResponseDTO>("Skill not found");

            //. Duplication validating 
            if (await instance.AnyAsync(skill => skill.Name.ToLower() == normalizedName.ToLower() &&
                                                !skill.IsArchived &&
                                                skill.IsActive &&
                                                !skill.IsDeleted))

                return await Result.FailureAsync<CreateSkillsResponseDTO>("Skill is already exist. ");

            existingSkill.Name = normalizedName.ToLower();
            existingSkill.NormalizedName = normalizedName.ToUpper();
            existingSkill.Description = request.requestDTO.Description;
            existingSkill.UpdatedAt = DateTime.UtcNow;
            existingSkill.UpdatedBy = userId.ToString()!;
            existingSkill.UpdatedByIp = userService.UserIP()!;
            existingSkill.SkillCategoryId = request.requestDTO.SkillCategoryId;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cache.InvalidateAsync("Skills:", cancellationToken);

            return await Result.SuccessAsync<CreateSkillsResponseDTO>(new CreateSkillsResponseDTO
            {
                Name = request.requestDTO.Name,
                Description = request.requestDTO.Description
            });
        }
    }

    public class UpdateSkillsCommandValidator : AbstractValidator<UpdateSkillsCommand>
    {
        public UpdateSkillsCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Skill Id is required.");
            RuleFor(x => x.requestDTO.Name)
                .NotEmpty().WithMessage("Skill name is required.")
                .MaximumLength(100);
            RuleFor(x => x.requestDTO.Description)
                .MaximumLength(500);
            RuleFor(x => x.requestDTO.SkillCategoryId)
                .NotEmpty().WithMessage("Skill Category Id is required.");
        }
    }
}