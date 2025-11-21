using Microsoft.EntityFrameworkCore;

namespace Smart_Resume_Job_Search_App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<ParsedResumeRecord> LatestParsedResume { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<TailoredResume> TailoredResumes { get; set; }
        public DbSet<OptimizationResult> OptimizationResults { get; set; }
    }
}
