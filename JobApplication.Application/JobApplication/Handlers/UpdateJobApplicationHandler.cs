using JobApplication.Application.Interfaces;
using JobApplication.Application.JobApplication.Commands;
using JobApplication.Application.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace JobApplication.Application.JobApplication.Handlers
{
    public sealed class UpdateJobApplicationHandler : IRequestHandler<UpdateJobApplicationCommand, GenericResult<UpdateApplicationResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService userService;

        public UpdateJobApplicationHandler(IUnitOfWork unitOfWork,
                                           ICurrentUserService userService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
        }
        public async Task<GenericResult<UpdateApplicationResponseDTO>> Handle(UpdateJobApplicationCommand request, CancellationToken cancellationToken)
        {
            var instance = unitOfWork.Repository<JobApplications>();
            if (instance is null) throw new ArgumentNullException(nameof(instance));

            //. Tracking must be here 
            var existingJobApplication = await instance
                                .GetByIdAsyncWithTracking(predicate: op => op.Id == request.id &&
                                              op.IsActive &&
                                              !op.IsArchived &&
                                              !op.IsDeleted, cancellationToken);
            if (existingJobApplication is null) return await Result.FailureAsync<UpdateApplicationResponseDTO>("Job application not found");

            existingJobApplication.UpdatedAt = DateTime.UtcNow;
            existingJobApplication.UpdatedBy = userService.UserId().ToString();
            existingJobApplication.UpdatedByIp = userService.UserIP();
            existingJobApplication.Description = request.requestDTO.Description;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return await Result.SuccessAsync<UpdateApplicationResponseDTO>(new UpdateApplicationResponseDTO
            {
                JobId = existingJobApplication.JobId,
                Description = request.requestDTO.Description
            });
        }
    }
}
