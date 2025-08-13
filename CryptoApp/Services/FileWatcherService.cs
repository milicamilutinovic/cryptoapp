using CryptoApp.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

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

        // (u FileWatcherService klase)
        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Novi fajl detektovan: {e.FullPath}");

            try
            {
                while (IsFileLocked(new FileInfo(e.FullPath)))
                    Thread.Sleep(100);

                using var scope = _scopeFactory.CreateScope();
                var encryptionHelper = scope.ServiceProvider.GetRequiredService<EncryptionHelper>();

                await encryptionHelper.EncryptAndSaveFileAsync(e.FullPath);

                Console.WriteLine($"Fajl {e.Name} uspešno šifrovan i meta sačuvan.");
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
                using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

    }
}
