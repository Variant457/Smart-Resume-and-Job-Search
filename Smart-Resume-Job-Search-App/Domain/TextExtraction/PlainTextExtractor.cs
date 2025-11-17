using System.Text;

namespace Smart_Resume_Job_Search_App.Domain.TextExtraction
{
    public sealed class PlainTextExtractor : ITextExtractor
    {
        public bool CanExtract(InputBlob input)
        {
            if (input == null) return false;
            var ext = Path.GetExtension(input.FileName ?? string.Empty).ToLowerInvariant();
            var mime = input.MimeType?.ToLowerInvariant();
            return mime == "text/plain" || ext == ".txt";
        }

        public Task<string> ExtractTextAsync(InputBlob input, CancellationToken cancellationToken = default)
        {
            // Attempt UTF-8 with BOM detection; fall back to ISO-8859-1 for simple ASCII files
            var content = input.Content ?? Array.Empty<byte>();
            if (content.Length == 0) return Task.FromResult(string.Empty);

            var text = Encoding.UTF8.GetString(content);
            if (string.IsNullOrWhiteSpace(text))
            {
                text = Encoding.Latin1.GetString(content);
            }

            return Task.FromResult(text);
        }
    }
}
