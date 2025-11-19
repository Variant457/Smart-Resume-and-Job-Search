using Smart_Resume_Job_Search_App.Domain;
using Smart_Resume_Job_Search_App.Data;
using System.Diagnostics;
using System.Linq;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class ApplicationFacade
    {
        private readonly AppWorkflow _workflow;
        private readonly AppDbContext _db;

        public ApplicationFacade(AppWorkflow workflow, AppDbContext db)
        {
            _workflow = workflow;
            _db = db;
        }

        public bool HasPreviousResume()
        {
            Debug.WriteLine("[ApplicationFacade] HasPreviousResume");

            try
            {
                // Adjust this to whatever makes sense:
                // e.g., check if any resumes have ever been saved
                return _db.InputResumes.Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApplicationFacade] HasPreviousResume error: {ex}");
                return false;
            }
        }

        public async Task UploadResume(InputBlob input)
        {
            Debug.WriteLine($"[ApplicationFacade] UploadResume: {input?.FileName} size={input?.Content?.Length}");

            // Transition to parsing state with input (your existing behaviour)
            _workflow.SetState(input);

            // Example: save basic info about the upload to the DB
            // TODO: Adjust property names to match your InputResume model
            /*
            var record = new InputResume
            {
                FileName = input.FileName,
                UploadedAt = DateTime.UtcNow,
                // ...any other properties we have
            };

            _db.InputResumes.Add(record);
            await _db.SaveChangesAsync();
            */

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
