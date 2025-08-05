namespace CryptoApp.Crypto.Adapters
{
    public class XTEACrypto : ICryptoAlgorithm
    {
        private readonly XTEA xtea;

        public XTEACrypto(byte[] key)
        {
            xtea = new XTEA(key);
        }

        public byte[] Encrypt(byte[] data) => xtea.Encrypt(data);
        public byte[] Decrypt(byte[] data) => xtea.Decrypt(data);
    }
}
