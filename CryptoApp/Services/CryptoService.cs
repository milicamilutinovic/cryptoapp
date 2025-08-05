using CryptoApp.Crypto.Adapters;
using System;
using System.Text;

namespace CryptoApp.Services
{
    public class CryptoService
    {
        public ICryptoAlgorithm GetAlgorithm(string algorithmName, byte[] key, byte[] iv = null)
        {
            return algorithmName switch
            {
                "RC4" => new RC4Crypto(key),
                "XTEA" => new XTEACrypto(key),
                "XTEA-CBC" => iv != null ? new CBCrypto(key, iv) : throw new ArgumentException("IV is required for CBC mode"),
                _ => throw new NotSupportedException($"Algorithm {algorithmName} is not supported")
            };
        }

        // Metod za šifrovanje, koristi se izvan servisa
        public byte[] Encrypt(byte[] data, string algorithmName, byte[] key, byte[] iv = null)
        {
            var algo = GetAlgorithm(algorithmName, key, iv);
            return algo.Encrypt(data);
        }

        // Metod za dešifrovanje
        public byte[] Decrypt(byte[] data, string algorithmName, byte[] key, byte[] iv = null)
        {
            var algo = GetAlgorithm(algorithmName, key, iv);
            return algo.Decrypt(data);
        }
    }
}
