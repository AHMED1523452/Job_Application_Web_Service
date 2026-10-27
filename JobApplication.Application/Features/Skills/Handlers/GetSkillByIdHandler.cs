using FluentValidation;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Features.Skills.DTOs;
using JobApplication.Application.Features.Skills.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Skills.Handlers
{
    public sealed class GetSkillByIdHandler : IRequestHandler<GetSkillByIdQuery, GenericResult<SkillsDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService userService;
        private readonly IApplicationCache cache;

        public GetSkillByIdHandler(IUnitOfWork unitOfWork,
                                   ICurrentUserService userService,
                                   IApplicationCache cache)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.cache = cache;
        }
        public async Task<GenericResult<SkillsDTO>> Handle(GetSkillByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"Skills:by-id:{request.Id}";
            var cached = await cache.GetAsync<GenericResult<SkillsDTO>>(cacheKey, cancellationToken);
            if (cached is not null) return cached;

            var existingSkill = await unitOfWork.Repository<Skill>()
                        .GetByIdAsNoTrackingAsync(predicate: op => op.Id == request.Id &&
                                                                 op.IsActive &&
                                                                 !op.IsArchived &&
                                                                 !op.IsDeleted,
                                               selector: op => new SkillsDTO
                                                             {
                                                                 Name = op.Name,
                                                                 SkillCategoryId = op.SkillCategoryId,
                                                                 SkillCategoryName = op.SkillCategory.Name
                                                             },
                                                   cancellationToken,
                                                   includes: op => op.SkillCategory);
            if (existingSkill is null)
                return await Result.FailureAsync<SkillsDTO>("Skill not found.");

            var result = await Result.SuccessAsync<SkillsDTO>(existingSkill);
            await cache.CreateAsync(cacheKey, result, cancellationToken);

            return result;
        }
    }

    public class GetSkillByIdQueryValidator : AbstractValidator<GetSkillByIdQuery>
    {
        public GetSkillByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
    }
}
