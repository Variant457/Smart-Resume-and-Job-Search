using System.Text;

namespace Smart_Resume_Job_Search_App.Domain.TextExtraction
{
    public sealed class FallbackTextExtractor : ITextExtractor
    {
        public bool CanExtract(InputBlob input) => true;

        public Task<string> ExtractTextAsync(InputBlob input, CancellationToken cancellationToken = default)
        {
            var bytes = input.Content ?? Array.Empty<byte>();
            if (bytes.Length == 0)
                return Task.FromResult(string.Empty);

            // Replace invalid sequences to keep the pipeline moving
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
            return Task.FromResult(utf8.GetString(bytes));
        }
    }
}
