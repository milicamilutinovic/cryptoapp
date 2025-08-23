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
            key[i] = BitConverter.ToUInt32(keyBytes, i * 4);
    }

    // metoda za enkripciju jednog bloka od 8 bajtova
    public byte[] EncryptBlock(byte[] block)
    {
        uint v0 = BitConverter.ToUInt32(block, 0);
        uint v1 = BitConverter.ToUInt32(block, 4);
        uint sum = 0;
        uint delta = 0x9E3779B9;

        for (int i = 0; i < NUM_ROUNDS; i++)
        {
            v0 += ((v1 << 4 ^ v1 >> 5) + v1) ^ (sum + key[sum & 3]);
            sum += delta;
            v1 += ((v0 << 4 ^ v0 >> 5) + v0) ^ (sum + key[(sum >> 11) & 3]);
        }

        byte[] result = new byte[8];
        Array.Copy(BitConverter.GetBytes(v0), 0, result, 0, 4);
        Array.Copy(BitConverter.GetBytes(v1), 0, result, 4, 4);
        return result;
    }

    //  metoda za dekripciju jednog bloka od 8 bajtova
    public byte[] DecryptBlock(byte[] block)
    {
        uint v0 = BitConverter.ToUInt32(block, 0);
        uint v1 = BitConverter.ToUInt32(block, 4);
        uint delta = 0x9E3779B9;
        uint sum = delta * NUM_ROUNDS;

        for (int i = 0; i < NUM_ROUNDS; i++)
        {
            v1 -= ((v0 << 4 ^ v0 >> 5) + v0) ^ (sum + key[(sum >> 11) & 3]);
            sum -= delta;
            v0 -= ((v1 << 4 ^ v1 >> 5) + v1) ^ (sum + key[sum & 3]);
        }

        byte[] result = new byte[8];
        Array.Copy(BitConverter.GetBytes(v0), 0, result, 0, 4);
        Array.Copy(BitConverter.GetBytes(v1), 0, result, 4, 4);
        return result;
    }

    //  metoda za enkripciju fajla bilo koje velicine
    public byte[] Encrypt(byte[] data)
    {
        int paddedLength = ((data.Length + 7) / 8) * 8; // padding do višekratnika 8
        byte[] padded = new byte[paddedLength];
        Array.Copy(data, padded, data.Length);

        // PKCS7 padding
        byte pad = (byte)(paddedLength - data.Length);
        for (int i = data.Length; i < padded.Length; i++)
            padded[i] = pad;

        byte[] result = new byte[paddedLength];
        for (int i = 0; i < paddedLength; i += 8)
        {
            byte[] block = new byte[8];
            Array.Copy(padded, i, block, 0, 8);
            byte[] enc = EncryptBlock(block);
            Array.Copy(enc, 0, result, i, 8);
        }

        return result;
    }

    //  metoda za dekripciju fajla bilo koje velicine
    public byte[] Decrypt(byte[] data)
    {
        if (data.Length % 8 != 0)
            throw new ArgumentException("Ciphertext length must be multiple of 8");

        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i += 8)
        {
            byte[] block = new byte[8];
            Array.Copy(data, i, block, 0, 8);
            byte[] dec = DecryptBlock(block);
            Array.Copy(dec, 0, result, i, 8);
        }

        // uklanjanje PKCS7 padding-a
        int pad = result[result.Length - 1];
        if (pad < 1 || pad > 8) throw new Exception("Invalid padding");
        byte[] finalResult = new byte[result.Length - pad];
        Array.Copy(result, 0, finalResult, 0, finalResult.Length);

        return finalResult;
    }
}
