using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Infrastructure.Security;

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string value)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = KeyDerivation.Pbkdf2(value, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);
        return Convert.ToBase64String(Combine(salt, key));
    }

    public bool Verify(string hash, string candidate)
    {
        var decoded = Convert.FromBase64String(hash);
        var salt = decoded[..SaltSize];
        var storedKey = decoded[SaltSize..];

        var key = KeyDerivation.Pbkdf2(candidate, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

        return CryptographicOperations.FixedTimeEquals(storedKey, key);
    }

    private static byte[] Combine(byte[] salt, byte[] key)
    {
        var buffer = new byte[salt.Length + key.Length];
        Buffer.BlockCopy(salt, 0, buffer, 0, salt.Length);
        Buffer.BlockCopy(key, 0, buffer, salt.Length, key.Length);
        return buffer;
    }
}
