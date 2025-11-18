using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Smart_Resume_Job_Search_App.Data;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class JobQueryBuilder
    {
        private readonly string baseUrl = "https://api.adzuna.com/v1/api/jobs/us/search/1";

        public string Build(ParsedResumeInfo parsed, string appId, string appKey)
        {
            var query = new List<string>
            {
                $"app_id={Uri.EscapeDataString(appId)}",
                $"app_key={Uri.EscapeDataString(appKey)}",
                $"content-type={Uri.EscapeDataString("application/json")}",
                $"results_per_page=20"
            };

            var whatParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(parsed.PrimaryTitle))
                whatParts.Add(parsed.PrimaryTitle);
            if (parsed.Skills?.Any() == true)
                whatParts.AddRange(parsed.Skills.Take(6));

            if (whatParts.Any())
                query.Add($"what={Uri.EscapeDataString(string.Join(" ", whatParts))}");

            if (!string.IsNullOrWhiteSpace(parsed.Location))
                query.Add($"where={Uri.EscapeDataString(parsed.Location)}");

            var qs = string.Join("&", query);
            return $"{baseUrl}?{qs}";
        }
    }
}
