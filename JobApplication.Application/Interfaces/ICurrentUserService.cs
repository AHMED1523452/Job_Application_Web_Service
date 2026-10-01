using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId();
        Guid? GetCurrentUserIdOrNull();
        string UserIP();
        string? UserAgent();
    }
}
