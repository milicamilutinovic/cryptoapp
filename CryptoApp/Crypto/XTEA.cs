namespace CryptoApp.Crypto
{
    public class XTEA
    {
        private const int NUM_ROUNDS = 32;
        private readonly uint[] key;

        public XTEA(byte[] keyBytes)
        {
            if (keyBytes.Length != 16)
                throw new ArgumentException("XTEA key must be 16 bytes");

            key = new uint[4];
            for (int i = 0; i < 4; i++)
            {
                key[i] = BitConverter.ToUInt32(keyBytes, i * 4);
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            int paddedLength = ((data.Length + 7) / 8) * 8;
            byte[] result = new byte[paddedLength];

            for (int i = 0; i < paddedLength; i += 8)
            {
                uint v0 = i < data.Length ? BitConverter.ToUInt32(data, i) : 0;
                uint v1 = (i + 4 < data.Length) ? BitConverter.ToUInt32(data, i + 4) : 0;

                uint sum = 0;
                uint delta = 0x9E3779B9;

                for (int j = 0; j < NUM_ROUNDS; j++)
                {
                    v0 += ((v1 << 4 ^ v1 >> 5) + v1) ^ (sum + key[sum & 3]);
                    sum += delta;
                    v1 += ((v0 << 4 ^ v0 >> 5) + v0) ^ (sum + key[(sum >> 11) & 3]);
                }

                Array.Copy(BitConverter.GetBytes(v0), 0, result, i, 4);
                Array.Copy(BitConverter.GetBytes(v1), 0, result, i + 4, 4);
            }

            return result;
        }

        public byte[] Decrypt(byte[] data)
        {
            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i += 8)
            {
                uint v0 = BitConverter.ToUInt32(data, i);
                uint v1 = BitConverter.ToUInt32(data, i + 4);

                uint delta = 0x9E3779B9;
                uint sum = delta * NUM_ROUNDS;

                for (int j = 0; j < NUM_ROUNDS; j++)
                {
                    v1 -= ((v0 << 4 ^ v0 >> 5) + v0) ^ (sum + key[(sum >> 11) & 3]);
                    sum -= delta;
                    v0 -= ((v1 << 4 ^ v1 >> 5) + v1) ^ (sum + key[sum & 3]);
                }

                Array.Copy(BitConverter.GetBytes(v0), 0, result, i, 4);
                Array.Copy(BitConverter.GetBytes(v1), 0, result, i + 4, 4);
            }

            return result;
        }
    }
}
