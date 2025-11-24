

namespace Smart_Resume_Job_Search_App.Data
{    
    /// Normalised representation of the resume that the AI / parser produces.
    public class ParsedResumeRecord
    {
        public int Id { get; set; }
        public string SourceSha256 { get; set; } = null!;
        public string? Location { get; set; } = null!;
        public string RawJson { get; set; }
        public string origMimeType { get; set; }
        public DateTime ParsedAt { get; set; }
    }

    /// A single job returned from Adzuna.
    public class JobPosting
    {
        public int Id { get; set; }

        /// External Adzuna job id.
        public string JobId { get; set; } = string.Empty;

        public int ParsedResumeInfoId { get; set; }
        public ParsedResumeRecord? ParsedResumeRecord { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public DateTime? PostedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// A tailored resume artifact for a specific job or job set.
    public class TailoredResume
    {
        public int Id { get; set; }

        public int ParsedResumeInfoId { get; set; }
        public ParsedResumeRecord? ParsedResumeRecord { get; set; }

        public int JobPostingId { get; set; }
        public JobPosting? JobPosting { get; set; }

        /// Where we stored this tailored resume (file name / path).
        public string FileName { get; set; } = string.Empty;

        /// AI-produced JSON representation, if want to keep it.
        public string JsonResult { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public OptimizationResult? OptimizationResult { get; set; }
    }

    /// Result from an optimisation / scoring pass comparing resume to a job.
    public class OptimizationResult
    {
        public int Id { get; set; }

        public int TailoredResumeId { get; set; }
        public TailoredResume? TailoredResume { get; set; }

        public string ResultJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
