using JobApplication.Application.JobApplication.Commands;
using JobApplication.Application.JobApplication.DTOs;
using JobApplication.Domain.Enums;
using JobApplication.Infrastructure.Persistence;
using MediatR;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using FluentValidation.Results;
using JobApplication.Domain.Entities;
namespace JobApplication.Application.JobApplication.Handlers
{
    public class CreateJobApplicationHander : IRequestHandler<CreateJobApplicationCommand, GenericResult<CreateJobApplicationResponseDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService userService;

        public CreateJobApplicationHander(IUnitOfWork unitOfWork,
                                          ICurrentUserService userService)
        {
            _unitOfWork = unitOfWork;
            this.userService = userService;
        }
        public async Task<GenericResult<CreateJobApplicationResponseDTO>> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
        {
            var instance = _unitOfWork.Repository<Domain.Entities.Jobs>();
            if(instance is null) throw new ArgumentNullException(nameof(instance));
            var job = await instance.GetByIdAsync(op =>
                                                op.Id == request.requestDTO.JobId &&
                                                op.IsActive &&
                                                !op.IsArchived &&
                                                !op.IsDeleted, cancellationToken);
            if (job is null) return await Result.FailureAsync<CreateJobApplicationResponseDTO>("Job not found! ");

            var jobApplication = new JobApplications
            {
                Id = Guid.NewGuid(),
                JobId = request.requestDTO.JobId,
                Description = request.requestDTO.Description,
                CandidateId = userService.UserId(),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = userService.UserIP(),
                CreatedBy = userService.UserId()!.ToString(),
            };
            var AnotherInstance = _unitOfWork.Repository<JobApplications>();
            if (AnotherInstance is null) throw new ArgumentNullException(nameof(AnotherInstance));

            await AnotherInstance.AddAsync(jobApplication, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<CreateJobApplicationResponseDTO>(new CreateJobApplicationResponseDTO
            {
                JobId = request.requestDTO.JobId,
                Description = request.requestDTO.Description,
                Status = JobApplicationStatus.Applied.ToString()
            });
        }
    }
}
