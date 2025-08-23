using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CryptoApp.Services;
using CryptoApp.Models;
using System.Text.Json;
using System.Security.Cryptography;

namespace CryptoApp.Pages
{
    public class EncodeModel : PageModel
    {
        private readonly CryptoService _cryptoService;
        private readonly IWebHostEnvironment _env;

        public EncodeModel(CryptoService cryptoService, IWebHostEnvironment env)
        {
            _cryptoService = cryptoService;
            _env = env;
        }

        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        [BindProperty]
        public string Algorithm { get; set; }

        public string EncryptedFilePath { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                ErrorMessage = "Morate izabrati fajl.";
                return Page();
            }

            try
            {
                // ucitavanje fajla
                byte[] fileBytes;
                using (var ms = new MemoryStream())
                {
                    UploadedFile.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }

                // generisanje slučajnog ključa (16 bajtova)
                byte[] keyBytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(keyBytes);
                }

                // IV samo za XTEA-CBC
                byte[] iv = null;
                if (Algorithm == "XTEA-CBC")
                {
                    iv = new byte[8];
                    Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));
                }

                // sifrovanje
                byte[] encryptedData = _cryptoService.Encrypt(fileBytes, Algorithm, keyBytes, iv);

                // snimanje šifrovanog fajla
                var encryptedDir = Path.Combine(_env.WebRootPath, "encrypted");
                if (!Directory.Exists(encryptedDir))
                    Directory.CreateDirectory(encryptedDir);

                // dodaj algoritam u ime fajla da se ne prepisuje
                var fileName = Path.GetFileNameWithoutExtension(UploadedFile.FileName)
                               + "_enc_" + Algorithm + Path.GetExtension(UploadedFile.FileName);

                var encryptedPath = Path.Combine(encryptedDir, fileName);
                System.IO.File.WriteAllBytes(encryptedPath, encryptedData);
                EncryptedFilePath = Path.Combine("encrypted", fileName).Replace("\\", "/");

                // snimanje metapodataka u posebnu mapu
                var metadata = new FileMetadata
                {
                    Algorithm = Algorithm,
                    KeyBase64 = Convert.ToBase64String(keyBytes),
                    OriginalFileName = UploadedFile.FileName,
                    OriginalSize = fileBytes.Length
                };

                var jsonDir = Path.Combine(_env.WebRootPath, "encryptedjson");
                if (!Directory.Exists(jsonDir))
                    Directory.CreateDirectory(jsonDir);

                var metadataPath = Path.Combine(jsonDir, Path.GetFileNameWithoutExtension(fileName) + ".json");
                System.IO.File.WriteAllText(metadataPath, JsonSerializer.Serialize(metadata));

                return RedirectToPage("/Decode");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Došlo je do greške: " + ex.Message;
                return Page();
            }
        }
    }
}
