using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Http;
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
            var UserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(nameof(ClaimTypes.NameIdentifier));
            return Guid.Parse(UserId!);
        }

        public string UserIP()
        {
            var Ip = _httpContextAccessor.HttpContext.Connection.LocalIpAddress;
            return Ip!.ToString();
        }
    }
}
