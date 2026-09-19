using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Auth.DTOs
{
    public sealed record LoginResponseDTO(string AccessToken, string RefreshToken);
}
