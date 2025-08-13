using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CryptoApp.Services;
using System;
using System.IO;
using System.Text;
using CryptoApp.Models;
using Microsoft.Extensions.Options;

namespace CryptoApp.Pages
{
    public class EncodeModel : PageModel
    {
        private readonly CryptoService _cryptoService;
        private readonly IWebHostEnvironment _env;
        private readonly AppSettings _settings;

        public EncodeModel(CryptoService cryptoService, IWebHostEnvironment env, IOptions<AppSettings> options)
        {
            _cryptoService = cryptoService;
            _env = env;
            _settings = options.Value;
        }

        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        [BindProperty]
        public string Key { get; set; }

        [BindProperty]
        public string Algorithm { get; set; }

        public string EncryptedFilePath { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                ErrorMessage = "Morate izabrati fajl.";
                return Page();
            }

            try
            {
                // Učitavanje fajla u bajtove
                using var ms = new MemoryStream();
                UploadedFile.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();

                // Pretvaranje ključa u bajtove
                byte[] keyBytes = Encoding.UTF8.GetBytes(Key);

                // IV samo ako je XTEA-CBC
                byte[] iv = null;
                if (Algorithm == "XTEA-CBC")
                {
                    iv = new byte[8];
                    Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));
                }

                // Šifrovanje
                byte[] encryptedData = _cryptoService.Encrypt(fileBytes, Algorithm, keyBytes, iv);

                // 1️ Snimanje u encrypted folder
                var encryptedDir = Path.Combine(_env.WebRootPath, "encrypted");
                if (!Directory.Exists(encryptedDir))
                    Directory.CreateDirectory(encryptedDir);

                var fileName = Path.GetFileNameWithoutExtension(UploadedFile.FileName) + "_enc" + Path.GetExtension(UploadedFile.FileName);
                var encryptedPath = Path.Combine(encryptedDir, fileName);

                System.IO.File.WriteAllBytes(encryptedPath, encryptedData);

                // Relativna putanja za prikaz
                EncryptedFilePath = Path.Combine("encrypted", fileName).Replace("\\", "/");

                // 2️ Ako je FileWatcher uključen — snimi i u TargetDirectory
                if (_settings.IsFileExchangeEnabled && !string.IsNullOrWhiteSpace(_settings.TargetDirectory))
                {
                    if (!Directory.Exists(_settings.TargetDirectory))
                        Directory.CreateDirectory(_settings.TargetDirectory);

                    var watchedPath = Path.Combine(_settings.TargetDirectory, fileName);
                    System.IO.File.WriteAllBytes(watchedPath, encryptedData);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Došlo je do greške: " + ex.Message;
            }

            return Page();
        }

    }
}
