using System.Diagnostics;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Smart_Resume_Job_Search_App.Domain.TextExtraction
{
    public sealed class PdfTextExtractor : ITextExtractor
    {
        public bool CanExtract(InputBlob input)
        {
            if (input == null) return false;
            var ext = Path.GetExtension(input.FileName ?? string.Empty).ToLowerInvariant();
            var mime = input.MimeType?.ToLowerInvariant();
            return mime == "application/pdf" || ext == ".pdf";
        }

        public Task<string> ExtractTextAsync(InputBlob input, CancellationToken cancellationToken = default)
        {
            if (input.Content == null || input.Content.Length == 0)
                return Task.FromResult(string.Empty);

            try
            {
                using var stream = new MemoryStream(input.Content, writable: false);
                using var document = PdfDocument.Open(stream);

                var builder = new StringBuilder();
                foreach (Page page in document.GetPages())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var text = page.Text;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        builder.AppendLine(text.Trim());
                        builder.AppendLine();
                    }
                }

                return Task.FromResult(builder.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PdfTextExtractor] Failed to read PDF: {ex}");
                return Task.FromResult(string.Empty);
            }
        }
    }
}
