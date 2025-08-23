using CryptoApp.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoApp.Services
{
    public class TransferService
    {
        private readonly AppSettings _settings;
        private readonly EncryptionHelper _encryptionHelper;

        public TransferService(IOptions<AppSettings> options, EncryptionHelper encryptionHelper)
        {
            _settings = options.Value;
            _encryptionHelper = encryptionHelper;
        }

        // -------------------- SLANJE --------------------
        public async Task<bool> SendFileAsync(string filePath, string ipAddress, int port, Action<string> statusCallback, int maxRetries = 5, int delayMs = 2000)
        {
            if (!File.Exists(filePath))
            {
                statusCallback?.Invoke($"Fajl '{filePath}' ne postoji.");
                return false;
            }

            // 1. Enkripcija fajla (postojeća metoda)
            string encryptedPath = await _encryptionHelper.EncryptAndSaveFileAsync(filePath, _settings.SelectedEncryptionAlgorithm);

            string metaPath = encryptedPath + ".meta";
            if (!File.Exists(metaPath))
            {
                statusCallback?.Invoke("Meta fajl nije pronađen nakon enkripcije.");
                return false;
            }

            byte[] encryptedData = await File.ReadAllBytesAsync(encryptedPath);
            byte[] metaData = await File.ReadAllBytesAsync(metaPath);

            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    using TcpClient client = new TcpClient();
                    await client.ConnectAsync(ipAddress, port);
                    using NetworkStream stream = client.GetStream();
                    using var writer = new BinaryWriter(stream);

                    string originalFileName = Path.GetFileName(filePath);

                    // --- Slanje imena fajla ---
                    writer.Write(originalFileName);

                    // --- Slanje enkriptovanog fajla ---
                    writer.Write((long)encryptedData.Length); // long: veličina enkriptovanog fajla
                    writer.Write(encryptedData);

                    // --- Slanje meta fajla ---
                    writer.Write((long)metaData.Length);      // long: veličina meta fajla
                    writer.Write(metaData);

                    await stream.FlushAsync();
                    statusCallback?.Invoke($"Fajl '{originalFileName}' uspešno poslat.");
                    return true;
                }
                catch (SocketException)
                {
                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        statusCallback?.Invoke("Niko ne sluša na ovom portu, fajl nije poslat.");
                        return false;
                    }
                    statusCallback?.Invoke($"Primalac nedostupan, pokušavam ponovo ({attempt}/{maxRetries})...");
                    await Task.Delay(delayMs);
                }
            }

            return false;
        }



        // -------------------- PRIJEM --------------------
        public async Task ReceiveFilesLoopAsync(string saveDirectory, int listenPort, CancellationToken token)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, listenPort);
            listener.Start();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    using var client = await listener.AcceptTcpClientAsync();
                    using var stream = client.GetStream();
                    using var reader = new BinaryReader(stream);

                    try
                    {
                        // --- Primanje imena fajla ---
                        string fileName = reader.ReadString();

                        // --- Primanje enkriptovanog fajla ---
                        long encryptedLength = reader.ReadInt64();
                        byte[] encryptedData = reader.ReadBytes((int)encryptedLength);

                        // --- Privremeni fajl za enkriptovani sadržaj ---
                        string tempEncryptedPath = Path.Combine(Path.GetTempPath(), fileName);
                        await File.WriteAllBytesAsync(tempEncryptedPath, encryptedData);

                        // --- Primanje meta fajla ---
                        long metaLength = reader.ReadInt64();
                        byte[] metaData = reader.ReadBytes((int)metaLength);
                        string tempMetaPath = tempEncryptedPath + ".meta";
                        await File.WriteAllBytesAsync(tempMetaPath, metaData);

                        // --- Folder gde UI gleda fajlove (saveDirectory) ---
                        Directory.CreateDirectory(saveDirectory);

                        // --- Dekripcija direktno u saveDirectory ---
                        await _encryptionHelper.DecryptAndSaveFileToDirectoryAsync(tempEncryptedPath, saveDirectory, fileName);

                        // --- Obriši privremeni enkriptovani fajl i meta fajl ---
                        File.Delete(tempEncryptedPath);
                        File.Delete(tempMetaPath);

                        Console.WriteLine($"Fajl '{fileName}' primljen, dekriptovan i sačuvan u '{saveDirectory}'.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Greška prilikom obrade fajla: " + ex.Message);
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }


    }
}
