namespace CryptoApp.Crypto.Adapters
{
    public class RC4Crypto : ICryptoAlgorithm
    {
        private readonly RC4 rc4;

        public RC4Crypto(byte[] key)
        {
            rc4 = new RC4(key);
        }

        public byte[] Encrypt(byte[] data) => rc4.Encrypt(data);
        public byte[] Decrypt(byte[] data) => rc4.Decrypt(data);
    }


}
