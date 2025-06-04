using System;
using System.Text;
using Konscious.Security.Cryptography;

public class BlakeHasher
{
    private readonly byte[] _key;
    private readonly int _hashSizeBits;

    public BlakeHasher(string key, int hashSizeBits = 256)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key must not be null or empty.", nameof(key));

        if (hashSizeBits <= 0 || hashSizeBits > 512)
            throw new ArgumentOutOfRangeException(nameof(hashSizeBits), "Hash size must be between 1 and 512 bits.");

        _key = Encoding.UTF8.GetBytes(key);
        _hashSizeBits = hashSizeBits;
    }

    public string HashString(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input must not be null or empty.", nameof(input));

        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        using (var hasher = new HMACBlake2B(_hashSizeBits))
        {
            hasher.Key = _key;
            byte[] hashBytes = hasher.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
