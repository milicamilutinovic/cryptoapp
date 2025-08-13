using CryptoApp.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CryptoApp.Services
{
    public class TransferService
    {
        private readonly AppSettings _settings;

        public TransferService(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<bool> SendFileAsync(string filePath, string ipAddress, int port, Action<string> statusCallback, int maxRetries = 5, int delayMs = 2000)
        {
            byte[] fileBytes;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using var ms = new MemoryStream();
                await fs.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            if (fileBytes == null || fileBytes.Length == 0)
            {
                statusCallback?.Invoke($"Fajl '{filePath}' je prazan ili nije moguće pročitati.");
                return false;
            }

            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    using TcpClient client = new TcpClient();
                    await client.ConnectAsync(ipAddress, port);

                    using NetworkStream stream = client.GetStream();
                    using BinaryWriter writer = new BinaryWriter(stream);

                    string fileName = Path.GetFileName(filePath);
                    var blakeHasher = new BlakeHasher("tajni_kljuc_za_hash");
                    byte[] hash = blakeHasher.HashBytes(fileBytes);
                    byte[] encryptedData = fileBytes;

                    writer.Write(fileName);
                    writer.Write((long)fileBytes.Length);
                    writer.Write(hash.Length);
                    writer.Write(hash);
                    writer.Write(encryptedData.Length);
                    writer.Write(encryptedData);

                    await stream.FlushAsync();
                    statusCallback?.Invoke($"Fajl '{fileName}' uspešno poslat.");
                    return true; // uspešno poslato
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



        public async Task ReceiveFilesLoopAsync(string saveDirectory, int listenPort, CancellationToken token)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, listenPort);
            listener.Start();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        using var client = await listener.AcceptTcpClientAsync();
                        using var stream = client.GetStream();
                        using var reader = new BinaryReader(stream);

                        string fileName = reader.ReadString();
                        long originalSize = reader.ReadInt64();
                        int hashLength = reader.ReadInt32();
                        byte[] receivedHash = reader.ReadBytes(hashLength);
                        int encryptedLength = reader.ReadInt32();

                        byte[] encryptedData = new byte[encryptedLength];
                        int totalRead = 0;
                        while (totalRead < encryptedLength)
                        {
                            int read = await stream.ReadAsync(encryptedData, totalRead, encryptedLength - totalRead, token);
                            if (read == 0)
                                throw new EndOfStreamException("Prekidan stream tokom čitanja fajla.");
                            totalRead += read;
                        }

                        byte[] decryptedData = encryptedData; // Dekripcija ako treba

                        var blakeHasher = new BlakeHasher("tajni_kljuc_za_hash");
                        byte[] localHash = blakeHasher.HashBytes(decryptedData);

                        bool isValid = StructuralComparisons.StructuralEqualityComparer.Equals(localHash, receivedHash);

                        if (!isValid)
                        {
                            Console.WriteLine("Integritet fajla NIJE potvrđen. Fajl je možda oštećen.");
                            continue; // nastavi sa sledećim fajlom, ne prekidaj petlju
                        }

                        Directory.CreateDirectory(saveDirectory);
                        string fullPath = Path.Combine(saveDirectory, fileName);
                        await File.WriteAllBytesAsync(fullPath, decryptedData);

                        Console.WriteLine($"Fajl '{fileName}' primljen i sačuvan u {fullPath}");
                    }
                    catch (Exception exInner)
                    {
                        Console.WriteLine("Greška prilikom obrade jednog fajla: " + exInner.Message);
                        // Možeš logovati i stack trace ako želiš
                    }
                }
            }
            catch (Exception exOuter)
            {
                Console.WriteLine("Greška u listen loop-u: " + exOuter.Message);
            }
            finally
            {
                listener?.Stop();
            }
        }

    }
}
