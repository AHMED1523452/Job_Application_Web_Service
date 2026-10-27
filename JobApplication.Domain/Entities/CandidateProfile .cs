using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class CandidateProfile : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Title { get; set; }

        public string? Description { get; set; }

        public int? YearsOfExperience { get; set; }

        public string? Location { get; set; }

        public Guid? ProfileImageId { get; set; }

        public MediaFile? ProfileImage { get; set; }
        public CandidatePreference? Preference { get; set; }

        public ICollection<CandidateSkill> Skills { get; set; }
            = new List<CandidateSkill>();

        public ICollection<CandidateCVs> CVs { get; set; }
            = new List<CandidateCVs>();

        public ICollection<JobApplications> Applications { get; set; }
            = new List<JobApplications>();


        public ICollection<JobRecommendation> JobRecommendations { get; set; }
            = new List<JobRecommendation>();

        public ICollection<CandidateRecommendation> CandidateRecommendations { get; set; }
            = new List<CandidateRecommendation>();

        public ICollection<CandidateExperience> experiences
         = new List<CandidateExperience>();

         public ICollection<CandidateEducation> educations
         = new List<CandidateEducation>();

         public ICollection<CandidateCertification> certifications
         = new List<CandidateCertification>();

         public ICollection<SavedJobs> savedJobs
         = new List<SavedJobs>();
    }
}
