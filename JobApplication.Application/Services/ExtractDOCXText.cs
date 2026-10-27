using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace JobApplication.Application.Services
{
    public static class ExtractDOCXText
    {
        public static string DocxFileTextExtractor(byte[] docxBytes)
        {
            var stream = new MemoryStream(docxBytes);
            using var document = WordprocessingDocument.Open(stream, false);

            var body = document.MainDocumentPart?.Document.Body;
            if (body is null) return string.Empty;

            return string.Join("\n", body.Descendants().Select(p => p.InnerText));
        }
    }
}
