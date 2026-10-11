using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;


namespace JobApplication.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId()
        {
            var UserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(UserId!);
        }

        public Guid? GetCurrentUserIdOrNull()
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }

        public string UserIP()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }

        public string? UserAgent() => _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
    }
}