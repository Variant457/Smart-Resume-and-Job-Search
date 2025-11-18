using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;                  
using Smart_Resume_Job_Search_App.Data;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class JobSearchService
    {
        private readonly HttpClient httpClient;
        private readonly AppDbContext db;
        private readonly JobQueryBuilder queryBuilder;
        private readonly string adzunaAppId;             
        private readonly string adzunaAppKey;             

        public JobSearchService(
            HttpClient httpClient,
            AppDbContext db,
            JobQueryBuilder queryBuilder,
            IConfiguration config)                       
        {
            this.httpClient = httpClient;
            this.db = db;
            this.queryBuilder = queryBuilder;

            // ⭐ Read from appsettings.json
            adzunaAppId = config["Adzuna:AppId"] ?? "";
            adzunaAppKey = config["Adzuna:AppKey"] ?? "";

            if (string.IsNullOrWhiteSpace(adzunaAppId) || string.IsNullOrWhiteSpace(adzunaAppKey))
            {
                Debug.WriteLine("[JobSearchService] WARNING: Adzuna AppId/AppKey are not configured.");
            }
        }

        public async Task<IReadOnlyList<JobPosting>> SearchAndStoreAsync(ParsedResumeInfo parsed, CancellationToken ct = default)
        {
            Debug.WriteLine("[JobSearchService] SearchAndStoreAsync start");

            var url = queryBuilder.Build(parsed, adzunaAppId, adzunaAppKey);   // ⭐ uses config values now
            Debug.WriteLine($"[JobSearchService] URL: {url}");

            using var response = await httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<AdzunaResponse>(cancellationToken: ct);
            if (json?.results == null || json.results.Count == 0)
            {
                Debug.WriteLine("[JobSearchService] No results from Adzuna");
                return Array.Empty<JobPosting>();
            }

            var postings = new List<JobPosting>();
            foreach (var r in json.results)
            {
                var jp = new JobPosting
                {
                    JobId = r.id ?? "",
                    Title = r.title ?? "",
                    Company = r.company?.display_name ?? "",
                    Location = r.location?.display_name ?? "",
                    Description = r.description ?? "",
                    Url = r.redirect_url ?? "",
                    SalaryMin = r.salary_min,
                    SalaryMax = r.salary_max,
                    ParsedResumeInfoId = parsed.Id
                };
                postings.Add(jp);
                db.JobPostings.Add(jp);
            }

            await db.SaveChangesAsync(ct);
            Debug.WriteLine($"[JobSearchService] Stored {postings.Count} job postings");
            return postings;
        }

        #region DTOs for Adzuna
        private sealed class AdzunaResponse
        {
            public List<JobResult> results { get; set; } = new();
        }

        private sealed class JobResult
        {
            public string? id { get; set; }
            public string? title { get; set; }
            public string? description { get; set; }
            public decimal? salary_min { get; set; }
            public decimal? salary_max { get; set; }
            public string? redirect_url { get; set; }
            public CompanyInfo? company { get; set; }
            public LocationInfo? location { get; set; }
        }

        private sealed class CompanyInfo
        {
            public string? display_name { get; set; }
        }

        private sealed class LocationInfo
        {
            public string? display_name { get; set; }
        }
        #endregion
    }
}
