namespace JobApplication.Application.Interfaces
{
    /// <summary>
    /// Abstraction for Optical Character Recognition (OCR) text extraction from images.
    /// </summary>
    public interface IOcrService
    {
        string ExtractTextFromImage(byte[] imageBytes);
    }
}
