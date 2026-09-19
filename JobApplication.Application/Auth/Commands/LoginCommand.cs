using JobApplication.Application.Auth.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Auth.Commands
{
    public sealed record LoginCommand(LoginRequestDTO requestDTO) : IRequest<GenericResult<LoginResponseDTO>>;
}
