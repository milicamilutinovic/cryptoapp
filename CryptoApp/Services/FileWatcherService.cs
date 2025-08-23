using CryptoApp.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CryptoApp.Services
{
    public class FileWatcherService : BackgroundService
    {
        private FileSystemWatcher _watcher;
        private readonly IOptionsMonitor<AppSettings> _settingsMonitor;
        private readonly IServiceScopeFactory _scopeFactory;

        public FileWatcherService(IOptionsMonitor<AppSettings> settingsMonitor, IServiceScopeFactory scopeFactory)
        {
            _settingsMonitor = settingsMonitor;
            _scopeFactory = scopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("FileWatcher servis pokrenut.");
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var settings = _settingsMonitor.CurrentValue;

            if (!settings.IsFileWatcherEnabled)
            {
                Console.WriteLine("File watcher nije omogućen.");
                return Task.CompletedTask;
            }

            if (!Directory.Exists(settings.TargetDirectory))
                Directory.CreateDirectory(settings.TargetDirectory);

            _watcher = new FileSystemWatcher(settings.TargetDirectory)
            {
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileCreated;

            Console.WriteLine("FileSystemWatcher aktiviran na folderu: " + settings.TargetDirectory);

            return Task.CompletedTask;
        }

        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            var settings = _settingsMonitor.CurrentValue;

            Console.WriteLine($"Novi fajl detektovan: {e.FullPath}");

            try
            {
                // čekaj dok fajl ne bude spreman
                while (IsFileLocked(new FileInfo(e.FullPath)))
                    Thread.Sleep(100);

                using var scope = _scopeFactory.CreateScope();
                var encryptionHelper = scope.ServiceProvider.GetRequiredService<EncryptionHelper>();

                // koristi algoritam iz trenutnog Settings
                await encryptionHelper.EncryptAndSaveFileAsync(e.FullPath, settings.SelectedEncryptionAlgorithm);

                Console.WriteLine($"Fajl {Path.GetFileName(e.FullPath)} uspešno šifrovan algoritmom {settings.SelectedEncryptionAlgorithm} i meta sačuvan.");
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
