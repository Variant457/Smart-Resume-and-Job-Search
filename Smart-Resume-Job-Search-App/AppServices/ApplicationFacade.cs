using Smart_Resume_Job_Search_App.Domain;
using Smart_Resume_Job_Search_App.Data;
using System.Diagnostics;
using System.Linq;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class ApplicationFacade
    {
        private readonly TextExtractionService textExtractionService;
        private readonly AppDbContext db;
        private readonly JobSearchService jobSearchService;

        public ApplicationFacade(TextExtractionService textExtractionService, AppDbContext db, JobSearchService jobSearchService)
        {
            this.textExtractionService = textExtractionService;
            this.db = db;
            this.jobSearchService = jobSearchService;
        }

        public bool HasPreviousResume()
        {
            Debug.WriteLine("[ApplicationFacade] HasPreviousResume");
            return db.LatestParsedResume.Any();
        }

        public async Task UploadResume(InputBlob input)
        {
            Debug.WriteLine($"[ApplicationFacade] UploadResume: {input?.FileName} size={input?.Content?.Length}");

            var workflow = new AppWorkflow(input, textExtractionService, db, jobSearchService);
            workflow.SetState();

            await Task.CompletedTask;
        }

        public string? GetHistory()
        {
            Debug.WriteLine("[ApplicationFacade] GetHistory");

            // TODO: pull from _db.JobPostings / _db.TailoredResumes / _db.OptimizationResults
            // and format whatever "history" to show.
            return null;
        }
    }
}
