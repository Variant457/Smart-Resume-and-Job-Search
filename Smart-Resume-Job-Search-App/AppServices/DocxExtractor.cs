using System.Diagnostics;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class DocxExtractor : ITextExtractor
    {
        public bool CanExtract(Domain.InputBlob blob)
        {
            return blob.MimeType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        }

        public string ExtractText(Domain.InputBlob blob)
        {
            if (blob == null) throw new ArgumentNullException(nameof(blob));

            using var ms = new MemoryStream(blob.Content);
            using var wordDoc = WordprocessingDocument.Open(ms, false);

            var mainPart = wordDoc.MainDocumentPart;
            if(mainPart == null || mainPart.Document?.Body == null)
            {
                Debug.WriteLine($"[DocxExtractor] No MainDocumentPart found in DOCX blob: {blob.FileName}");
                return string.Empty;
            }

            var body = mainPart.Document.Body;
            var sb = new StringBuilder();

            foreach (var para in body.Elements<Paragraph>())
            {
                var text = para.InnerText;
                if(!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                }
            }

            Debug.WriteLine($"[DocxExtractor] Extracted {sb.Length} characters from DOCX blob: {blob.FileName}, Size: {blob.SizeBytes / 1024} KB");
            return sb.ToString();
        }
    }
}
