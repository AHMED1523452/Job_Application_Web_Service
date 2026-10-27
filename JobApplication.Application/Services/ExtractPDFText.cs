using System.Linq;
using System.Text;
using UglyToad.PdfPig;

namespace JobApplication.Application.Services
{
    public static class ExtractPDFText
    {
        public static string PdfExtractor(byte[] pdfBytes)
        {
            var sb = new StringBuilder();
            using var document = PdfDocument.Open(pdfBytes);
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(string.Join(" ", page.Letters.Select(l => l.ToString())));
            }

            return sb.ToString();
        }
    }
}
