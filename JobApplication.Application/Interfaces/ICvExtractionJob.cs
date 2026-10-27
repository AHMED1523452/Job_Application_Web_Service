namespace JobApplication.Application.Interfaces
{
    public interface ICvExtractionJob
    {
        Task ExtractTextAsync(Guid candidateCvId, CancellationToken cancellationToken = default);
    }
}
