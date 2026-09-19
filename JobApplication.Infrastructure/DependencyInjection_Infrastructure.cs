using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using JobApplication.Infrastructure.Security;
using JobApplication.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JobApplication.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace JobApplication.Infrastructure
{
    public static class DependencyInjection_Infrastructure
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {

            services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                            );

            //. the settings for the LockOut 
            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.AllowedForNewUsers = true;

                //. limited access failed to login 
                options.Lockout.MaxFailedAccessAttempts = 5;
            });

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = true;

                options.User.RequireUniqueEmail = true; ///. Unique email   
            }).AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<AppDbContext>();

            // Bind RoleSeed settings and register for later use (if needed elsewhere)
            services.Configure<RoleSeedSettings>(configuration.GetSection("RoleSeed"));


            services.AddScoped(typeof
                (GenericRepository<>), typeof(IGenericRepository<>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //. Configuration for JWT Settings 
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.AddScoped<IGenerateJwtToken, GenerateJwtToken>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
