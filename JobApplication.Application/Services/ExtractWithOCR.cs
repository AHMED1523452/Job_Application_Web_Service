using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using JobApplication.Application.Interfaces;
using Tesseract;

namespace JobApplication.Application.Services
{
    /// <summary>
    /// Service for performing Optical Character Recognition (OCR) on images using Tesseract.
    /// </summary>
    public class ExtractWithOCR : IOcrService, IDisposable
    {
        private readonly string _tessDataPath;
        private readonly string _language;
        private readonly TesseractEngine _engine;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractWithOCR"/> class.
        /// </summary>
        /// <param name="tessDataPath">Polder containing Tesseract traineddata files (e.g., eng.traineddata).</param>
        /// <param name="language">Language code for OCR (default is "eng" for English).</param>
        /// <param name="engineMode">Tesseract engine mode (defauath to the flt is Default).</param>
        public ExtractWithOCR(string tessDataPath, string language = "eng", EngineMode engineMode = EngineMode.Default)
        {
            if (string.IsNullOrWhiteSpace(tessDataPath))
                throw new ArgumentException("TessData path cannot be null or empty.", nameof(tessDataPath));

            //. validating if the directory or the folder is already exist or not
            if (!Directory.Exists(tessDataPath))
                throw new DirectoryNotFoundException($"TessData directory not found: {tessDataPath}");

            _tessDataPath = tessDataPath;
            _language = string.IsNullOrWhiteSpace(language) ? "eng" : language;

            // Initialize the Tesseract engine
            _engine = new TesseractEngine(_tessDataPath, _language, engineMode);
        }

        /// <summary>
        /// Extracts text from an image provided as a byte array.
        /// </summary>
        /// <param name="imageBytes">The image bytes to process.</param>
        /// <returns>The recognized text from the image.</returns>
        public string ExtractTextFromImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Image bytes cannot be null or empty.", nameof(imageBytes));

           //.transfering the image butes to the units that the tesseract understand
            using var pix = Pix.LoadFromMemory(imageBytes);
            using var page = _engine.Process(pix);
            var text = page.GetText();

            return string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
        }

        /// <summary>
        /// Extracts text from an image file path.
        /// </summary>
        /// <param name="imageFilePath">Path to the image file.</param>
        /// <returns>The recognized text from the image.</returns>
        public string ExtractTextFromFile(string imageFilePath)
        {
            if (string.IsNullOrWhiteSpace(imageFilePath))
                throw new ArgumentException("Image file path cannot be null or empty.", nameof(imageFilePath));

            //. validating if the file is already existing in memory
            if (!File.Exists(imageFilePath))
                throw new FileNotFoundException("Image file not found.", imageFilePath);

            using var image = new Bitmap(imageFilePath);
            return ExtractTextFromBitmap(image);
        }

        /// <summary>
        /// Extracts text from a System.Drawing.Bitmap image.
        /// </summary>
        /// <param name="image">The bitmap image to process.</param>
        /// <returns>The recognized text from the image.</returns>
        public string ExtractTextFromBitmap(Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            //. creating a new memory stream for saving the images or the file
            using var ms = new MemoryStream();
            //.Compress this file and save it in the server of supabase with png type of formating that has loseless of the lines
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //. after that i need to get text in the 
            using var pix = Pix.LoadFromMemory(ms.ToArray());
            using var page = _engine.Process(pix);
            var text = page.GetText(); //.extract the internal text

            return string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
        }

        /// <summary>
        /// Extracts text from an image with confidence scoring.
        /// </summary>
        /// <param name="imageBytes">The image bytes to process.</param>
        /// <returns>A tuple containing the recognized text and the average confidence score.</returns>
        public (string Text, double Confidence) ExtractTextWithConfidence(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Image bytes cannot be null or empty.", nameof(imageBytes));

            using var pix = Pix.LoadFromMemory(imageBytes);
            using var page = _engine.Process(pix);
            var text = page.GetText();
            var confidence = page.GetMeanConfidence();

            return (string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim(), confidence);
        }

        /// <summary>
        /// Extracts text from a Bitmap with confidence scoring.
        /// </summary>
        /// <param name="image">The bitmap image to process.</param>
        /// <returns>A tuple containing the recognized text and the average confidence score.</returns>
        public (string Text, double Confidence) ExtractTextWithConfidenceFromBitmap(Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            using var pix = Pix.LoadFromMemory(ms.ToArray());
            using var page = _engine.Process(pix);
            var text = page.GetText();
            var confidence = page.GetMeanConfidence();

            return (string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim(), confidence);
        }

        /// <summary>
        /// Checks whether the specified traineddata language file exists in the TessData path.
        /// </summary>
        /// <param name="language">The language code to check (e.g., "eng", "ara").</param>
        /// <returns>True if the traineddata file exists; otherwise, false.</returns>
        public bool IsLanguageAvailable(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
                return false;

            var trainedDataFile = Path.Combine(_tessDataPath, $"{language}.traineddata");
            return File.Exists(trainedDataFile);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _engine?.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        ~ExtractWithOCR()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _engine?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
