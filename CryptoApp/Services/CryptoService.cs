using CryptoApp.Crypto.Adapters;
using System;
using System.Text;

namespace CryptoApp.Services
{
    public class CryptoService
    {
        public ICryptoAlgorithm GetAlgorithm(string algorithmName, byte[] key, byte[] iv = null, int originalSize = 0)
        {
            return algorithmName switch
            {
                "RC4" => new RC4Crypto(key),
                "XTEA" => new XTEACrypto(key),
                "XTEA-CBC" => iv != null ? new CBCrypto(key, iv, originalSize) : throw new ArgumentException("IV is required for CBC mode"),
                _ => throw new NotSupportedException($"Algorithm {algorithmName} is not supported")
            };
        }

        public byte[] Encrypt(byte[] data, string algorithmName, byte[] key, byte[] iv = null)
        {
            var algo = GetAlgorithm(algorithmName, key, iv);
            return algo.Encrypt(data);
        }

        public byte[] Decrypt(byte[] data, string algorithmName, byte[] key, byte[] iv = null, int originalSize = 0)
        {
            var algo = GetAlgorithm(algorithmName, key, iv, originalSize);
            return algo.Decrypt(data);
        }
    }

}
