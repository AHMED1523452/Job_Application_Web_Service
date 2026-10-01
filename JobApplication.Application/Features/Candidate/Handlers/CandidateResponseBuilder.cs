using AutoMapper;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Features.Candidate.Handlers;

internal static class CandidateResponseBuilder
{
    public static async Task<CandidateResponseDTO> BuildAsync(
        Domain.Entities.CandidateProfile candidate,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var responses = await BuildManyAsync([candidate], unitOfWork, mapper, cancellationToken);
        return responses.Single();
    }

    public static async Task<List<CandidateResponseDTO>> BuildManyAsync(
        IEnumerable<Domain.Entities.CandidateProfile> candidates,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var responses = mapper.Map<List<CandidateResponseDTO>>(candidates);
        var skillIds = responses.SelectMany(response => response.Skills)
            .Select(skill => skill.SkillId).Distinct().ToArray();

        if (skillIds.Length == 0) return responses;

        var skills = await unitOfWork.Repository<Skill>().GetAllAsync<Skill>(
            skill => skillIds.Contains(skill.Id), cancellationToken: cancellationToken);
        var skillNames = skills.ToDictionary(skill => skill.Id, skill => skill.Name);

        foreach (var skill in responses.SelectMany(response => response.Skills))
            if (skillNames.TryGetValue(skill.SkillId, out var name)) skill.SkillName = name;

        return responses;
    }
}
