using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace Smart_Resume_Job_Search_App.AI
{
 // DTO to represent extracted entities from resumes
 public sealed record ExtractedEntity(string Label, string Text, int StartIndex);

 public interface IResumeParser
 {
    Task<IReadOnlyList<ExtractedEntity>> ParseAsync(string text);
 }

 // C# wrapper registered in DI. It can later be wired to the Python implementation (ParsingModel.py)
 public sealed class ResumeParser : IResumeParser
 {
     public ResumeParser()
     {
        Debug.WriteLine("[ResumeParser] Initialized (C# wrapper)");
     }

 // Placeholder implementation. Replace with a bridge to Python (pythonnet, gRPC, or subprocess) when ready.
     public Task<IReadOnlyList<ExtractedEntity>> ParseAsync(string text)
     {
         if (string.IsNullOrWhiteSpace(text))
         return Task.FromResult<IReadOnlyList<ExtractedEntity>>(Array.Empty<ExtractedEntity>());

         // Minimal fake extraction so the app can be wired end-to-end before the Python bridge is added
         var sample = new List<ExtractedEntity>();
         if (text.Contains("Experience", StringComparison.OrdinalIgnoreCase))
         sample.Add(new ExtractedEntity("SECTION", "Experience", text.IndexOf("Experience", StringComparison.OrdinalIgnoreCase)));
         if (text.Contains("Education", StringComparison.OrdinalIgnoreCase))
         sample.Add(new ExtractedEntity("SECTION", "Education", text.IndexOf("Education", StringComparison.OrdinalIgnoreCase)));

         return Task.FromResult<IReadOnlyList<ExtractedEntity>>(new ReadOnlyCollection<ExtractedEntity>(sample));
     }
 }
}
