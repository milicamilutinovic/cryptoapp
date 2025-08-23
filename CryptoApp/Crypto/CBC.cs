
public class CBC
{
    private readonly XTEA xtea;
    private readonly byte[] iv;
    private const int BlockSize = 8;

    public CBC(byte[] key, byte[] iv)
    {
        this.xtea = new XTEA(key);
        if (iv.Length != BlockSize)
            throw new ArgumentException($"IV must be {BlockSize} bytes");
        this.iv = iv;
    }

    public byte[] Encrypt(byte[] plaintext)
    {
        int padding = BlockSize - (plaintext.Length % BlockSize);
        if (padding == 0) padding = BlockSize;

        byte[] padded = new byte[plaintext.Length + padding];
        Array.Copy(plaintext, padded, plaintext.Length);

        //  dodajem padding vrednosti u zadnje bajtove
        for (int i = plaintext.Length; i < padded.Length; i++)
            padded[i] = (byte)padding;

        byte[] ciphertext = new byte[padded.Length];
        byte[] previousBlock = iv;

        for (int i = 0; i < padded.Length; i += BlockSize)
        {
            byte[] block = new byte[BlockSize];
            Array.Copy(padded, i, block, 0, BlockSize);

            // XOR sa prethodnim blokom
            for (int j = 0; j < BlockSize; j++)
                block[j] ^= previousBlock[j];

            byte[] encrypted = xtea.EncryptBlock(block);
            Array.Copy(encrypted, 0, ciphertext, i, BlockSize);
            previousBlock = encrypted;
        }

        return ciphertext;
    }

    public byte[] Decrypt(byte[] ciphertext, int originalSize)
    {
        if (ciphertext.Length % BlockSize != 0)
            throw new ArgumentException("Ciphertext length must be multiple of block size");

        byte[] plaintext = new byte[ciphertext.Length];
        byte[] previousBlock = iv;

        for (int i = 0; i < ciphertext.Length; i += BlockSize)
        {
            byte[] block = new byte[BlockSize];
            Array.Copy(ciphertext, i, block, 0, BlockSize);

            byte[] decrypted = xtea.DecryptBlock(block);

            for (int j = 0; j < BlockSize; j++)
                plaintext[i + j] = (byte)(decrypted[j] ^ previousBlock[j]);

            previousBlock = block;
        }

        // sklanjam padding tako da vraća originalnu veličinu fajla
        byte[] final = new byte[originalSize];
        Array.Copy(plaintext, 0, final, 0, originalSize);
        return final;
    }
}
