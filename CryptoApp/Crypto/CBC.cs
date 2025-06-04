namespace CryptoApp.Crypto
{
    public class CBC
    {
        private readonly XTEA xtea;
        private readonly byte[] iv;

        public CBC(byte[] key, byte[] iv)
        {
            this.xtea = new XTEA(key);
            this.iv = iv;
        }

        public byte[] Encrypt(byte[] plaintext)
        {
            int blockSize = 8;
            int paddedLength = ((plaintext.Length + blockSize - 1) / blockSize) * blockSize;
            byte[] padded = new byte[paddedLength];
            Array.Copy(plaintext, padded, plaintext.Length); // zero padding

            byte[] ciphertext = new byte[paddedLength];
            byte[] previousBlock = iv;

            for (int i = 0; i < paddedLength; i += blockSize)
            {
                byte[] block = new byte[blockSize];
                Array.Copy(padded, i, block, 0, blockSize);

                for (int j = 0; j < blockSize; j++)
                    block[j] ^= previousBlock[j];

                byte[] encrypted = xtea.Encrypt(block);
                Array.Copy(encrypted, 0, ciphertext, i, blockSize);
                previousBlock = encrypted;
            }

            return ciphertext;
        }

        public byte[] Decrypt(byte[] ciphertext)
        {
            int blockSize = 8;
            byte[] plaintext = new byte[ciphertext.Length];
            byte[] previousBlock = iv;

            for (int i = 0; i < ciphertext.Length; i += blockSize)
            {
                byte[] block = new byte[blockSize];
                Array.Copy(ciphertext, i, block, 0, blockSize);

                byte[] decrypted = xtea.Decrypt(block);

                for (int j = 0; j < blockSize; j++)
                    plaintext[i + j] = (byte)(decrypted[j] ^ previousBlock[j]);

                previousBlock = block;
            }

            return plaintext;
        }
    }
}
