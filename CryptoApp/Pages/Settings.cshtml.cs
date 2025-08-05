using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CryptoApp.Models;
using Microsoft.Extensions.Options;

namespace CryptoApp.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public SettingsModel(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [BindProperty]
        public AppSettings Settings { get; set; }

       
        public string StatusMessage { get; set; }

        public void OnGet()
        {
            Settings = _configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
        }

        public IActionResult OnPost()
        {
            var form = Request.Form;

            Settings.EncryptedFilesDirectory = form["EncryptedFilesDirectory"];
            Settings.TargetDirectory = form["TargetDirectory"];
            Settings.SelectedEncryptionAlgorithm = form["SelectedEncryptionAlgorithm"];
            Settings.SelectedHashAlgorithm = form["SelectedHashAlgorithm"];

            Settings.IsFileWatcherEnabled = form.ContainsKey("IsFileWatcherEnabled");
            Settings.IsFileExchangeEnabled = form.ContainsKey("IsFileExchangeEnabled");

            var configPath = Path.Combine(_env.ContentRootPath, "appsettings.json");

            var newConfig = new
            {
                AppSettings = Settings
            };

            var json = System.Text.Json.JsonSerializer.Serialize(newConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            System.IO.File.WriteAllText(configPath, json);

            StatusMessage = "Podešavanja su uspešno sačuvana.";

            return Page();
        }


    }
}
