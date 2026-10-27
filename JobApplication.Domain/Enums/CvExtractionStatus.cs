namespace JobApplication.Domain.Enums
{
    /// <summary>
    /// Lifecycle of the background text extraction for an uploaded candidate CV.
    /// </summary>
    public enum CvExtractionStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
}
