using Smart_Resume_Job_Search_App.Domain;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class ApplicationFacade
    {
        private readonly TextExtractionService textExtractionService;

        public ApplicationFacade(TextExtractionService textExtractionService)
        {
            this.textExtractionService = textExtractionService;
        }

        public bool HasPreviousResume()
        {
            Debug.WriteLine("[ApplicationFacade] HasPreviousResume");
            return true;
        }

        public async Task UploadResume(InputBlob input)
        {
            Debug.WriteLine($"[ApplicationFacade] UploadResume: {input?.FileName} size={input?.Content?.Length}");
            var workflow = new AppWorkflow(input, textExtractionService);

            workflow.SetState();
            await Task.CompletedTask;
        }

        public string? GetHistory()
        {
            Debug.WriteLine("[ApplicationFacade] GetHistory");
            return null;
        }
    }
}
