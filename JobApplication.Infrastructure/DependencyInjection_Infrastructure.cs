using JobApplication.Application.Interfaces;
using JobApplication.Application.Services;
using JobApplication.Infrastructure.Persistence;
using JobApplication.Infrastructure.Security;
using JobApplication.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using JobApplication.Infrastructure.Settings;
using JobApplication.Infrastructure.Identity;
using Hangfire;
using System.Net;
using System.Net.Mail;
using StackExchange.Redis;
using JobApplication.Infrastructure.Services.Storage;
namespace JobApplication.Infrastructure
{
    public static class DependencyInjection_Infrastructure
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            //. had been added for the location point property 
            services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                             sql => sql.UseNetTopologySuite()));

            services.AddHangfire(register =>
                            register.UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

            services.AddHangfireServer();

            var distributedCache = configuration.GetSection("DistributedCache");
            var redisConnection = distributedCache["ConnectionString"];
            var redisInstanceName = distributedCache["InstanceName"];
            var cacheExpirationMinutes = distributedCache.GetValue<int>("DefaultExpirationMinutes");
            if (string.IsNullOrWhiteSpace(redisConnection)
                || string.IsNullOrWhiteSpace(redisInstanceName)
                || cacheExpirationMinutes <= 0)
                throw new InvalidOperationException("DistributedCache connection, instance name, and positive expiration must be configured.");

            //. Register distributed cache settings and services
            services.Configure<DistributedCacheSettings>(distributedCache);

            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
            services.AddScoped<IApplicationCache, DistributedApplicationCache>();

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
            }).AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            // Register NetTopologySuite geometry factory for spatial types if needed
            services.AddSingleton<GeometryFactory>(sp => NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));

            // Bind RoleSeed settings and register for later use (if needed elsewhere)
            services.Configure<RoleSeedSettings>(configuration.GetSection("RoleSeed"));

            // Register generic repository: interface -> implementation
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();
            services.AddScoped<IJobDeadlineService, JobDeadlineService>();
            services.AddScoped<IEmailNotificationQueue, EmailNotificationQueue>();
            services.AddScoped<EmailNotificationDeliveryJob>();
            services.AddScoped<IEmailNotificationDeliveryJob>(provider =>
                provider.GetRequiredService<EmailNotificationDeliveryJob>());

            //. Background job that extracts the text of an uploaded candidate CV
            services.AddScoped<ICvExtractionJob, CvExtractionJob>();

            // Configure FluentEmail to deliver messages through the configured SMTP server.
            var smtp = configuration.GetSection("SMTP");
            var smtpHost = smtp["Host"];
            var smtpPort = int.TryParse(smtp["Port"], out var configuredPort) ? configuredPort : 587;
            var smtpUser = smtp["User"];
            var smtpPassword = smtp["Password"]?.Trim();
            if (string.Equals(smtpHost, "smtp.gmail.com", StringComparison.OrdinalIgnoreCase)
                && smtpPassword is not null)
            {
                // Google displays app passwords in groups for readability; SMTP expects the code without separators.
                smtpPassword = string.Concat(smtpPassword.Where(character => !char.IsWhiteSpace(character)));
            }
            var fromAddress = smtp["From"] ?? smtpUser;
            var fromName = smtp["FromName"];

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(fromAddress))
                throw new InvalidOperationException("SMTP:Host and SMTP:From (or SMTP:User) must be configured.");
            if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPassword))
                throw new InvalidOperationException(
                    "SMTP authentication is required. Configure SMTP:User and SMTP:Password (for example, the SMTP__Password environment variable). ");

            services
                .AddFluentEmail(fromAddress, fromName)
                .AddSmtpSender(() => new SmtpClient(smtpHost, smtpPort)
                {
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword)
                });

            services.AddScoped<IEmailService, SmtpEmailService>();
            // Use custom token hasher (BCrypt) for OTPs and refresh tokens
            services.AddTransient<IPasswordHasher, PasswordHasher>();



            //. Configuration for JWT Settings 
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.AddScoped<IGenerateJwtToken, GenerateJwtToken>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<AdminAccountBootstrapper>();

            //. Register All Services that related with the storage logic 

            services.AddTransient<IValidatorService, ValidatorService>();
            services.AddScoped<IFileStorageService, SupabaseStorageService>();
            services.AddScoped<IExtractorService, ExtractorService>();
            services.AddScoped<IFileHashService, FileHashService>();
            services.AddScoped<IUserService, UserService>();

            //. Bind OCR settings and register IOcrService abstraction with ExtractWithOCR implementation
            services.Configure<OcrSettings>(configuration.GetSection("Ocr"));
            services.AddScoped<IOcrService>(sp =>
            {
                var ocrSettings = sp.GetRequiredService<IOptions<OcrSettings>>().Value;
                var tessDataPath = string.IsNullOrWhiteSpace(ocrSettings.TessDataPath)
                    ? "tessdata"
                    : ocrSettings.TessDataPath;
                return new ExtractWithOCR(tessDataPath, ocrSettings.DefaultLanguage);
            });

            //. Configuration for the Supabase Storage Settings 
            services.Configure<SupabaseStorageServiceOptions>
                                (configuration.GetSection("SupabaseStorage"));

            return services;
        }
    }
}
