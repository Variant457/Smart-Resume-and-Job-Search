using System;

namespace Smart_Resume_Job_Search_App.Data
{
    public class InputResume
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string RawText { get; set; } = "";
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    public class ParsedResumeInfo
    {
        public int Id { get; set; }
        public int InputResumeId { get; set; }
        public InputResume? InputResume { get; set; }

        public string? PrimaryTitle { get; set; }
        public string? Location { get; set; }
        public string[]? Skills { get; set; }
        public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
    }

    public class JobPosting
    {
        public int Id { get; set; }
        public string JobId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Company { get; set; } = "";
        public string Location { get; set; } = "";
        public string Description { get; set; } = "";
        public string Url { get; set; } = "";
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ParsedResumeInfoId { get; set; }
        public ParsedResumeInfo? ParsedResumeInfo { get; set; }
    }

    public class TailoredResume
    {
        public int Id { get; set; }
        public int ParsedResumeInfoId { get; set; }
        public ParsedResumeInfo? ParsedResumeInfo { get; set; }
        public string JsonResult { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class OptimizationResult
    {
        public int Id { get; set; }
        public int TailoredResumeId { get; set; }
        public TailoredResume? TailoredResume { get; set; }
        public string ResultJson { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
