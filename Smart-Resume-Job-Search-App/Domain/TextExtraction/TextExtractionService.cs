using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.Domain.TextExtraction
{
    public sealed class TextExtractionService
    {
        private readonly IReadOnlyList<ITextExtractor> extractors;

        public TextExtractionService(IEnumerable<ITextExtractor> extractors)
        {
            this.extractors = extractors?.ToList() ?? throw new ArgumentNullException(nameof(extractors));
        }

        public async Task<string> ExtractAsync(InputBlob input, CancellationToken cancellationToken = default)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            foreach (var extractor in extractors)
            {
                if (!extractor.CanExtract(input))
                    continue;

                try
                {
                    var text = await extractor.ExtractTextAsync(input, cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(text))
                        return text;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[TextExtractionService] Extractor {extractor.GetType().Name} failed: {ex}");
                }
            }

            return string.Empty;
        }

        public static TextExtractionService CreateDefault()
        {
            var defaults = new ITextExtractor[]
            {
                new PlainTextExtractor(),
                new DocxTextExtractor(),
                new PdfTextExtractor(),
                new FallbackTextExtractor(),
            };

            return new TextExtractionService(defaults);
        }
    }
}
