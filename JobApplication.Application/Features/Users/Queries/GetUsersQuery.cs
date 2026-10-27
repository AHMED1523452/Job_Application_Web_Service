using MediatR;
using JobApplication.Application.Patterns;
using System.Collections.Generic;
using JobApplication.Application.Features.Users.DTOs;

namespace JobApplication.Application.Features.Users.Queries
{
    public record GetUsersQuery() : IRequest<GenericResult<List<UserResponseDTO>>>;
}
