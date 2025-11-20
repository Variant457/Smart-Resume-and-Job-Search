using System;
using System.Collections.Generic;
using System.Globalization;
using Smart_Resume_Job_Search_App.Data;

namespace Smart_Resume_Job_Search_App.AppServices
{
    /// Builds Adzuna job search URLs.
    public class JobQueryBuilder
    {
        private readonly string _baseUrl;

        /// baseUrl e.g. "http://api.adzuna.com:80"
        public JobQueryBuilder(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

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
            int resultsPerPage = 5,
            int page = 1,
            string countryCode = "us")
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentException("Adzuna appId is required", nameof(appId));
            if (string.IsNullOrWhiteSpace(appKey))
                throw new ArgumentException("Adzuna appKey is required", nameof(appKey));

            if (string.IsNullOrWhiteSpace(jobTitle))
                jobTitle = "software engineer";

            var queryParts = new List<string>();

            void Add(string key, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var encoded = Uri.EscapeDataString(value);
                    queryParts.Add($"{key}={encoded}");
                }
            }

            Add("app_id", appId);
            Add("app_key", appKey);
            Add("results_per_page", resultsPerPage.ToString(CultureInfo.InvariantCulture));
            Add("what", jobTitle);

            if (!string.IsNullOrWhiteSpace(location))
                Add("where", location);

            if (salaryMin.HasValue)
                Add("salary_min", salaryMin.Value.ToString(CultureInfo.InvariantCulture));

            if (fullTime.HasValue)
                Add("full_time", fullTime.Value ? "1" : "0");

            if (permanent.HasValue)
                Add("permanent", permanent.Value ? "1" : "0");

            if (!string.IsNullOrWhiteSpace(sortBy))
                Add("sort_by", sortBy);

            if (!string.IsNullOrWhiteSpace(whatExclude))
                Add("what_exclude", whatExclude);

            var path = $"/v1/api/jobs/{countryCode}/search/{page}";
            var queryString = string.Join("&", queryParts);

            return $"{_baseUrl}{path}?{queryString}";
        }

        /// Overload for ParsedResumeInfo (one title + location).
        public string Build(ParsedResumeInfo parsed, string appId, string appKey)
        {
            if (parsed == null)
                throw new ArgumentNullException(nameof(parsed));

            var jobTitle = parsed.PrimaryTitle ?? string.Empty;
            var location = parsed.Location ?? string.Empty;

            return Build(
                appId: appId,
                appKey: appKey,
                jobTitle: jobTitle,
                location: location
            );
        }
    }
}
