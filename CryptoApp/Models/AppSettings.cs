namespace CryptoApp.Models
{
    public class AppSettings
    {
        // Putanja do foldera u koji se smeštaju kodirani fajlovi
        public string EncryptedFilesDirectory { get; set; }

        // Putanja do foldera koji FileSystemWatcher prati za nove fajlove
        public string TargetDirectory { get; set; }

        // Da li je FileSystemWatcher uključen ili ne
        public bool IsFileWatcherEnabled { get; set; }

        // Da li je razmena fajlova preko TCP aktivirana
        public bool IsFileExchangeEnabled { get; set; }

        // Izabrani algoritam za kodiranje (npr. "RC4", "XTEA", "XTEA-CBC")
        public string SelectedEncryptionAlgorithm { get; set; }

        // Izabrani heš algoritam (npr. "Blake2b")
        public string SelectedHashAlgorithm { get; set; }

        
    }
}
