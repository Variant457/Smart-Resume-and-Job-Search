using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Smart_Resume_Job_Search_App.Domain;

namespace Smart_Resume_Job_Search_App.AI
{
 // DTO to represent extracted entities from resumes
 public sealed record ExtractedEntity(string Label, string Text, int StartIndex);

 // C# wrapper registered in DI. It can later be wired to the Python implementation (ParsingModel.py)
 public sealed class ResumeParser
 {
    private readonly ParsingAdapter adapter;

    public ResumeParser(ParsingAdapter adapter)
    {
        this.adapter = adapter;
        Debug.WriteLine("[ResumeParser] Initialized (C# wrapper with Python bridge)");
    }

    // First try Python bridge. If unavailable, fallback to minimal in-proc extraction
    public async Task<IReadOnlyList<ExtractedEntity>> ParseAsync(byte[] data)
    {
        string text = data != null ? System.Text.Encoding.UTF8.GetString(data) : string.Empty;

        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<ExtractedEntity>();

        // Attempt Python adapter on desktop
        var entities = await adapter.TryParseAsync(text);
        if (entities.Count > 0) return entities;

        // Fallback stub so the app remains functional
        var sample = new List<ExtractedEntity>();
        if (text.Contains("Experience", StringComparison.OrdinalIgnoreCase))
        {
            sample.Add(new ExtractedEntity("SECTION", "Experience", text.IndexOf("Experience", StringComparison.OrdinalIgnoreCase)));
        }
        if (text.Contains("Education", StringComparison.OrdinalIgnoreCase))
        {
            sample.Add(new ExtractedEntity("SECTION", "Education", text.IndexOf("Education", StringComparison.OrdinalIgnoreCase)));
        }

        return new ReadOnlyCollection<ExtractedEntity>(sample);
    }
 }
}
