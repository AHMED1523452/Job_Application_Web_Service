using AutoMapper;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Features.Candidate.Handlers;

/// <summary>
/// Applies the collection portion of a full candidate-profile update while retaining IDs for existing rows.
/// 
/// </summary>
internal static class CandidateProfileCollectionUpdater
{
    public static bool RequestReferencesOwnedChildren(CandidateProfile candidate, CandidateRequestDTO request)
    {
        // Hash sets keep ownership checks constant-time for each submitted child ID.
        var experienceIds = candidate.experiences.Select(item => item.Id).ToHashSet();
        var educationIds = candidate.educations.Select(item => item.Id).ToHashSet();
        var certificationIds = candidate.certifications.Select(item => item.Id).ToHashSet();

        return request.Experiences.All(item => !item.Id.HasValue || experienceIds.Contains(item.Id.Value))
            && request.Educations.All(item => !item.Id.HasValue || educationIds.Contains(item.Id.Value))
            && request.Certifications.All(item => !item.Id.HasValue || certificationIds.Contains(item.Id.Value));
    }

    public static async Task ApplyUpdateAsync(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CandidateProfile candidate,
        CandidateRequestDTO request,
        string actor,
        CancellationToken cancellationToken)
    {
        await SyncSkillsAsync(unitOfWork, mapper, candidate, request.Skills, actor, cancellationToken);

        await SyncCollectionAsync(
            candidate.experiences, request.Experiences,
            item => item.Id.HasValue, item => item.Id.GetValueOrDefault(), entity => entity.Id,
            item => mapper.Map<CandidateExperience>(item), (item, entity) => mapper.Map(item, entity),
            entity => { entity.Id = Guid.NewGuid(); entity.CandidateProfileId = candidate.Id; InitializeAudit(entity, actor); },
            entity => MarkUpdated(entity, actor),
            unitOfWork.Repository<CandidateExperience>(), cancellationToken);

        await SyncCollectionAsync(
            candidate.educations, request.Educations,
            item => item.Id.HasValue, item => item.Id.GetValueOrDefault(), entity => entity.Id,
            item => mapper.Map<CandidateEducation>(item), (item, entity) => mapper.Map(item, entity),
            entity => { entity.Id = Guid.NewGuid(); entity.CandidateProfileId = candidate.Id; InitializeAudit(entity, actor); },
            entity => MarkUpdated(entity, actor),
            unitOfWork.Repository<CandidateEducation>(), cancellationToken);

        await SyncCollectionAsync(
            candidate.certifications, request.Certifications,
            item => item.Id.HasValue, item => item.Id.GetValueOrDefault(), entity => entity.Id,
            item => mapper.Map<CandidateCertification>(item), (item, entity) => mapper.Map(item, entity),
            entity => { entity.Id = Guid.NewGuid(); entity.CandidateProfileId = candidate.Id; InitializeAudit(entity, actor); },
            entity => MarkUpdated(entity, actor),
            unitOfWork.Repository<CandidateCertification>(), cancellationToken);

        await SyncPreferenceAsync(unitOfWork, mapper, candidate, request.Preference, actor, cancellationToken);
    }

    private static async Task SyncSkillsAsync(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CandidateProfile candidate,
        IReadOnlyCollection<CandidateSkillRequestDTO> requested,
        string actor,
        CancellationToken cancellationToken)
    {
        await SyncCollectionAsync(
            candidate.Skills, requested,
            _ => true, item => item.SkillId, entity => entity.SkillId,
            item => mapper.Map<CandidateSkill>(item), (item, entity) => mapper.Map(item, entity),
            entity =>
            {
                entity.Id = $"{candidate.Id:N}:{entity.SkillId:N}";
                entity.CandidateProfileId = candidate.Id;
                InitializeAudit(entity, actor);
            }, entity => MarkUpdated(entity, actor),
            unitOfWork.Repository<CandidateSkill>(), cancellationToken);
    }

    private static async Task SyncCollectionAsync<TRequest, TEntity>(
        ICollection<TEntity> currentItems,
        IReadOnlyCollection<TRequest> requestedItems,
        Func<TRequest, bool> hasExistingId,
        Func<TRequest, Guid> requestId,
        Func<TEntity, Guid> entityId,
        Func<TRequest, TEntity> createEntity,
        Action<TRequest, TEntity> mapIntoEntity,
        Action<TEntity> initializeNew,
        Action<TEntity> markUpdated,
        IGenericRepository<TEntity> repository,
        CancellationToken cancellationToken)
        where TEntity : baseClass
    {
        var currentById = currentItems.ToDictionary(entityId);
        var retainedIds = requestedItems.Where(hasExistingId).Select(requestId).ToHashSet();

        foreach (var current in currentItems.Where(item => !retainedIds.Contains(entityId(item))).ToList())
        {
            currentItems.Remove(current);
            await repository.RemoveAsync(current, cancellationToken);
        }

        foreach (var requested in requestedItems)
        {
            var entity = hasExistingId(requested) && currentById.TryGetValue(requestId(requested), out var existing)
                ? existing
                : null;

            if (entity is null)
            {
                entity = createEntity(requested);
                initializeNew(entity);
                currentItems.Add(entity);
                await repository.AddAsync(entity, cancellationToken);
                continue;
            }

            mapIntoEntity(requested, entity);
            markUpdated(entity);
            await repository.EditAsync(entity, cancellationToken);
        }
    }

    private static async Task SyncPreferenceAsync(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CandidateProfile candidate,
        CandidatePreferenceRequestDTO? request,
        string actor,
        CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<CandidatePreference>();
        if (request is null)
        {
            if (candidate.Preference is not null)
            {
                await repository.RemoveAsync(candidate.Preference, cancellationToken);
                candidate.Preference = null;
            }
            return;
        }

        if (candidate.Preference is null)
        {
            candidate.Preference = mapper.Map<CandidatePreference>(request);
            candidate.Preference.Id = Guid.NewGuid();
            candidate.Preference.CandidateProfileId = candidate.Id;
            candidate.Preference.CreatedBy = actor;
            await repository.AddAsync(candidate.Preference, cancellationToken);
            return;
        }

        mapper.Map(request, candidate.Preference);
        MarkUpdated(candidate.Preference, actor);
        await repository.EditAsync(candidate.Preference, cancellationToken);
    }

    private static void InitializeAudit(baseClass entity, string actor)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = actor;
    }

    private static void MarkUpdated(baseClass entity, string actor)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = actor;
    }
}
