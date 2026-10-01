using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Auth.Commands
{
    public sealed record LoginCommand(LoginRequestDTO requestDTO) : IRequest<GenericResult<LoginResponseDTO>>;
}
