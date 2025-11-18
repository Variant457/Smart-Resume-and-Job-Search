using Smart_Resume_Job_Search_App.Domain;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public interface ITextExtractor
    {
        bool CanExtract(InputBlob blob);
        
        string ExtractText(InputBlob blob);
    }
}
