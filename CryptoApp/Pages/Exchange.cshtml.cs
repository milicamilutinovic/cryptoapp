using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using CryptoApp.Services;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace CryptoApp.Pages
{
    public class ExchangeModel : PageModel
    {
        private readonly TransferService _transferService;
        private readonly IWebHostEnvironment _env;

        public ExchangeModel(TransferService transferService, IWebHostEnvironment env)
        {
            _transferService = transferService;
            _env = env;
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

       


        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (action == "Pošalji fajl")
            {
                if (UploadFile == null || UploadFile.Length == 0)
                {
                    StatusMessage = "Niste odabrali fajl.";
                    return Page();
                }

                // Sačuvaj fajl privremeno na serveru
                var tempFile = Path.Combine(_env.WebRootPath, "uploads", UploadFile.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(tempFile));
                using var stream = new FileStream(tempFile, FileMode.Create);
                await UploadFile.CopyToAsync(stream);

                await _transferService.SendFileAsync(tempFile, IpAddress, Port);
                StatusMessage = "Fajl uspešno poslat.";
                IsListening = false;
            }
            else if (action == "Slušaj za fajl")
            {
                var savePath = Path.Combine(_env.WebRootPath, "received");
                Directory.CreateDirectory(savePath);

                _ = Task.Run(() => _transferService.ReceiveFileAsync(savePath, Port));
                StatusMessage = $"Slušanje pokrenuto na portu {Port}.";
                IsListening = true;
            }

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

    }
}
