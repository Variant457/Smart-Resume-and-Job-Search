using Smart_Resume_Job_Search_App.Domain;

namespace Smart_Resume_Job_Search_App.Domain.TextExtraction
{
    public interface ITextExtractor
    {
        bool CanExtract(InputBlob input);

        Task<string> ExtractTextAsync(InputBlob input, CancellationToken cancellationToken = default);
    }
}
