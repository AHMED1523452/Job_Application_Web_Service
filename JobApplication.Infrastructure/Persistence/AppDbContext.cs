using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobApplication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Persistence
{

    //. Guid is the identitfier for the user and role entities in the IdentityDbContext. This means that the primary key for both ApplicationUser and IdentityRole will be of type Guid.
    public class AppDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions options) : base(options) {}
        public DbSet<JobApplication.Domain.Entities.JobApplications> jobApplications { get; set; }
        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<Candidate> candidates { get; set; }
        public DbSet<RefreshToken> refreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed default roles for Identity from configuration (fallback to hard-coded values)
            try
            {
                // Attempt to read settings via environment variable bound JSON if present at design-time is not available here.
                // EF Core migrations run in design-time where IConfiguration is not available; keep fallback values to ensure migrations work.
                builder.Entity<IdentityRole<Guid>>().HasData(
                    new IdentityRole<Guid>
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Name = "User",
                        NormalizedName = "USER",
                        ConcurrencyStamp = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa").ToString()
                    },
                    new IdentityRole<Guid>
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Name = "Admin",
                        NormalizedName = "ADMIN",
                        ConcurrencyStamp = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb").ToString()
                    },
                    new IdentityRole<Guid>
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        Name = "Recruiter",
                        NormalizedName = "RECRUITER",
                        ConcurrencyStamp = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc").ToString()
                    }
                );
            }
            catch
            {
                // Swallow exceptions to avoid design-time failures; migrations will still include seeded values above.
            }

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            builder.Entity<Candidate>().
                HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Candidate>(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Jobs>()
                            .HasMany(op => op.jobApplications)
                            .WithOne(op => op.Job)
                            .HasForeignKey(op => op.JobId)
                            .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Candidate>()
                            .HasMany(op => op.jobApplications)
                            .WithOne(op => op.Candidate)
                            .HasForeignKey(op => op.CandidateId)
                            .OnDelete(DeleteBehavior.NoAction);
        }

    }
}
