using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Smart_Resume_Job_Search_App.Data;
using System.Text.Json;
using System.Linq;

namespace Smart_Resume_Job_Search_App.AppServices
{
    /// Wraps calls to the Adzuna API and persists the resulting job postings.
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
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _queryBuilder = queryBuilder ?? throw new ArgumentNullException(nameof(queryBuilder));

            _adzunaAppId = config["Adzuna:AppId"] ?? string.Empty;
            _adzunaAppKey = config["Adzuna:AppKey"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_adzunaAppId) || string.IsNullOrWhiteSpace(_adzunaAppKey))
            {
                Debug.WriteLine("[JobSearchService] WARNING: Adzuna AppId/AppKey are not configured.");
            }
        }

        /// Searches Adzuna based on the parsed resume and stores the results.
        public async Task<IReadOnlyList<JobPosting>> SearchAndStoreAsync(
            ParsedResumeRecord parsed,
            CancellationToken ct = default)
        {
            if (parsed == null) throw new ArgumentNullException(nameof(parsed));

            Debug.WriteLine("[JobSearchService] SearchAndStoreAsync start");

            if (string.IsNullOrWhiteSpace(_adzunaAppId) || string.IsNullOrWhiteSpace(_adzunaAppKey))
            {
                Debug.WriteLine("[JobSearchService] Aborting search – Adzuna credentials are missing.");
                return Array.Empty<JobPosting>();
            }

            var titles = GetRecommendedJobTitles(parsed);
            Debug.WriteLine($"[JobSearchService] Recommended job titles count: {titles.Count}");

            if (titles.Count == 0)
            {
                Debug.WriteLine("[JobSearchService] No recommended job titles found in parsed resume.");
                return Array.Empty<JobPosting>();
            }

            var allPostings = new List<JobPosting>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var title in titles)
            {
                var url = _queryBuilder.Build(parsed, _adzunaAppId, _adzunaAppKey, title);
                Debug.WriteLine($"[JobSearchService] Querying Adzuna for title: {title}");

                using var response = await _httpClient.GetAsync(url, ct);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                var json = await JsonSerializer.DeserializeAsync<AdzunaResponse>(stream, cancellationToken: ct);

                if (json?.results == null || json.results.Count == 0)
                {
                    Debug.WriteLine($"[JobSearchService] No Results found for {title}");
                    return Array.Empty<JobPosting>();
                }

                foreach (var result in json.results)
                {
                    var jobId = result.id;
                    if (!seenIds.Add(jobId)) continue;

                    var jp = new JobPosting
                    {
                        JobId = jobId,
                        Title = result.title ?? string.Empty,
                        Company = result.company?.display_name ?? string.Empty,
                        Location = result.location?.display_name ?? string.Empty,
                        Description = result.description ?? string.Empty,
                        Url = result.redirect_url ?? string.Empty,
                        SalaryMin = result.salary_min,
                        SalaryMax = result.salary_max,
                        PostedDate = result.created,
                        ParsedResumeInfoId = parsed.Id
                    };

                    allPostings.Add(jp);
                    _db.JobPostings.Add(jp);
                }
            }

            await _db.SaveChangesAsync(ct);
            Debug.WriteLine($"[JobSearchService] Stored {allPostings.Count} Job Postings");
            return allPostings;
        }

        private sealed class ParsedResumeForSearch
        {
            public List<string>? recommended_job_titles { get; set; }
        }

        private static List<string> GetRecommendedJobTitles(ParsedResumeRecord parsed)
        {
            if (string.IsNullOrWhiteSpace(parsed.RawJson))
                return new List<string>();

            ParsedResumeForSearch? parsedData = null;
            try
            {
                parsedData = JsonSerializer.Deserialize<ParsedResumeForSearch>(parsed.RawJson);
            }
            catch
            {
                Debug.WriteLine("[JobSearchService] Failed to parse recommended job titles from ParsedDataJson");
                return new List<string>();
            }

            return parsedData?.recommended_job_titles?
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Select(title => title.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .ToList() ?? new List<string>();
        }

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

            public DateTime? created { get; set; }
        }

        private sealed class CompanyInfo
        {
            public string? display_name { get; set; }
        }

        private sealed class LocationInfo
        {
            public string? display_name { get; set; }
        }
    }
}
