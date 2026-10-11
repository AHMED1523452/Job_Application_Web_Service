using JobApplication.Application.Features.Skills.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Skills.Handlers
{
    public sealed class DeleteSkillHandler : IRequestHandler<DeleteSkillCommand, GenericResult<string>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService userService;
        private readonly IApplicationCache cache;

        public DeleteSkillHandler(IUnitOfWork unitOfWork,
                                  ICurrentUserService userService,
                                  IApplicationCache cache)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.cache = cache;
        }
        public async Task<GenericResult<string>> Handle(DeleteSkillCommand request, CancellationToken cancellationToken)
        {
            var userId = userService.GetCurrentUserIdOrNull();
            if (userId.HasValue) return await Result.FailureAsync<string>("Authentication is required. ");

            IGenericRepository<Skill> instance = unitOfWork.Repository<Skill>();
            if (instance is null) throw new ArgumentNullException(nameof(instance));

            var existingSkill = await instance.GetByIdAsyncWithTracking(predicate: op => op.Id == request.Id &&
                                                                                    op.IsActive &&
                                                                                    !op.IsArchived &&
                                                                                    !op.IsDeleted, cancellationToken);
            if (existingSkill is null)
                return await Result.FailureAsync<string>(message: "Skill not found");

            existingSkill.IsActive = false;
            existingSkill.IsArchived = true;
            existingSkill.IsDeleted = true;
            existingSkill.DeletedAt = DateTime.UtcNow;
            existingSkill.DeletedBy = userId.ToString();
            existingSkill.DeletedByIp = userService.UserIP();

            await unitOfWork.SaveChangesAsync(cancellationToken); 
            //. removing a 
            await cache.InvalidateAsync("Skills:", cancellationToken);

            return await Result.SuccessAsync<string>("Skill had been removed. ");
        }
    }
}
