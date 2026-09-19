using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace JobApplication.Application.Interfaces
{
    public interface IGenerateJwtToken
    {
        Task<string> GenerateAccessTokenAsync(string Id,
                                              string Email,
                                              string? UserName,
                                              string FullName,
                                              string? role, CancellationToken cancellationToken);
        Task<string> GenerateRefreshTokenAsync(string Id,
                                               string Email,
                                               string? UserName,
                                               string FullName,
                                               string? role, CancellationToken cancellationToken);
    }
}
