using System.IO.Compression;
using System.Xml.Linq;

namespace Smart_Resume_Job_Search_App.Domain.TextExtraction
{
    public sealed class DocxTextExtractor : ITextExtractor
    {
        public bool CanExtract(InputBlob input)
        {
            if (input == null) return false;
            var ext = Path.GetExtension(input.FileName ?? string.Empty).ToLowerInvariant();
            var mime = input.MimeType?.ToLowerInvariant();
            return mime == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" || ext == ".docx";
        }

        public Task<string> ExtractTextAsync(InputBlob input, CancellationToken cancellationToken = default)
        {
            if (input.Content == null || input.Content.Length == 0)
                return Task.FromResult(string.Empty);

            using var stream = new MemoryStream(input.Content, writable: false);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
            var entry = archive.GetEntry("word/document.xml");
            if (entry == null)
                return Task.FromResult(string.Empty);

            using var entryStream = entry.Open();
            var xdoc = XDocument.Load(entryStream);
            XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

            var textNodes = xdoc.Descendants(w + "t");
            var pieces = textNodes.Select(t => (string)t);
            var text = string.Join(" ", pieces);
            return Task.FromResult(text);
        }
    }
}
