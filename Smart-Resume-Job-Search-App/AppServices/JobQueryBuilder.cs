using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Smart_Resume_Job_Search_App.Data;

namespace Smart_Resume_Job_Search_App.AppServices
{
    /// <summary>
    /// Builds Adzuna job search URLs.
    /// Core inputs we care about:
    ///   - jobTitle: one of the extracted titles (e.g. "Software Engineer")
    ///   - location: from the extractor or user input (e.g. "Seattle, WA")
    /// Optional filters:
    ///   - salaryMin, fullTime, permanent, sortBy, whatExclude
    /// </summary>
    public class JobQueryBuilder
    {
        private readonly string _baseUrl;

        /// <param name="baseUrl">
        /// Base API URL, e.g. "http://api.adzuna.com:80".
        /// </param>
        public JobQueryBuilder(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        /// <summary>
        /// Preferred overload: build from explicit parameters
        /// (one title + location + optional filters).
        /// </summary>
        public string Build(
            string appId,
            string appKey,
            string jobTitle,
            string location,
            int? salaryMin = null,
            bool? fullTime = null,
            bool? permanent = null,
            string? sortBy = "relevance",
            string? whatExclude = null,
            int resultsPerPage = 20,
            int page = 1,
            string countryCode = "us")
        {
            // Fallbacks so we never send empty values to Adzuna
            if (string.IsNullOrWhiteSpace(jobTitle))
                jobTitle = "software engineer";

            // location can be left blank – Adzuna will broaden the search –
            // but typically we’ll pass the user’s location / extracted location.
            var builder = new UriBuilder(_baseUrl)
            {
                Path = $"v1/api/jobs/{countryCode}/search/{page}"
            };

            var query = HttpUtility.ParseQueryString(builder.Query);

            query["app_id"] = appId;
            query["app_key"] = appKey;
            query["results_per_page"] = resultsPerPage.ToString(CultureInfo.InvariantCulture);
            query["what"] = jobTitle;

            if (!string.IsNullOrWhiteSpace(location))
                query["where"] = location;

            if (salaryMin.HasValue)
                query["salary_min"] = salaryMin.Value.ToString(CultureInfo.InvariantCulture);

            if (fullTime.HasValue)
                query["full_time"] = fullTime.Value ? "1" : "0";

            if (permanent.HasValue)
                query["permanent"] = permanent.Value ? "1" : "0";

            if (!string.IsNullOrWhiteSpace(sortBy))
                query["sort_by"] = sortBy;

            if (!string.IsNullOrWhiteSpace(whatExclude))
                query["what_exclude"] = whatExclude;

            builder.Query = query.ToString();

            return builder.Uri.ToString();
        }

        /// <summary>
        /// Backwards-compat overload so existing code that passes ParsedResumeInfo
        /// keeps compiling. We pick ONE title + the location from the parsed resume.
        /// can later replace this with direct calls to the explicit overload.
        /// </summary>
        public string Build(ParsedResumeInfo parsed, string appId, string appKey)
        {
            if (parsed == null)
                throw new ArgumentNullException(nameof(parsed));

            // For now, PrimaryTitle should be mapped from the first item in titles[]
            // by parsing layer.
            var jobTitle = parsed.PrimaryTitle ?? string.Empty;
            var location = parsed.Location ?? string.Empty;

            return Build(
                appId: appId,
                appKey: appKey,
                jobTitle: jobTitle,
                location: location
            // other filters left null so Adzuna uses defaults
            );
        }
    }

    public class JobPosting
    {
        public string Title { get; set; } = "";
        public string Company { get; set; } = "";
        public string Location { get; set; } = "";
        public string Url { get; set; } = "";
        public DateTime PostedDate { get; set; }
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public string Description { get; set; } = "";
    }
}
