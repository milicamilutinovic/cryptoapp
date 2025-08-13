using CryptoApp.Models;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CryptoApp.Services
{
    public class EncryptionHelper
    {
        private readonly CryptoService _cryptoService;
        private readonly AppSettings _settings;
        private readonly IWebHostEnvironment _env;

        public EncryptionHelper(CryptoService cryptoService, IOptions<AppSettings> options, IWebHostEnvironment env)
        {
            _cryptoService = cryptoService;
            _settings = options.Value;
            _env = env;
        }

        public async Task EncryptAndSaveFileAsync(string inputFilePath)
        {
            // učitaj original
            byte[] fileBytes = await File.ReadAllBytesAsync(inputFilePath);

            // generiši key/iv
            byte[] key;
            byte[] iv = null;
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                key = new byte[16];
                rng.GetBytes(key);
                if (_settings.SelectedEncryptionAlgorithm?.ToUpper().Contains("CBC") == true)
                {
                    iv = new byte[8];
                    rng.GetBytes(iv);
                }
            }

            // enkripcija
            byte[] encrypted = _cryptoService.Encrypt(fileBytes, _settings.SelectedEncryptionAlgorithm, key, iv);

            // destinacija u wwwroot/X (EncryptedFilesDirectory)
            var encryptedDir = Path.Combine(_env.WebRootPath, _settings.EncryptedFilesDirectory.TrimStart('/', '\\'));
            Directory.CreateDirectory(encryptedDir);

            // ime sa sufiksom _enc
            string originalFileName = Path.GetFileName(inputFilePath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            string ext = Path.GetExtension(originalFileName);
            string encryptedFileName = $"{fileNameWithoutExt}_enc{ext}";

            string encryptedPath = Path.Combine(encryptedDir, encryptedFileName);

            // Ako već postoji enkriptovana verzija - preskoči (izbegava duplo enkriptovanje)
            if (File.Exists(encryptedPath))
                return;

            await File.WriteAllBytesAsync(encryptedPath, encrypted);

            var metaObj = new
            {
                OriginalFileName = originalFileName,
                Algorithm = _settings.SelectedEncryptionAlgorithm,
                Key = Convert.ToBase64String(key),
                IV = iv != null ? Convert.ToBase64String(iv) : null,
                CreatedAt = DateTime.UtcNow
            };

            string metaJson = JsonSerializer.Serialize(metaObj, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(encryptedPath + ".meta", metaJson);
        }

        public byte[] Decrypt(byte[] data, string algorithmName, byte[] key, byte[] iv = null)
        {
            return _cryptoService.Decrypt(data, algorithmName, key, iv);
        }

        public async Task DecryptAndSaveFileAsync(string encryptedFileName)
        {
            var encryptedDir = Path.Combine(_env.WebRootPath, _settings.EncryptedFilesDirectory.TrimStart('/', '\\'));
            var encryptedFilePath = Path.Combine(encryptedDir, encryptedFileName);

            if (!File.Exists(encryptedFilePath))
                throw new FileNotFoundException("Enkriptovani fajl nije pronađen.", encryptedFileName);

            var metaFilePath = encryptedFilePath + ".meta";
            if (!File.Exists(metaFilePath))
                throw new FileNotFoundException("Meta fajl nije pronađen za enkriptovani fajl.", metaFilePath);

            var metaJson = await File.ReadAllTextAsync(metaFilePath);
            var meta = JsonSerializer.Deserialize<EncryptedMeta>(metaJson);

            byte[] encryptedData = await File.ReadAllBytesAsync(encryptedFilePath);
            byte[] key = Convert.FromBase64String(meta.Key);
            byte[] iv = meta.IV != null ? Convert.FromBase64String(meta.IV) : null;

            byte[] decryptedData = Decrypt(encryptedData, meta.Algorithm, key, iv);

            var decodedDir = Path.Combine(_env.WebRootPath, "decoded");
            Directory.CreateDirectory(decodedDir);

            // izbaci samo tačan _enc suffix pre ekstenzije (bez grešaka)
            string decodedFileName = RemoveEncSuffix(encryptedFileName);
            var decodedFilePath = Path.Combine(decodedDir, decodedFileName);

            // Ako već postoji decoded fajl — preskoči (ne prepisuj)
            if (File.Exists(decodedFilePath))
                return;

            await File.WriteAllBytesAsync(decodedFilePath, decryptedData);
        }

        private static string RemoveEncSuffix(string encryptedFileName)
        {
            var ext = Path.GetExtension(encryptedFileName);
            var name = Path.GetFileNameWithoutExtension(encryptedFileName);
            if (name.EndsWith("_enc"))
                name = name.Substring(0, name.Length - 4);
            return name + ext;
        }

        private class EncryptedMeta
        {
            public string OriginalFileName { get; set; }
            public string Algorithm { get; set; }
            public string Key { get; set; }
            public string IV { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
