using JobApplication.Application.Interfaces;
using JobApplication.Application.JobApplication.Commands;
using JobApplication.Application.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using JobApplication.Infrastructure.Persistence;
using MediatR;

namespace JobApplication.Application.JobApplication.Handlers
{
    public sealed class CancelJobApplicationHandler : IRequestHandler<CancelJobApplicationCommand, GenericResult<CancelJobApplicationResponse>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService userService;

        public CancelJobApplicationHandler(IUnitOfWork unitOfWork, 
                                           ICurrentUserService userService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
        }
        public async Task<GenericResult<CancelJobApplicationResponse>> Handle(CancelJobApplicationCommand request, CancellationToken cancellationToken)
        {
            var instance = unitOfWork.Repository<JobApplications>();
            if (instance is null) throw new ArgumentNullException(nameof(instance));
            var app = await instance.GetByIdAsyncWithTracking(predicate:
                                                              op => op.Id == request.appId &&
                                                              op.IsActive &&
                                                              !op.IsArchived &&
                                                              !op.IsDeleted,
                                                              cancellationToken,
                                                              includes: op => op.Job);
            if (app == null) return await Result.FailureAsync<CancelJobApplicationResponse>("Job application not found! ");
            app.status = JobApplicationStatus.Cancelled;
            app.UpdatedAt = DateTime.UtcNow;
            app.UpdatedBy = userService.UserId().ToString();
            app.UpdatedByIp = userService.UserIP();
            app.IsActive = false;
            app.IsArchived = true;
            app.IsDeleted = true;

            await instance.EditAsync(app,cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync<CancelJobApplicationResponse>(new CancelJobApplicationResponse
            {
                Id = app.Id,
                JobId = app.JobId
            });
        }
    }
}
