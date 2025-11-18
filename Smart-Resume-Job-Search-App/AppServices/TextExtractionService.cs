using System.Diagnostics;
using System.Collections.Generic;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class TextExtractionService
    {
        private readonly IReadOnlyCollection<ITextExtractor> textExtractors;

        public TextExtractionService(IEnumerable<ITextExtractor> extractors)
        {
            textExtractors = extractors.ToList();
        }

        public string Extract(Domain.InputBlob blob)
        {
            if(blob == null)
            {
                throw new ArgumentNullException(nameof(blob));
            }

            Debug.WriteLine($"[TextExtractionService] Extracting text from blob '{blob.FileName}' with MIME type '{blob.MimeType}'");
            var extractor = textExtractors.FirstOrDefault(e => e.CanExtract(blob));
            if (extractor == null)
            {
                throw new NotSupportedException($"No text extractor found for file '{blob.FileName}' with MIME type '{blob.MimeType}'");
            }

            return extractor.ExtractText(blob);
        }
    }
}
