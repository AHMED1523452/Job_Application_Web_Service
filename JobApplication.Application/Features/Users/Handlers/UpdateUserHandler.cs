using FluentValidation;
using JobApplication.Application.Features.Users.Commands;
using JobApplication.Application.Features.Users.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Users.Handlers
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, GenericResult<UserResponseDTO>>
    {
        private readonly IUserService _userService;

        public UpdateUserHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<GenericResult<UserResponseDTO>> Handle(
            UpdateUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.UpdateUserAsync(
                request.UserId,
                request.Request.FullName,
                request.Request.Address,
                cancellationToken);
        }
    }

    public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");

            RuleFor(x => x.Request.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

            RuleFor(x => x.Request.Address)
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Address));
        }
    }
}
