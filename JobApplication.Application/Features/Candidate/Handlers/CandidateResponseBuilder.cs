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

    //. 
    public static async Task<List<CandidateResponseDTO>> BuildManyAsync(
        IEnumerable<Domain.Entities.CandidateProfile> candidates,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        //. the 
        List<CandidateResponseDTO> responses = mapper.Map<List<CandidateResponseDTO>>(candidates);

        //. the distinct is not necessary here (i already adding a distinct skills in DB)
        var skillIds = responses.SelectMany(response => response.Skills)
            .Select(skill => skill.SkillId).Distinct().ToArray();

        if (skillIds.Length == 0) return responses;

        var skills = await unitOfWork.Repository<Skill>().GetAllAsync<Skill>(
            skill => skillIds.Contains(skill.Id), cancellationToken: cancellationToken);

        //. Creating dictionary with Skill Id as key, and Skill Name as a value
        var skillNames = skills.ToDictionary(skill => skill.Id, skill => skill.Name);

        //. Selecting the Candidate Skills that existing in the Canidate profile
        //                      and the Canidate Response after mapping
        //. Assigning the Skill Name that has the same skill id 
        foreach (var skill in responses.SelectMany(response => response.Skills))
            if (skillNames.TryGetValue(skill.SkillId, out var name)) skill.SkillName = name;

        return responses;
    }
}
