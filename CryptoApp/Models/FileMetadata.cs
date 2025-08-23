namespace CryptoApp.Models
{
    public class FileMetadata
    {
        public string Algorithm { get; set; }
        public string KeyBase64 { get; set; }
        public string OriginalFileName { get; set; }
        public int OriginalSize { get; set; }
    }
}
