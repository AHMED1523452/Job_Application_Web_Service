using FluentValidation;
using JobApplication.Application.Features.Users.DTOs;
using JobApplication.Application.Features.Users.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Users.Handlers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, GenericResult<List<UserResponseDTO>>>
    {
        private readonly IUserService _userService;

        public GetUsersHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<GenericResult<List<UserResponseDTO>>> Handle(
            GetUsersQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetAllUsersAsync(cancellationToken);
        }
    }

    public class GetUsersValidator : AbstractValidator<GetUsersQuery>
    {
        public GetUsersValidator()
        {
            // GetUsersQuery has no parameters to validate, but validator exists for consistency
            // and future validation rules if needed.
        }
    }
}
