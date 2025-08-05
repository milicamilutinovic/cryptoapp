namespace CryptoApp.Crypto.Adapters
{
    public class CBCrypto : ICryptoAlgorithm
    {
        private readonly CBC cbc;

        public CBCrypto(byte[] key, byte[] iv)
        {
            cbc = new CBC(key, iv);
        }

        public byte[] Encrypt(byte[] data) => cbc.Encrypt(data);
        public byte[] Decrypt(byte[] data) => cbc.Decrypt(data);
    }
}
