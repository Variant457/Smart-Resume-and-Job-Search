using System.Web;

public class JobQueryBuilder
{
    private readonly string baseUrl = "https://api.adzuna.com/v1/api/jobs/us/search/1";

    public string Build(ParsedResumeInfo parsed, string appId, string appKey)
    {
        var qs = HttpUtility.ParseQueryString(string.Empty);
        qs["app_id"] = appId;
        qs["app_key"] = appKey;
        qs["content-type"] = "application/json";
        qs["results_per_page"] = "20";

        var whatParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(parsed.PrimaryTitle)) whatParts.Add(parsed.PrimaryTitle);
        if (parsed.Skills?.Any() == true) whatParts.AddRange(parsed.Skills.Take(6));
        if (whatParts.Any()) qs["what"] = string.Join(" ", whatParts);

        if (!string.IsNullOrWhiteSpace(parsed.Location)) qs["where"] = parsed.Location;

        return $"{baseUrl}?{qs}";
    }
}
