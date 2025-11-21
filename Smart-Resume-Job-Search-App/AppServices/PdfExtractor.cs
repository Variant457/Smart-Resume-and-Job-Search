using UglyToad.PdfPig;
using System.Text;
using System.Diagnostics;
using PDFtoImage;
using Tesseract;
using SkiaSharp;
using System.Drawing;
using System.IO;

namespace Smart_Resume_Job_Search_App.AppServices
{
    public class PdfExtractor : ITextExtractor
    {
        public bool CanExtract(Domain.InputBlob blob)
        {
            return blob.MimeType == "application/pdf";
        }
        public string ExtractText(Domain.InputBlob blob)
        {
            if(blob == null) throw new ArgumentNullException(nameof(blob));

            var sb = new StringBuilder();
            using var ms  = new MemoryStream(blob.Content);

            using(var doc = PdfDocument.Open(ms))
            {
                foreach(var page in doc.GetPages())
                {
                    var pageText = page.Text;
                    if(!string.IsNullOrWhiteSpace(pageText))
                    {
                        sb.AppendLine(pageText);
                        sb.AppendLine(); 
                    }
                }
            }

            if(sb.Length == 0)
            {
                Debug.WriteLine($"[PdfExtractor] No text extracted from PDF, attempting to extract with image-based text extractor");
                
                var strBuilder = new StringBuilder();
                var pages = Conversion.ToImages(blob.Content);

                var tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
                using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);

                foreach(var skPage in pages)
                {
                    using var bitmap = SkiaToBitmap(skPage);
                    using var pix = PixConverter.ToPix(bitmap);
                    using var page = engine.Process(pix);

                    strBuilder.AppendLine(page.GetText());
                    strBuilder.AppendLine();
                }

                Debug.WriteLine($"[PdfExtractor] Extracted {strBuilder.Length} characters from image-based PDF blob: {blob.FileName}, Size: {blob.SizeBytes / 1024} KB");
                return strBuilder.ToString();
            } else
            {
                Debug.WriteLine($"[PdfExtractor] Extracted {sb.Length} characters from PDF blob: {blob.FileName}, Size: {blob.SizeBytes / 1024} KB");
                return sb.ToString();
            }
        }

        private static Bitmap SkiaToBitmap(SKBitmap skBitmap)
        {
            using var image = SKImage.FromBitmap(skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Position = 0;

            return new Bitmap(ms);
        }
    }
}
