using CryptoApp.Models;
using CryptoApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CryptoApp.Pages
{
    public class TargetPage : PageModel
    {
        private readonly EncryptionHelper _encryptionHelper;
        private readonly IWebHostEnvironment _env;
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

        public async Task<IActionResult> OnPostAsync(string action, string fileName)
        {
            var selectedAlgorithm = _settingsSnapshot.Value.SelectedEncryptionAlgorithm;

            // Upload i šifrovanje
            if (UploadFile != null && UploadFile.Length > 0)
            {
                var targetDir = Path.Combine(_env.WebRootPath, _settingsSnapshot.Value.EncryptedFilesDirectory);
                Directory.CreateDirectory(targetDir);

                var savePath = Path.Combine(targetDir, Path.GetFileName(UploadFile.FileName));
                using (var fs = new FileStream(savePath, FileMode.Create))
                {
                    await UploadFile.CopyToAsync(fs);
                }

                // Prosledi izabrani algoritam
                await _encryptionHelper.EncryptAndSaveFileAsync(savePath, selectedAlgorithm);

                StatusMessage = $"Fajl snimljen i enkriptovan ({selectedAlgorithm}).";
                LoadEncryptedFiles();
                return Page();
            }

            // Dešifrovanje
            if (action == "Decrypt" && !string.IsNullOrEmpty(fileName))
            {
                string decodedDir = Path.Combine(_env.WebRootPath, "decoded");
                Directory.CreateDirectory(decodedDir);

                string decodedFileName = RemoveEncSuffix(fileName);
                string decodedFilePath = Path.Combine(decodedDir, decodedFileName);

                if (!System.IO.File.Exists(decodedFilePath))
                {
                    try
                    {
                        await _encryptionHelper.DecryptAndSaveFileAsync(fileName);
                        StatusMessage = $"Fajl '{fileName}' uspešno dešifrovan.";
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"Greška pri dešifrovanju: {ex.Message}";
                    }
                }
                else
                {
                    StatusMessage = $"Fajl '{fileName}' je već dešifrovan.";
                }

                LoadEncryptedFiles();
                return Page();
            }

            StatusMessage = "Niste izabrali fajl ili akciju.";
            LoadEncryptedFiles();
            return Page();
        }

        // Preuzimanje dekriptovanog fajla
        public async Task<IActionResult> OnGetDownloadAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            string decodedDir = Path.Combine(_env.WebRootPath, "decoded");
            Directory.CreateDirectory(decodedDir);

            string decodedFileName = RemoveEncSuffix(fileName);
            string decodedFilePath = Path.Combine(decodedDir, decodedFileName);

            if (!System.IO.File.Exists(decodedFilePath))
            {
                try
                {
                    await _encryptionHelper.DecryptAndSaveFileAsync(fileName);
                }
                catch (Exception ex)
                {
                    return Content("Greška pri dešifrovanju: " + ex.Message);
                }
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(decodedFilePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", decodedFileName);
        }

        private void LoadEncryptedFiles()
        {
            EncryptedFiles.Clear();
            var encDir = Path.Combine(_env.WebRootPath, _settingsSnapshot.Value.EncryptedFilesDirectory);
            if (!Directory.Exists(encDir)) return;

            var files = Directory.GetFiles(encDir)
                        .Where(f => !f.EndsWith(".meta"))
                        .ToList();

            foreach (var f in files)
            {
                string name = Path.GetFileName(f);
                if (name.Contains("_enc_"))
                    EncryptedFiles.Add(name);
            }
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
    }
}
