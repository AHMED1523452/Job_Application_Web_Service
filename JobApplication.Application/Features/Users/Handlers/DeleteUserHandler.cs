using FluentValidation;
using JobApplication.Application.Features.Users.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Users.Handlers
{
    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, GenericResult<bool>>
    {
        private readonly IUserService _userService;

        public DeleteUserHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<GenericResult<bool>> Handle(
            DeleteUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.DeleteUserAsync(request.UserId, cancellationToken);
        }
    }

    public class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");
        }
    }
}
