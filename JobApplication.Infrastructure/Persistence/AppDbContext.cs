using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Persistence
{

    //. Guid is the identitfier for the user and role entities in the IdentityDbContext. This means that the primary key for both ApplicationUser and IdentityRole will be of type Guid.
    public class AppDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        // Use generic DbContextOptions<AppDbContext> so DI and EF tools can resolve correctly
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<JobApplication.Domain.Entities.JobApplications> jobApplications { get; set; }
        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<CandidateProfile> candidates { get; set; }
        public DbSet<RefreshToken> refreshTokens { get; set; }
        public DbSet<JobApplication.Domain.Entities.VerificationCode> VerificationCodes { get; set; }
        public DbSet<HrProfile> HrProfiles { get; set; }
        public DbSet<JobCategory> JobCategories { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<SkillCategory> SkillCategories { get; set; }
        public DbSet<JobRequirements> JobRequirements { get; set; }
        public DbSet<RequirementMatch> RequirementMatches { get; set; }
        public DbSet<AtsAnalysis> AtsAnalyses { get; set; }
        public DbSet<CandidateCVs> CandidateCVs { get; set; }
        public DbSet<CandidateCvExtraction> CandidateCvExtractions { get; set; }
        public DbSet<CandidateSkill> CandidateSkills { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<IdempotencyRecord> records { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure spatial mapping for Jobs.LocationPoint if NetTopologySuite is available
            try
            {
                modelBuilder.Entity<Jobs>().Property(j => j.LocationPoint).HasColumnType("geography");
            }
            catch
            {
                // Ignore if NetTopologySuite not present at design time
            }

            // Seed default roles for Identity from configuration (fallback to hard-coded values)
            try
            {
                // Attempt to read settings via environment variable bound JSON if present at design-time is not available here.
                // EF Core migrations run in design-time where IConfiguration is not available; keep fallback values to ensure migrations work.
                modelBuilder.Entity<IdentityRole<Guid>>().HasData(
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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Seed SkillCategory lookup data
            try
            {
                //. not best practice but not like a gap 
                modelBuilder.Entity<SkillCategory>().HasData(
                    new SkillCategory { Id = 1, Name = "Programming Languages", Description = "Languages used to write software (e.g., C#, Java, Python)", IsActive = true },
                    new SkillCategory { Id = 2, Name = "Backend Development", Description = "Server-side development, APIs, and business logic", IsActive = true },
                    new SkillCategory { Id = 3, Name = "Frontend Development", Description = "Client-side UI/UX and web technologies", IsActive = true },
                    new SkillCategory { Id = 4, Name = "Database", Description = "Relational and NoSQL databases and data modeling", IsActive = true },
                    new SkillCategory { Id = 5, Name = "Cloud", Description = "Cloud platforms and services (e.g., Azure, AWS)", IsActive = true },
                    new SkillCategory { Id = 6, Name = "DevOps", Description = "CI/CD, automation, and infrastructure as code", IsActive = true },
                    new SkillCategory { Id = 7, Name = "Testing", Description = "Unit, integration, and automated testing practices", IsActive = true },
                    new SkillCategory { Id = 8, Name = "Security", Description = "Application and infrastructure security practices", IsActive = true },
                    new SkillCategory { Id = 9, Name = "AI / Machine Learning", Description = "Machine learning, data science and AI-related skills", IsActive = true },
                    new SkillCategory { Id = 10, Name = "Soft Skills", Description = "Communication, teamwork and other interpersonal skills", IsActive = true }
                );
            }
            catch
            {
                // Swallow exceptions during design-time to avoid migration failures
            }

            modelBuilder.Entity<IdempotencyRecord>()
                .HasKey(op => op.Key)
                .HasName("PK_IdempotencyRecord_Key");

            modelBuilder.Entity<CandidateProfile>().
                HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<CandidateProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //. 1-1 between the uploaded CV and its background extraction record;
            //. the unique index guarantees one extraction per CV.
            modelBuilder.Entity<CandidateCvExtraction>()
                .HasOne(extraction => extraction.CandidateCv)
                .WithOne(cv => cv.Extraction)
                .HasForeignKey<CandidateCvExtraction>(extraction => extraction.CandidateCvId)
                .OnDelete(DeleteBehavior.Cascade);

            //. creating an index to make the read processes more easier
            modelBuilder.Entity<CandidateCvExtraction>()
                .HasIndex(extraction => extraction.CandidateCvId)
                .IsUnique();


            modelBuilder.Entity<Jobs>()
                            .HasMany(op => op.jobApplications)
                            .WithOne(op => op.Job)
                            .HasForeignKey(op => op.JobId)
                            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RequirementMatch>()
                            .HasOne(x => x.JobRequirement)
                            .WithMany()
                            .HasForeignKey(x => x.JobRequirementId)
                            .OnDelete(DeleteBehavior.NoAction);

            //. creating a relation of type 1-1 with Hr and Application User table
            modelBuilder.Entity<HrProfile>()
                            .HasOne<ApplicationUser>()
                            .WithOne()
                            .HasForeignKey<HrProfile>(hr => hr.UserId)
                            .OnDelete(DeleteBehavior.NoAction);

            ////. this is another way for creating a relatioship between model and another one 
            /// if model in domain and another model in infrastructure that's meaning that the model that existing in the domain layer 
            /// can't make a navigation property for the model that existing in the infrastructure layer. 
            /// `so  this is the soluation`

            modelBuilder.Entity<RefreshToken>().HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(user => user.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //. Another 1-M with the Notifiction model 
            modelBuilder.Entity<Notification>().HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(user => user.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<EmailNotification>().HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(user => user.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<EmailNotification>()
                .HasOne(emailNotification => emailNotification.Notification)
                .WithMany()
                .HasForeignKey(emailNotification => emailNotification.NotificationId)
                .OnDelete(DeleteBehavior.NoAction);

            // Scores and weights use the same precision as the existing SQL schema.
            modelBuilder.Entity<AtsAnalysis>().Property(analysis => analysis.AtsScore).HasPrecision(18, 2);
            modelBuilder.Entity<AtsAnalysis>().Property(analysis => analysis.RequirementsScore).HasPrecision(18, 2);
            modelBuilder.Entity<AtsAnalysis>().Property(analysis => analysis.FinalScore).HasPrecision(18, 2);
            modelBuilder.Entity<CandidatePreference>().Property(preference => preference.MinimumMatchScore).HasPrecision(18, 2);
            modelBuilder.Entity<CandidateRecommendation>().Property(recommendation => recommendation.MatchScore).HasPrecision(18, 2);
            modelBuilder.Entity<JobRecommendation>().Property(recommendation => recommendation.MatchScore).HasPrecision(18, 2);
            modelBuilder.Entity<JobRequirements>().Property(requirement => requirement.Weight).HasPrecision(18, 2);
            modelBuilder.Entity<RequirementMatch>().Property(match => match.Score).HasPrecision(18, 2);

            modelBuilder.Entity<CandidateProfile>()
                            .HasMany(op => op.Applications)
                            .WithOne(op => op.CandidateProfile)
                            .HasForeignKey(op => op.CandidateProfileId)
                            .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Jobs>()
                        .HasOne(j => j.JobCategory)
                        .WithMany(c => c.Jobs)
                        .HasForeignKey(j => j.JobCategoryId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobRequirements>()
                .HasOne(requirement => requirement.Skill)
                .WithMany()
                .HasForeignKey(requirement => requirement.SkillId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CandidateExperience>()
                                .HasOne(e => e.CandidateProfile)
                                .WithMany(p => p.experiences)
                                .HasForeignKey(e => e.CandidateProfileId)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CandidateEducation>()
                .HasOne(e => e.CandidateProfile)
                .WithMany(p => p.educations)
                .HasForeignKey(e => e.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CandidateCertification>()
                .HasOne(c => c.CandidateProfile)
                .WithMany(p => p.certifications)
                .HasForeignKey(c => c.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedJobs>()
                .HasOne(s => s.CandidateProfile)
                .WithMany(p => p.savedJobs)
                .HasForeignKey(s => s.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedJobs>()
                .HasOne(s => s.Job)
                .WithMany(j => j.SavedByCandidates)
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedJobs>()
                .HasIndex(s => new { s.CandidateProfileId, s.JobId })
                .IsUnique();

            modelBuilder.Entity<SMSNotification>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SearchHistory>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MatchingHistory>()
                .HasOne(m => m.CandidateProfile)
                .WithMany()
                .HasForeignKey(m => m.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchingHistory>()
                .HasOne(m => m.Job)
                .WithMany(j => j.MatchingHistories)
                .HasForeignKey(m => m.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobView>()
                .HasOne(v => v.Job)
                .WithMany(j => j.Views)
                .HasForeignKey(v => v.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobView>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CandidateView>()
                .HasOne(v => v.CandidateProfile)
                .WithMany()
                .HasForeignKey(v => v.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CandidateView>()
                .HasOne(v => v.HrProfile)
                .WithMany()
                .HasForeignKey(v => v.HrProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.JobApplication)
                .WithMany()
                .HasForeignKey(i => i.JobApplicationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.InterviewerHrProfile)
                .WithMany()
                .HasForeignKey(i => i.InterviewerHrProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.CandidateProfile)
                .WithMany()
                .HasForeignKey(c => c.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.HrProfile)
                .WithMany()
                .HasForeignKey(c => c.HrProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.JobApplication)
                .WithMany(application => application.Chats)
                .HasForeignKey(c => c.JobApplicationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChatParticipant>()
                .HasKey(participant => new { participant.ChatId, participant.UserId });

            modelBuilder.Entity<ChatParticipant>()
                .HasOne(participant => participant.Chat)
                .WithMany(chat => chat.Participants)
                .HasForeignKey(participant => participant.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            //. 1-N with the ApplicationUser table and the ChatParticipant table
            //. 1-N with the  chat table and the ChatParticipant table
            modelBuilder.Entity<ChatParticipant>()
               .HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(participant => participant.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatParticipant>()
                .HasIndex(participant => participant.UserId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.MediaFile)
                .WithMany()
                .HasForeignKey(m => m.MediaFileId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Skill>()
                .HasOne(op => op.SkillCategory)
                .WithMany(op => op.Skills)
                .HasForeignKey(op => op.SkillCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}