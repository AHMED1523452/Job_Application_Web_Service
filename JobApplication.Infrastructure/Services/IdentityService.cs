using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure.Identity;
using JobApplication.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public IdentityService(IUnitOfWork unitOfWork,
                               UserManager<ApplicationUser> userManager)
        { 
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        public async Task<bool> ConfirmEmailAsync(string Email, 
                                                  CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(Email);
            if (user is null) return false;

            if (user.EmailConfirmed
                     && !user.IsActive
                     && !user.IsRevoked) 
                return false;
            user.EmailConfirmed = true;

            await userManager.UpdateAsync(user);
            return true;
        }

        public async Task<UserContactInfo?> GetUserContactAsync(Guid userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
                return null;

            return new UserContactInfo(user.Id, user.Email, user.FullName);
        }
    }
}
