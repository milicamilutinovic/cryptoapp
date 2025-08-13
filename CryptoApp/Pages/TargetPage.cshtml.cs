using CryptoApp.Models;
using CryptoApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CryptoApp.Pages
{
    public class TargetPage : PageModel
    {
        private readonly EncryptionHelper _encryptionHelper;
        private readonly IWebHostEnvironment _env;
        private readonly AppSettings _settings;
        private readonly IOptionsSnapshot<AppSettings> _settingsSnapshot;

        public TargetPage(EncryptionHelper encryptionHelper, IWebHostEnvironment env, IOptionsSnapshot<AppSettings> settingsSnapshot)
        {
            _encryptionHelper = encryptionHelper;
            _env = env;
            _settingsSnapshot = settingsSnapshot;
        }


        [BindProperty]
        public IFormFile UploadFile { get; set; }

        public string StatusMessage { get; set; }
        public List<string> EncryptedFiles { get; set; } = new();
       public bool IsFileWatcherEnabled => _settingsSnapshot.Value.IsFileWatcherEnabled;


        public void OnGet()
        {
            StatusMessage = "FileWatcher enabled: " + IsFileWatcherEnabled;
            if (IsFileWatcherEnabled)
                LoadEncryptedFiles();
        }

        // action: "Decrypt" kada je forma za decrypt poslata
        public async Task<IActionResult> OnPostAsync(string action, string fileName)
        {
            // Ako FSW nije uključen — dozvoli ručno enkriptovanje, ali u drugačijem režimu:
            if (!IsFileWatcherEnabled)
            {
                // Ako korisnik želi da DEŠIFRUJE, može — proveravamo kao i obično
                if (action == "Decrypt" && !string.IsNullOrEmpty(fileName))
                {
                    var decodedDir = Path.Combine(_env.WebRootPath, "decoded");
                    Directory.CreateDirectory(decodedDir);

                    var decodedFileName = RemoveEncSuffix(fileName);
                    var decodedFilePath = Path.Combine(decodedDir, decodedFileName);

                    if (System.IO.File.Exists(decodedFilePath))
                    {
                        StatusMessage = $"Fajl '{fileName}' je već dešifrovan i nalazi se u folderu 'decoded'.";
                    }
                    else
                    {
                        try
                        {
                            await _encryptionHelper.DecryptAndSaveFileAsync(fileName);
                            StatusMessage = $"Fajl '{fileName}' uspešno dešifrovan. <a href=\"/decoded/{decodedFileName}\" target=\"_blank\">Preuzmi ovde</a>.";
                        }
                        catch (Exception ex)
                        {
                            StatusMessage = $"Greška prilikom dešifrovanja: {ex.Message}";
                        }
                    }

                    return Page();
                }

                // FSW = false => korisnik može upload-ovati fajl i direktno ga šifrovati u X (ne u Target)
                if (UploadFile == null || UploadFile.Length == 0)
                {
                    StatusMessage = "Niste izabrali fajl.";
                    return Page();
                }

                byte[] input;
                using (var ms = new MemoryStream())
                {
                    await UploadFile.CopyToAsync(ms);
                    input = ms.ToArray();
                }

                // privremeni fajl (ne moramo da ga držimo u Target) — sačuvaj u temp pa enkriptuj
                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(UploadFile.FileName));
                await System.IO.File.WriteAllBytesAsync(tempPath, input);

                // Enkripcija i čuvanje u wwwroot/X
                await _encryptionHelper.EncryptAndSaveFileAsync(tempPath);

                // izbriši temp
                try { System.IO.File.Delete(tempPath); } catch { }

                StatusMessage = "Fajl uspešno enkriptovan i sačuvan u X (encrypted).";
                return Page();
            }

            // ---- ovde je slučaj FSW = true ----
            // Ako je zahtev za dešifrovanje
            if (action == "Decrypt" && !string.IsNullOrEmpty(fileName))
            {
                var decodedDir = Path.Combine(_env.WebRootPath, "decoded");
                Directory.CreateDirectory(decodedDir);

                var decodedFileName = RemoveEncSuffix(fileName);
                var decodedFilePath = Path.Combine(decodedDir, decodedFileName);

                if (System.IO.File.Exists(decodedFilePath))
                {
                    StatusMessage = $"Fajl '{fileName}' je već dešifrovan i nalazi se u folderu 'decoded'.";
                }
                else
                {
                    try
                    {
                        await _encryptionHelper.DecryptAndSaveFileAsync(fileName);
                        StatusMessage = $"Fajl '{fileName}' uspešno dešifrovan. <a href=\"/decoded/{decodedFileName}\" target=\"_blank\">Preuzmi ovde</a>.";
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"Greška prilikom dešifrovanja: {ex.Message}";
                    }
                }

                LoadEncryptedFiles();
                return Page();
            }

            // Upload kada je FSW uključen: snimi u Target i odmah enkriptuj (EncryptHelper će preskočiti duplikate)
            if (UploadFile == null || UploadFile.Length == 0)
            {
                StatusMessage = "Niste izabrali fajl.";
                LoadEncryptedFiles();
                return Page();
            }

            var targetDir = Path.Combine(_env.WebRootPath, _settingsSnapshot.Value.EncryptedFilesDirectory);
            Directory.CreateDirectory(targetDir);

            var savePath = Path.Combine(targetDir, Path.GetFileName(UploadFile.FileName));
            using (var fs = new FileStream(savePath, FileMode.Create))
            {
                await UploadFile.CopyToAsync(fs);
            }

            // Odmah enkriptuj (EncryptionHelper će preskočiti ako već postoji)
            await _encryptionHelper.EncryptAndSaveFileAsync(savePath);

            StatusMessage = "Fajl snimljen u Target i odmah enkriptovan.";
            LoadEncryptedFiles();
            return Page();
        }

        private void LoadEncryptedFiles()
        {
            EncryptedFiles.Clear();
            var encDir = Path.Combine(_env.WebRootPath, _settingsSnapshot.Value.EncryptedFilesDirectory);
            if (!Directory.Exists(encDir)) return;

            var files = Directory.GetFiles(encDir)
                       .Where(f => !f.EndsWith(".meta") && IsEncryptedFile(f))
                       .ToList();

            foreach (var f in files)
            {
                EncryptedFiles.Add(Path.GetFileName(f));
            }

            bool IsEncryptedFile(string filePath)
            {
                // Npr. samo fajlovi sa _enc pre ekstenzijom ili neka druga logika ako treba
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                return fileName.EndsWith("_enc");
            }

        }


        private static string RemoveEncSuffix(string encryptedFileName)
        {
            var ext = Path.GetExtension(encryptedFileName);
            var name = Path.GetFileNameWithoutExtension(encryptedFileName);
            if (name.EndsWith("_enc"))
                name = name.Substring(0, name.Length - 4);
            return name + ext;
        }
    }
}
