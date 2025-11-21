using System;
using System.Text.Json;

namespace Smart_Resume_Job_Search_App.Domain
{
    public class ParsedResume
    {
        public string sourceSha256 { get; }
        public string RawJson { get; }
        public string? Location { get; }
        public DateTime ParsedAt { get; }

        public ParsedResume(string sha256, string rawJson)
        {
            if(string.IsNullOrWhiteSpace(rawJson))
            {
                throw new ArgumentException("Raw JSON cannot be null or empty.", nameof(rawJson));
            }

            sourceSha256 = sha256;
            RawJson = rawJson;
            Location = ExtractLocation(rawJson);
            ParsedAt = DateTime.UtcNow;
        }

        private static string? ExtractLocation(string rawJson)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(rawJson);
                if (document.RootElement.TryGetProperty("location", out JsonElement locationElement))
                {
                    return locationElement.GetString();
                }
            }
            catch (JsonException)
            {
                return null;
            }

            return null;

        }

        public bool IsOutdated(ParsedResume other)
        {
            if (other is null)
            {
                return false;
            }

            return this.ParsedAt < other.ParsedAt;
        }
    }
}
