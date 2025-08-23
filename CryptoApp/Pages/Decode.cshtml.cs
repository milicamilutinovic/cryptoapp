using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CryptoApp.Services;
using CryptoApp.Models;
using System.Text.Json;

namespace CryptoApp.Pages
{
    public class DecodeModel : PageModel
    {
        private readonly CryptoService _cryptoService;
        private readonly IWebHostEnvironment _env;

        public DecodeModel(CryptoService cryptoService, IWebHostEnvironment env)
        {
            _cryptoService = cryptoService;
            _env = env;
        }

        [BindProperty]
        public string SelectedFile { get; set; }

        public List<string> EncryptedFiles { get; set; } = new();
        public string DecryptedFilePath { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet() => LoadEncryptedFiles();

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(SelectedFile))
            {
                ErrorMessage = "Morate izabrati fajl.";
                LoadEncryptedFiles();
                return Page();
            }

            try
            {
                var encryptedDir = Path.Combine(_env.WebRootPath, "encrypted");
                var encryptedFilePath = Path.Combine(encryptedDir, SelectedFile);

                if (!System.IO.File.Exists(encryptedFilePath))
                {
                    ErrorMessage = "Fajl ne postoji.";
                    LoadEncryptedFiles();
                    return Page();
                }

                // ucitavanje metapodataka
                var jsonDir = Path.Combine(_env.WebRootPath, "encryptedjson");
                var metadataPath = Path.Combine(jsonDir, Path.GetFileNameWithoutExtension(SelectedFile) + ".json");

                if (!System.IO.File.Exists(metadataPath))
                {
                    ErrorMessage = "Metapodaci za fajl ne postoje.";
                    LoadEncryptedFiles();
                    return Page();
                }

                var metadata = JsonSerializer.Deserialize<FileMetadata>(System.IO.File.ReadAllText(metadataPath));
                byte[] keyBytes = Convert.FromBase64String(metadata.KeyBase64);
                byte[] iv = null;
                if (metadata.Algorithm == "XTEA-CBC")
                {
                    iv = new byte[8];
                    Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));
                }

                // ucitavanje sifrovanih bajtova i dekriptovanje
                byte[] encryptedBytes = System.IO.File.ReadAllBytes(encryptedFilePath);
                byte[] decrypted = _cryptoService.Decrypt(encryptedBytes, metadata.Algorithm, keyBytes, iv, metadata.OriginalSize);

                // snimanje dekriptovanog fajla
                var saveDir = Path.Combine(_env.WebRootPath, "decrypted");
                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);

                var decryptedFileName = Path.GetFileNameWithoutExtension(SelectedFile)
                                        .Replace("_enc_" + metadata.Algorithm, "")
                                        + "_dec" + Path.GetExtension(metadata.OriginalFileName);

                var decryptedFilePath = Path.Combine(saveDir, decryptedFileName);
                System.IO.File.WriteAllBytes(decryptedFilePath, decrypted);

                DecryptedFilePath = Path.Combine("decrypted", decryptedFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Greška pri dešifrovanju: " + ex.Message;
            }

            LoadEncryptedFiles();
            return Page();
        }

        private void LoadEncryptedFiles()
        {
            var encryptedDir = Path.Combine(_env.WebRootPath, "encrypted");
            if (Directory.Exists(encryptedDir))
            {
                // ucitavanje svih fajlova u enc
                EncryptedFiles = Directory.GetFiles(encryptedDir)
                    .Select(Path.GetFileName)
                    .Where(f => f.Contains("_enc_"))
                    .ToList();
            }
            else
            {
                EncryptedFiles = new List<string>();
            }
        }
    }
}
