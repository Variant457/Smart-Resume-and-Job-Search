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
        private readonly ResumeTailoringService resumeTailoringService;

        public ApplicationFacade(TextExtractionService textExtractionService, AppDbContext db, JobSearchService jobSearchService, ResumeTailoringService resumeTailoringService)
        {
            this.textExtractionService = textExtractionService;
            this.db = db;
            this.jobSearchService = jobSearchService;
            this.resumeTailoringService = resumeTailoringService;
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

        public async Task<TailoredResume> GetLatestTailoredResume(int jobId)
        {
            Debug.WriteLine($"[ApplicationFacade] GetLatestTailoredResume for JobPostingId={jobId}");
            return await db.TailoredResumes
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(t => t.JobPostingId == jobId);
        }

        public async Task<ParsedResumeRecord> GetLatestParsedResume()
        {
            Debug.WriteLine("[ApplicationFacade] GetLatestParsedResume");
            return await db.LatestParsedResume
                .OrderByDescending(r => r.ParsedAt)
                .FirstAsync();
        }

        public async Task TailorResume(JobPosting job) 
        { 
            var resume = await GetLatestParsedResume();
            await resumeTailoringService.TailorResume(job, resume);
        }
    }
}
