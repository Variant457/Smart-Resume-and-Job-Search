

namespace Smart_Resume_Job_Search_App.Domain
{
    public class InputBlob
    {
        public string FileName { get; }
        public string MimeType { get; }
        public byte[] Content { get; }
        public long SizeBytes => Content.LongLength;
        public string Sha256 { get; }
        public DateTime CreatedUtc { get; }
        
        public InputBlob(string fileName, string mimeType, byte[] content, string sha256)
        {
            if(string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("FileName cannot be null or whitespace.", nameof(fileName));
            if(string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("MimeType cannot be null or whitespace.", nameof(mimeType));
            if(content == null || content.Length == 0)
                throw new ArgumentException("Content cannot be null or empty.", nameof(content));

            FileName = fileName;
            MimeType = mimeType;
            Content = content;
            Sha256 = sha256;
            CreatedUtc = DateTime.UtcNow;
        }
    }
}
