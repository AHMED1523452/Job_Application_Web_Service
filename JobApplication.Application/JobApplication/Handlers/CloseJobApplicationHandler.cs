using JobApplication.Application.Interfaces;
using JobApplication.Application.JobApplication.Commands;
using JobApplication.Application.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.JobApplication.Handlers
{
    public sealed class CloseJobApplicationHandler : IRequestHandler<CloseJobApplicationCommand, GenericResult<CloseJobApplicationResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService userService;

        public CloseJobApplicationHandler(IUnitOfWork unitOfWork,
                                          ICurrentUserService userService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
        }
        public async Task<GenericResult<CloseJobApplicationResponseDTO>> Handle(CloseJobApplicationCommand request, CancellationToken cancellationToken)
        {
            var instance = unitOfWork.Repository<JobApplications>();
            if (instance is null) throw new ArgumentException(nameof(instance));
            JobApplications application = await instance.GetByIdAsyncWithTracking
                                        (predicate: op => op.Id == request.appId &&
                                            op.IsActive &&
                                            !op.IsArchived &&
                                            !op.IsDeleted, cancellationToken);
            if (application == null) return await Result.FailureAsync<CloseJobApplicationResponseDTO>("Application not found! ");

            application.UpdatedAt = DateTime.UtcNow;
            application.UpdatedBy = userService.UserId().ToString();
            application.UpdatedByIp = userService.UserIP();
            application.status = Domain.Enums.JobApplicationStatus.Closed;
            application.IsActive = false;
            application.IsArchived = true;

            await instance.EditAsync(application, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<CloseJobApplicationResponseDTO>(new CloseJobApplicationResponseDTO
            {
                Id = application.Id,
                Status = application.status.ToString()
            });
        }
    }
}
