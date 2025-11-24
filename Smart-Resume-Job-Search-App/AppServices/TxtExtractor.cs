using System.Diagnostics;
using System.Text;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class TxtExtractor : ITextExtractor
    {
        public bool CanExtract(Domain.InputBlob blob)
        {
            return blob.MimeType == "text/plain";
        }
        public string ExtractText(Domain.InputBlob blob)
        {            
            Debug.WriteLine($"[TxtExtractor] Extracted text from TXT blob: {blob.FileName}, Size: {blob.SizeBytes / 1024} KB");
            return Encoding.UTF8.GetString(blob.Content);
        }
    }
}
