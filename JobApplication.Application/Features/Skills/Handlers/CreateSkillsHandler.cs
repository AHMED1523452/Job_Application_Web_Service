using JobApplication.Application.Features.Skills.Commands;
using JobApplication.Application.Features.Skills.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Skills.Handlers
{
    public sealed class CreateSkillsHandler : IRequestHandler<CreateSkillsCommand, GenericResult<CreateSkillsResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService userService;
        private readonly IApplicationCache cache;

        public CreateSkillsHandler(IUnitOfWork unitOfWork,  
                                   ICurrentUserService userService,
                                   IApplicationCache cache)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.cache = cache;
        }
        async Task<GenericResult<CreateSkillsResponseDTO>> IRequestHandler<CreateSkillsCommand, GenericResult<CreateSkillsResponseDTO>>.Handle(CreateSkillsCommand request, CancellationToken cancellationToken)
        {
            var userId =userService.UserId();
            if (userId == default)
                return await Result.FailureAsync<CreateSkillsResponseDTO>("Authentication is required.");
             
            IGenericRepository<Skill> instance = unitOfWork.Repository<Skill>()!;

            var normalizedName = request.requestDTO.Name.Trim();
            if (await instance
                        .AnyAsync(predicate: skill => skill.Name.ToLower() == normalizedName.ToLower() &&
                                                skill.IsActive &&
                                                !skill.IsArchived &&
                                                !skill.IsDeleted, cancellationToken))
                return await Result.FailureAsync<CreateSkillsResponseDTO>("Skill is already exist. ");

            var skill = new Skill
            {
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()!,
                CreatedByIp = userService.UserIP()!,
                Description = request.requestDTO.Description,
                Name =normalizedName.ToLower(), //. must be uploaded without spaces and in lower case
                NormalizedName = normalizedName.ToUpper(),
                SkillCategoryId = request.requestDTO.SkillCategoryId
            };

            await instance.AddAsync(skill, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cache.InvalidateAsync("Skills:", cancellationToken); //. try to review the prefix yasta 

            return await Result.SuccessAsync<CreateSkillsResponseDTO>(new CreateSkillsResponseDTO
            {
                Name = request.requestDTO.Name,
                Description = request.requestDTO.Description
            });
        }
    }
}
