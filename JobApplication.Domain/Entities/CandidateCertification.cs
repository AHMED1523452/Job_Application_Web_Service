
namespace JobApplication.Domain.Entities
{
    public class CandidateCertification : baseClass
    {
        public Guid Id { get; set; }

        // FK -> CandidateProfile (OnDelete: Cascade)
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string IssuingOrganization { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public string? CredentialId { get; set; }
        public string? CredentialUrl { get; set; }
    }
}
