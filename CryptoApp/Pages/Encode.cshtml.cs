using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CryptoApp.Services;
using System;
using System.IO;
using System.Text;

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

                // Pretvaranje ključa u bajtove (ovde jednostavno UTF8)
                byte[] keyBytes = Encoding.UTF8.GetBytes(Key);

                // Ako koristiš CBC algoritam, možeš ovde dodati inicijalizacijski vektor (iv)
                byte[] iv = null;
                if (Algorithm == "XTEA-CBC")
                {
                    // Primer: fiksni IV (u realnoj aplikaciji generiši nasumični IV i sačuvaj ga zajedno sa fajlom)
                    iv = new byte[8]; // XTEA block size je 8 bajtova
                    Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));
                }

                // Šifrovanje
                byte[] encryptedData = _cryptoService.Encrypt(fileBytes, Algorithm, keyBytes, iv);

                // Snimanje fajla u wwwroot/encrypted (kreiraj ovaj folder)
                var saveDir = Path.Combine(_env.WebRootPath, "encrypted");
                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);

                var fileName = Path.GetFileNameWithoutExtension(UploadedFile.FileName) + "_enc" + Path.GetExtension(UploadedFile.FileName);
                var filePath = Path.Combine(saveDir, fileName);

                System.IO.File.WriteAllBytes(filePath, encryptedData);

                // Sačuvaj relativnu putanju za prikaz u linku za preuzimanje
                EncryptedFilePath = Path.Combine("encrypted", fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Došlo je do greške: " + ex.Message;
            }

            return Page();
        }
    }
}
