using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Smart_Resume_Job_Search_App.Data;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class JobSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;
        private readonly JobQueryBuilder _queryBuilder;
        private readonly string _adzunaAppId;
        private readonly string _adzunaAppKey;

        public JobSearchService(
            HttpClient httpClient,
            AppDbContext db,
            JobQueryBuilder queryBuilder,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _db = db;
            _queryBuilder = queryBuilder;

            // From appsettings.json
            _adzunaAppId = config["Adzuna:AppId"] ?? string.Empty;
            _adzunaAppKey = config["Adzuna:AppKey"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_adzunaAppId) ||
                string.IsNullOrWhiteSpace(_adzunaAppKey))
            {
                Debug.WriteLine("[JobSearchService] WARNING: Adzuna AppId/AppKey are not configured.");
            }
        }

        public async Task<IReadOnlyList<JobPosting>> SearchAndStoreAsync(
            ParsedResumeInfo parsed,
            CancellationToken ct = default)
        {
            Debug.WriteLine("[JobSearchService] SearchAndStoreAsync start");

            if (string.IsNullOrWhiteSpace(_adzunaAppId) ||
                string.IsNullOrWhiteSpace(_adzunaAppKey))
            {
                Debug.WriteLine("[JobSearchService] Missing Adzuna credentials – skipping search.");
                return Array.Empty<JobPosting>();
            }

            // Build URL from parsed resume (uses one title + location)
            var url = _queryBuilder.Build(parsed, _adzunaAppId, _adzunaAppKey);
            Debug.WriteLine($"[JobSearchService] URL: {url}");

            using var response = await _httpClient.GetAsync(url, ct);
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
                    JobId = r.id ?? string.Empty,
                    Title = r.title ?? string.Empty,
                    Company = r.company?.display_name ?? string.Empty,
                    Location = r.location?.display_name ?? string.Empty,
                    Description = r.description ?? string.Empty,
                    Url = r.redirect_url ?? string.Empty,
                    SalaryMin = r.salary_min,
                    SalaryMax = r.salary_max,
                    ParsedResumeInfoId = parsed.Id
                };

                postings.Add(jp);
                _db.JobPostings.Add(jp);
            }

            await _db.SaveChangesAsync(ct);
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
