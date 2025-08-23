namespace CryptoApp.Crypto
{
    public class RC4
    {
        private byte[] S = new byte[256];
        private int x = 0, y = 0;

        public RC4(byte[] key)
        {
            Initialize(key);
        }

        private void Initialize(byte[] key)
        {
            int keyLength = key.Length;

            for (int i = 0; i < 256; i++)
                S[i] = (byte)i;

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + S[i] + key[i % keyLength]) % 256;
                Swap(i, j);
            }
        }

        private void Swap(int i, int j)
        {
            byte temp = S[i];
            S[i] = S[j];
            S[j] = temp;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] result = new byte[data.Length];

            for (int k = 0; k < data.Length; k++)
            {
                x = (x + 1) % 256;
                y = (y + S[x]) % 256;
                Swap(x, y);
                byte xorIndex = (byte)((S[x] + S[y]) % 256);
                result[k] = (byte)(data[k] ^ S[xorIndex]);
            }

            return result;
        }

        public byte[] Decrypt(byte[] data)
        {
            return Encrypt(data); // RC4 dekripcija ista kao enkripcija
        }
    }
}
