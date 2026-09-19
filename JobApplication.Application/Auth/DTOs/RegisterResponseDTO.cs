using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Auth.DTOs
{
    public sealed record RegisterResponseDTO(string Id, string FullName, string Email, string AccessToken, string RefreshToken);
}
