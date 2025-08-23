// CBCrypto adapter
using CryptoApp.Crypto.Adapters;

public class CBCrypto : ICryptoAlgorithm
{
    private readonly CBC cbc;
    private readonly int originalSize;

    public CBCrypto(byte[] key, byte[] iv, int originalSize)
    {
        cbc = new CBC(key, iv);
        this.originalSize = originalSize;
    }

    public byte[] Encrypt(byte[] data) => cbc.Encrypt(data);

    public byte[] Decrypt(byte[] data) => cbc.Decrypt(data, originalSize);
}
