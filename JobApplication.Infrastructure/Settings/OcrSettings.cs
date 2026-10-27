namespace JobApplication.Infrastructure.Settings
{
    /// <summary>
    /// Settings for Tesseract OCR engine.
    /// </summary>
    public class OcrSettings
    {
        /// <summary>
        /// Path to the folder containing Tesseract traineddata files (e.g., eng.traineddata, ara.traineddata).
        /// </summary>
        public string TessDataPath { get; set; } = string.Empty;

        /// <summary>
        /// Default language code for OCR (default is "eng" for English).
        /// </summary>
        public string DefaultLanguage { get; set; } = "eng";
    }
}
