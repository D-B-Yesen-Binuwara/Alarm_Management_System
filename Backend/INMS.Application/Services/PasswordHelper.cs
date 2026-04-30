using System.Security.Cryptography;

namespace INMS.Application.Services;

public static class PasswordHelper
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32; // 256 bit
    private const int Iterations = 100_000;

    public static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[SaltSize];
        rng.GetBytes(saltBytes);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);

        passwordSalt = Convert.ToBase64String(saltBytes);
        passwordHash = Convert.ToBase64String(key);
    }

    public static bool VerifyPassword(string password, string base64Salt, string base64Hash)
    {
        if (string.IsNullOrEmpty(base64Salt) || string.IsNullOrEmpty(base64Hash)) return false;

        var saltBytes = Convert.FromBase64String(base64Salt);
        var expectedHash = Convert.FromBase64String(base64Hash);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);

        return CryptographicOperations.FixedTimeEquals(key, expectedHash);
    }
}
