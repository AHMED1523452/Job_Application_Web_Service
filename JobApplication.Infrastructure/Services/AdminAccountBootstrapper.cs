using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JobApplication.Infrastructure.Services
{
    public sealed class AdminAccountBootstrapper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminAccountBootstrapper> _logger;

        public AdminAccountBootstrapper(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration,
            ILogger<AdminAccountBootstrapper> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var section = _configuration.GetSection("AdminBootstrap");
            var password = section["Password"];
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogInformation("Admin bootstrap skipped because AdminBootstrap:Password is not configured.");
                return;
            }

            var email = section["Email"];
            var fullName = section["FullName"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName))
                throw new InvalidOperationException("AdminBootstrap:Email and AdminBootstrap:FullName are required when a bootstrap password is configured.");

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Could not create the Admin role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
            {
                if (!await _userManager.IsInRoleAsync(existingUser, "Admin"))
                    throw new InvalidOperationException("The configured bootstrap email already belongs to a non-admin account. No role changes were made.");

                _logger.LogInformation("Configured bootstrap account already exists as an Admin; no changes were made.");
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                Address = section["Address"],
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Could not create the bootstrap Admin account: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");

            var addRoleResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                throw new InvalidOperationException($"Could not assign the Admin role: {string.Join("; ", addRoleResult.Errors.Select(error => error.Description))}");
            }

            _logger.LogInformation("Created the configured bootstrap Admin account {Email}.", email);
        }
    }
}
