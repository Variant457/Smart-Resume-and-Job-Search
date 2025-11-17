using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
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

         // Preferred entry: accept InputBlob so we can extract text from PDFs or other formats
         public async Task<IReadOnlyList<ExtractedEntity>> ParseAsync(InputBlob input)
         {
             if (input == null || input.Content == null || input.Content.Length ==0)
             return Array.Empty<ExtractedEntity>();

             var text = ExtractText(input);
             if (string.IsNullOrWhiteSpace(text))
             {
                 Debug.WriteLine("[ResumeParser] No textual content extracted.");
                 return Array.Empty<ExtractedEntity>();
             }

             // Attempt Python adapter on desktop
             var entities = await adapter.TryParseAsync(text);
             if (entities.Count >0) return entities;

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

         // Back-compat: treat bytes as UTF-8 text
         public async Task<IReadOnlyList<ExtractedEntity>> ParseAsync(byte[] data)
         {
             string text = data != null ? Encoding.UTF8.GetString(data) : string.Empty;
             if (string.IsNullOrWhiteSpace(text)) return Array.Empty<ExtractedEntity>();

             var entities = await adapter.TryParseAsync(text);
             if (entities.Count >0) return entities;

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

         private static string ExtractText(InputBlob input)
         {
             try
             {
                 var mime = input.MimeType?.ToLowerInvariant() ?? string.Empty;
                 var ext = Path.GetExtension(input.FileName ?? string.Empty).ToLowerInvariant();

                 if (mime == "application/pdf" || ext == ".pdf")
                 {
                     Debug.WriteLine("[ResumeParser] PDF detected. No PDF text extractor configured. Skipping text extraction.");
                     return string.Empty;
                 }

                 if (mime == "text/plain" || ext == ".txt")
                 {
                     // Assume UTF-8 for now; could apply BOM sniffing if needed
                     return Encoding.UTF8.GetString(input.Content);
                 }

                 // Minimal default: try UTF-8 decode
                 return Encoding.UTF8.GetString(input.Content);
             }
             catch (Exception ex)
             {
                 Debug.WriteLine($"[ResumeParser] ExtractText failed: {ex}");
                 return string.Empty;
             }
         }
     }
}
