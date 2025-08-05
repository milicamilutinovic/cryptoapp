using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CryptoApp.Services;
using System;
using System.IO;
using System.Text;

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
        public IFormFile UploadedFile { get; set; }

        [BindProperty]
        public string Key { get; set; }

        [BindProperty]
        public string Algorithm { get; set; }

        public string DecryptedFilePath { get; set; }
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
                byte[] encryptedBytes = ms.ToArray();

                // Priprema ključa i IV ako treba
                byte[] keyBytes = Encoding.UTF8.GetBytes(Key);
                byte[] iv = null;

                if (Algorithm == "XTEA-CBC")
                {
                    iv = new byte[8];
                    Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));
                }

                // Dešifrovanje fajla
                byte[] decrypted = _cryptoService.Decrypt(encryptedBytes, Algorithm, keyBytes, iv);

                // Kreiranje direktorijuma ako ne postoji
                var saveDir = Path.Combine(_env.WebRootPath, "decrypted");
                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);

                // Generisanje imena dešifrovanog fajla
                var decryptedFileName = Path.GetFileNameWithoutExtension(UploadedFile.FileName) + "_dec" + Path.GetExtension(UploadedFile.FileName);
                var decryptedFilePath = Path.Combine(saveDir, decryptedFileName);

                // Čuvanje dešifrovanog fajla
                System.IO.File.WriteAllBytes(decryptedFilePath, decrypted);

                DecryptedFilePath = Path.Combine("decrypted", decryptedFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Greška pri dešifrovanju: " + ex.Message;
            }

            return Page();
        }
    }
}
