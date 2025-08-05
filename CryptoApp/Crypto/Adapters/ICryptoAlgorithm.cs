namespace CryptoApp.Crypto.Adapters
{
    public interface ICryptoAlgorithm
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }

}
