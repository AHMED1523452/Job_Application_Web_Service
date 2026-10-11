using JobApplication.Application.Features.Skills.DTOs;
using JobApplication.Application.Features.Skills.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;
using System.Linq.Expressions;

namespace JobApplication.Application.Features.Skills.Handlers
{
    public sealed class GetSkillsHandler : IRequestHandler<GetSkillsQuery, GenericResult<PaginatedResult<SkillsDTO>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IApplicationCache cache;
        private readonly ICurrentUserService userService;

        public GetSkillsHandler(IUnitOfWork unitOfWork,
                                IApplicationCache cache,
                                ICurrentUserService userService)
        {
            this.unitOfWork = unitOfWork;
            this.cache = cache;
            this.userService = userService;
        }
        public async Task<GenericResult<PaginatedResult<SkillsDTO>>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
        {
            IGenericRepository<Skill> instace = unitOfWork.Repository<Skill>();
            if (instace is null) throw new ArgumentNullException(nameof(instace));

            //. will be used in caching 
            var searchKey = Uri.EscapeDataString(request.search?.Trim().ToLowerInvariant() ?? string.Empty);
            var userId = userService.GetCurrentUserIdOrNull()?.ToString() ?? "anonymous";

            var cacheKey = $"Skills:all:user:{userId}:page:{request.page}:pageSize:{request.pageSize}:search:{searchKey}";
            var cached = await cache.GetAsync<GenericResult<PaginatedResult<SkillsDTO>>>(cacheKey, cancellationToken);
            if (cached is not null) return cached;


            Expression<Func<Skill, bool>> predicate;
            if (!string.IsNullOrWhiteSpace(request.search))
            {
                //. removing the spaces from the Search string and after that search in the values and the description column
                var search = request.search.Trim();
                predicate = candidate => !candidate.IsDeleted && !candidate.IsArchived && candidate.IsActive
                        || (candidate.Description != null && candidate.Description.Contains(search));
            }
            else
            {
                predicate = candidate => !candidate.IsDeleted && !candidate.IsArchived && candidate.IsActive;
            }


            var paginatedResult = await instace
                         .GetPagedAsync(page: request.page,
                                        pageSize: request.pageSize,
                                        predicate: predicate,
                                        selector: op => new SkillsDTO
                                        {
                                            Name = op.Name,
                                            SkillCategoryName = op.SkillCategory.Name,
                                            SkillCategoryId = op.SkillCategoryId,
                                        },
                                        searchTerm: request.search,
                                        cancellationToken: cancellationToken,
                                        includes: op => op.SkillCategory);

            var result = new PaginatedResult<SkillsDTO>
            {
                Items = paginatedResult.Items,
                Pagination = new PaginationMetadata
                {
                    Page = request.page,
                    PageSize = request.pageSize,
                    TotalCount = paginatedResult.TotalCount,
                    TotalPages = (int)Math.Ceiling(paginatedResult.TotalCount / (double)request.pageSize)
                }
            };

            var response = await Result.SuccessAsync(result, "Skills retrieved successfully.");
            await cache.CreateAsync(cacheKey, response, cancellationToken);
            return response;
        }
    }
}
