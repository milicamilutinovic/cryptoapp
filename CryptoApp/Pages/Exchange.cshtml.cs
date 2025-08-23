using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using CryptoApp.Services;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using CryptoApp.Models;
using Microsoft.Extensions.Options;

namespace CryptoApp.Pages
{
    public class ExchangeModel : PageModel
    {
        private readonly TransferService _transferService;
        private readonly IWebHostEnvironment _env;
        private readonly IOptionsSnapshot<AppSettings> _settingsSnapshot;


        public ExchangeModel(TransferService transferService, IWebHostEnvironment env, IOptionsSnapshot<AppSettings> settingsSnapshot)
        {
            _transferService = transferService;
            _env = env;
            _settingsSnapshot = settingsSnapshot;
        }
        [BindProperty]
        public IFormFile UploadFile { get; set; }

        [BindProperty]
        public string IpAddress { get; set; }

        [BindProperty]
        public int Port { get; set; } = 9000;

        public string StatusMessage { get; set; }
        public List<string> ReceivedFiles { get; set; } = new();
        public bool IsListening { get; set; } = false;

        public bool IsFileExchangeEnabled => _settingsSnapshot.Value.IsFileExchangeEnabled;

        private CancellationTokenSource _cts;

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (action == "Slušaj za fajl")
            {
                var savePath = Path.Combine(_env.WebRootPath, "received");
                Directory.CreateDirectory(savePath);

                // ako vec slusa, ne pokreći opet
                if (_cts == null)
                {
                    _cts = new CancellationTokenSource();

                    // pokretanje slusanje u pozadini 
                    _ = _transferService.ReceiveFilesLoopAsync(savePath, Port, _cts.Token);
                }

                StatusMessage = $"Slušanje pokrenuto na portu {Port}.";
                IsListening = true;
            }
            else if (action == "Zaustavi slušanje")
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts = null;
                    StatusMessage = "Slušanje je zaustavljeno.";
                    IsListening = false;
                }
            }
            else if (action == "Pošalji fajl")
            {
                if (UploadFile == null || UploadFile.Length == 0)
                {
                    StatusMessage = "Niste odabrali fajl.";
                    return Page();
                }

                var tempFile = Path.Combine(_env.WebRootPath, "uploads", UploadFile.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(tempFile));

                using (var fileStream = new FileStream(tempFile, FileMode.Create))
                {
                    await UploadFile.CopyToAsync(fileStream);
                }

                // zovi SendFileAsync sa callback funkcijom koja azurira StatusMessage
                await _transferService.SendFileAsync(tempFile, IpAddress, Port, msg =>
                {
                    StatusMessage = msg;
                });
            }



            // azuriraj listu primljenih fajlova ako slusas
            if (IsListening)
            {
                var receivedDir = Path.Combine(_env.WebRootPath, "received");
                if (Directory.Exists(receivedDir))
                {
                    ReceivedFiles = new List<string>(Directory.GetFiles(receivedDir));
                    for (int i = 0; i < ReceivedFiles.Count; i++)
                    {
                        ReceivedFiles[i] = Path.GetFileName(ReceivedFiles[i]);
                    }
                }
            }
            else
            {
                ReceivedFiles.Clear();
            }

            return Page();
        }
        public JsonResult OnGetReceivedFiles()
        {
            var receivedDir = Path.Combine(_env.WebRootPath, "received");
            List<string> files = new();

            if (Directory.Exists(receivedDir))
            {
                files = Directory.GetFiles(receivedDir)
                                 .Select(Path.GetFileName)
                                 .ToList();
            }

            return new JsonResult(files);
        }

    }
}