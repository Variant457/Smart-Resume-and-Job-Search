using Smart_Resume_Job_Search_App.Domain;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class ApplicationFacade
    {
        public bool HasPreviousResume()
        {
            Debug.WriteLine("[ApplicationFacade] HasPreviousResume");
            return true;
        }

        public Task UploadResume(InputBlob input)
        {
            Debug.WriteLine($"[ApplicationFacade] UploadResume: {input?.FileName} size={input?.Content?.Length}");
            return Task.CompletedTask;
        }

        public string GetHistory()
        {
            Debug.WriteLine("[ApplicationFacade] GetHistory");
            return null;
        }
    }
}
