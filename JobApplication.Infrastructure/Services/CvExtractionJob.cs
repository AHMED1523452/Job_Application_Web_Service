using Hangfire;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JobApplication.Infrastructure.Services
{
    public sealed class CvExtractionJob : ICvExtractionJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;
        private readonly IExtractorService _extractor;
        private readonly IOcrService _ocrService;
        private readonly IValidatorService _validator;
        private readonly ILogger<CvExtractionJob> _logger;

        public CvExtractionJob(
                                IUnitOfWork unitOfWork,
                                IFileStorageService fileStorage,
                                IExtractorService extractor,
                                IOcrService ocrService,
                                IValidatorService validator,
                                ILogger<CvExtractionJob> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _extractor = extractor;
            _ocrService = ocrService;
            _validator = validator;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task ExtractTextAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
        {
            //. Projection: only StoragePath and OriginalFileName are needed — the full CV row
            //. (and any navigation properties) are never loaded into memory.
            var cvData = await _unitOfWork.Repository<Domain.Entities.CandidateCVs>()
                .GetByIdAsNoTrackingAsync(
                    cv => cv.Id == candidateCvId,
                    cv => new CandidateCvExtractionDataDTO //  as selector 
                    {
                        StoragePath = cv.StoragePath,
                        OriginalFileName = cv.OriginalFileName
                    },
                    cancellationToken);

            if (cvData is null)
            {
                _logger.LogWarning(
                    "CV extraction skipped: candidate CV {CandidateCvId} was not found.", candidateCvId);
                return;
            }

            var extractionRepository = _unitOfWork.Repository<Domain.Entities.CandidateCvExtraction>();
            var extraction = await extractionRepository.GetByIdAsyncWithTracking(
                record => record.CandidateCvId == candidateCvId, cancellationToken);

            if (extraction is null || extraction.Status == CvExtractionStatus.Completed)
                return;

            extraction.Status = CvExtractionStatus.Processing;
            extraction.ErrorMessage = null;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await using var storageStream = await _fileStorage.DownloadAsync(
                    cvData.StoragePath, FileCategory.CandidateCV.ToString(), cancellationToken);

                using var buffer = new MemoryStream();
                await storageStream.CopyToAsync(buffer, cancellationToken);
                var fileBytes = buffer.ToArray();

                var fileExtension = Path.GetExtension(cvData.OriginalFileName).ToLowerInvariant();
                var extractedText = await _extractor.ExtractTextFromBytesAsync(
                    fileBytes, fileExtension, FileCategory.CandidateCV, cancellationToken);

                if (fileExtension == ".pdf" && _validator.IsPdfNeedsOcr(extractedText))
                    extractedText = _ocrService.ExtractTextFromImage(fileBytes);

                extraction.ExtractedText = extractedText;
                extraction.Status = CvExtractionStatus.Completed;
                extraction.CompletedAtUtc = DateTime.UtcNow;

                //. Set-based update: the CV row is updated in the database without being tracked in memory.
                await _unitOfWork.Repository<Domain.Entities.CandidateCVs>().ExecuteUpdateAsync(
                    cv => cv.Id == candidateCvId,
                    setters => setters
                        .SetProperty(cv => cv.ProcessingStatus, CvProcesssingStatus.Processed)
                        .SetProperty(cv => cv.ProcessedAtUtc, DateTime.UtcNow),
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Text extraction completed for candidate CV {CandidateCvId} ({ExtractedCharacters} characters).",
                    candidateCvId, extractedText?.Length ?? 0);
            }
            catch (Exception exception)
            {
                //. Persist the failure on the extraction row so consumers can read the reason
                //. instead of dereferencing a null ExtractedText.
                extraction.Status = CvExtractionStatus.Failed;
                extraction.ErrorMessage = exception.Message;
                extraction.CompletedAtUtc = DateTime.UtcNow;

                try
                {
                    await _unitOfWork.Repository<Domain.Entities.CandidateCVs>().ExecuteUpdateAsync(
                        cv => cv.Id == candidateCvId,
                        setters => setters
                            .SetProperty(cv => cv.ProcessingStatus, CvProcesssingStatus.Failed),
                        cancellationToken);

                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);
                }
                catch (Exception persistenceException)
                {
                    _logger.LogError(persistenceException,
                        "Could not persist the failed extraction state for candidate CV {CandidateCvId}.",
                        candidateCvId);
                }

                _logger.LogError(exception,
                    "CV text extraction failed for candidate CV {CandidateCvId} (file: {FileName}).",
                    candidateCvId, cvData.OriginalFileName);

                throw; //. rethrow so Hangfire schedules the configured retries
            }
        }
    }
}
