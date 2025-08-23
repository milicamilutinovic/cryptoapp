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

        // Algoritam se sada prosleđuje kao parametar
        public async Task<string> EncryptAndSaveFileAsync(string inputFilePath, string algorithm)
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(inputFilePath);

            // Generiši key/iv
            byte[] key;
            byte[] iv = null;
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                key = new byte[16];
                rng.GetBytes(key);

                if (algorithm?.ToUpper().Contains("CBC") == true)
                {
                    iv = new byte[8];
                    rng.GetBytes(iv);
                }
            }

            // Šifrovanje
            byte[] encrypted = _cryptoService.Encrypt(fileBytes, algorithm, key, iv);

            var encryptedDir = Path.Combine(_env.WebRootPath, _settings.EncryptedFilesDirectory.TrimStart('/', '\\'));
            Directory.CreateDirectory(encryptedDir);

            string originalFileName = Path.GetFileName(inputFilePath);
            string encryptedFileName = $"{Path.GetFileNameWithoutExtension(originalFileName)}_enc_{algorithm}{Path.GetExtension(originalFileName)}";
            string encryptedPath = Path.Combine(encryptedDir, encryptedFileName);

            await File.WriteAllBytesAsync(encryptedPath, encrypted);

            // Snimi meta fajl
            var metaObj = new
            {
                OriginalFileName = originalFileName,
                Algorithm = algorithm,
                Key = Convert.ToBase64String(key),
                IV = iv != null ? Convert.ToBase64String(iv) : null,
                OriginalSize = fileBytes.Length,
                CreatedAt = DateTime.UtcNow
            };
            string metaJson = JsonSerializer.Serialize(metaObj, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(encryptedPath + ".meta", metaJson);

            return encryptedPath; // vraćamo apsolutnu putanju fajla
        }



        // Dekripcija ostaje ista, koristi meta fajl
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

            byte[] decryptedData = _cryptoService.Decrypt(encryptedData, meta.Algorithm, key, iv);

            // Trim prema originalnoj veličini
            if (meta.OriginalSize < decryptedData.Length)
            {
                byte[] trimmed = new byte[meta.OriginalSize];
                Array.Copy(decryptedData, trimmed, meta.OriginalSize);
                decryptedData = trimmed;
            }

            var decodedDir = Path.Combine(_env.WebRootPath, "decoded");
            Directory.CreateDirectory(decodedDir);

            string decodedFileName = RemoveEncSuffix(encryptedFileName);
            var decodedFilePath = Path.Combine(decodedDir, decodedFileName);

            if (!File.Exists(decodedFilePath))
                await File.WriteAllBytesAsync(decodedFilePath, decryptedData);
        }

        private static string RemoveEncSuffix(string encryptedFileName)
        {
            var ext = Path.GetExtension(encryptedFileName);
            var name = Path.GetFileNameWithoutExtension(encryptedFileName);
            int encIndex = name.IndexOf("_enc_");
            if (encIndex >= 0)
                name = name.Substring(0, encIndex);
            return name + ext;
        }
        public async Task DecryptAndSaveFileToDirectoryAsync(string encryptedFilePath, string targetDirectory, string originalFileName)
        {
            if (!File.Exists(encryptedFilePath))
                throw new FileNotFoundException("Enkriptovani fajl nije pronađen.", encryptedFilePath);

            var metaFilePath = encryptedFilePath + ".meta";
            if (!File.Exists(metaFilePath))
                throw new FileNotFoundException("Meta fajl nije pronađen.", metaFilePath);

            var metaJson = await File.ReadAllTextAsync(metaFilePath);
            var meta = JsonSerializer.Deserialize<EncryptedMeta>(metaJson);

            byte[] encryptedData = await File.ReadAllBytesAsync(encryptedFilePath);
            byte[] key = Convert.FromBase64String(meta.Key);
            byte[] iv = meta.IV != null ? Convert.FromBase64String(meta.IV) : null;

            byte[] decryptedData = _cryptoService.Decrypt(encryptedData, meta.Algorithm, key, iv);

            if (meta.OriginalSize < decryptedData.Length)
            {
                byte[] trimmed = new byte[meta.OriginalSize];
                Array.Copy(decryptedData, trimmed, meta.OriginalSize);
                decryptedData = trimmed;
            }

            Directory.CreateDirectory(targetDirectory);
            string decodedFilePath = Path.Combine(targetDirectory, originalFileName);
            await File.WriteAllBytesAsync(decodedFilePath, decryptedData);
        }


        private class EncryptedMeta
        {
            public string OriginalFileName { get; set; }
            public string Algorithm { get; set; }
            public string Key { get; set; }
            public string IV { get; set; }
            public int OriginalSize { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
