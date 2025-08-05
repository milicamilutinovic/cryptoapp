using CryptoApp.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoApp.Services
{
    public class FileWatcherService : BackgroundService
    {
        private FileSystemWatcher _watcher;
        private readonly AppSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;

        public FileWatcherService(IOptions<AppSettings> options, IServiceScopeFactory scopeFactory)
        {
            _settings = options.Value;
            _scopeFactory = scopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("FileWatcher servis pokrenut.");
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.IsFileWatcherEnabled)
            {
                Console.WriteLine("File watcher nije omogućen.");
                return Task.CompletedTask;
            }

            _watcher = new FileSystemWatcher(_settings.TargetDirectory)
            {
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileCreated;

            Console.WriteLine("FileSystemWatcher aktiviran na folderu: " + _settings.TargetDirectory);

            return Task.CompletedTask;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Novi fajl detektovan: {e.FullPath}");

            try
            {
                while (IsFileLocked(new FileInfo(e.FullPath)))
                    Thread.Sleep(100);

                using var scope = _scopeFactory.CreateScope();
                var cryptoService = scope.ServiceProvider.GetRequiredService<CryptoService>();

                byte[] fileBytes = File.ReadAllBytes(e.FullPath);

                // Priprema ključa i IV
                string keyText = _settings.SelectedEncryptionAlgorithm.PadRight(16).Substring(0, 16);
                byte[] key = System.Text.Encoding.UTF8.GetBytes(keyText);
                byte[] iv = new byte[8];

                // Šifrovanje
                byte[] encrypted = cryptoService.Encrypt(fileBytes, _settings.SelectedEncryptionAlgorithm, key, iv);

                string encryptedFileName = Path.GetFileName(e.FullPath);
                string outputPath = Path.Combine(_settings.EncryptedFilesDirectory, encryptedFileName);

                Directory.CreateDirectory(_settings.EncryptedFilesDirectory); // kreira ako ne postoji


                File.WriteAllBytes(outputPath, encrypted);
               

                // Hash ključa (radi kasnije verifikacije)
                /*var hasher = new BlakeHasher("meta");  // možeš staviti bilo šta, jer se koristi samo za hashiranje ključa
                string keyHash = hasher.HashString(keyText);

                // Kreiranje .meta fajla
                var meta = new
                {
                    OriginalFileName = encryptedFileName,
                    Algorithm = _settings.SelectedEncryptionAlgorithm,
                    KeyHash = keyHash,
                    CreatedAt = DateTime.UtcNow
                };

                string metaJson = System.Text.Json.JsonSerializer.Serialize(meta, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                string metaPath = outputPath + ".meta";
                File.WriteAllText(metaPath, metaJson);*/

                Console.WriteLine($"Fajl {e.Name} uspešno šifrovan");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u FileWatcherService: {ex.Message}");
            }
        }



        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using var stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
