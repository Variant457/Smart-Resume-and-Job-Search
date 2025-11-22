using Smart_Resume_Job_Search_App.Domain;
using Smart_Resume_Job_Search_App.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

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
            await Task.Run(workflow.SetState);
        }

        public async Task<List<JobPosting>> GetJobPostings()
        {
            Debug.WriteLine("[ApplicationFacade] GetJobPostings");
            return await db.JobPostings
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();
        }
    }
}
