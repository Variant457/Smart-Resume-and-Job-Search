using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Smart_Resume_Job_Search_App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<InputResume> InputResumes { get; set; }
        public DbSet<ParsedResumeInfo> ParsedResumeInfos { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<TailoredResume> TailoredResumes { get; set; }
        public DbSet<OptimizationResult> OptimizationResults { get; set; }
    }
}
