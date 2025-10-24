using Smart_Resume_Job_Search_App.Domain;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class ApplicationFacade
    {
        private readonly AppWorkflow workflow = new();

        public bool HasPreviousResume()
        {
            Debug.WriteLine("[ApplicationFacade] HasPreviousResume");
            return true;
        }

        public async Task UploadResume(InputBlob input)
        {
            Debug.WriteLine($"[ApplicationFacade] UploadResume: {input?.FileName} size={input?.Content?.Length}");
            // Transition to parsing state with input
            workflow.SetState(input);
            await Task.CompletedTask;
        }

        public string? GetHistory()
        {
            Debug.WriteLine("[ApplicationFacade] GetHistory");
            return null;
        }
    }
}
