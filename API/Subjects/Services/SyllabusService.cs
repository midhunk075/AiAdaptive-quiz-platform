using API.Exceptions;
using API.Subjects.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace API.Subjects.Services
{
    public class SyllabusService(ITopicGenerationService geminiService) : ISyllabusService
    {
        public async Task<string> ExtractTextAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Syllabus document is empty or unreadable.");

            using var stream = file.OpenReadStream();
            var header = new byte[5];
            var bytesRead = await stream.ReadAsync(header, 0, header.Length);
            stream.Position = 0;

            var isPdfHeader = bytesRead == 5 &&
                              header[0] == (byte)'%' &&
                              header[1] == (byte)'P' &&
                              header[2] == (byte)'D' &&
                              header[3] == (byte)'F' &&
                              header[4] == (byte)'-';
            if (!isPdfHeader)
            {
                throw new ValidationException("Uploaded file is not a valid PDF.");
            }

            PdfDocument pdfDocument;
            try
            {
                var pdfReader = new PdfReader(stream);
                pdfDocument = new PdfDocument(pdfReader);
            }
            catch
            {
                throw new ValidationException("Uploaded file could not be parsed as a PDF.");
            }

            using (pdfDocument)
            {
                var textBuilder = new StringBuilder();

                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    var strategy = new SimpleTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    textBuilder.AppendLine(pageText);
                }

                return textBuilder.ToString();
            }
        }

        public async Task<List<string>> ExtractTopicsWithAIAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return new List<string>();

            return await geminiService.ExtractTopicsAsync(rawText);
        }
    }
}
