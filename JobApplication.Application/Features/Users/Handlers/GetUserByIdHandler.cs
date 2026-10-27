using FluentValidation;
using JobApplication.Application.Features.Users.DTOs;
using JobApplication.Application.Features.Users.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Users.Handlers
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, GenericResult<UserResponseDTO>>
    {
        private readonly IUserService _userService;

        public GetUserByIdHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<GenericResult<UserResponseDTO>> Handle(
            GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUserByIdAsync(request.UserId, cancellationToken);
        }
    }

    public class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");
        }
    }
}
