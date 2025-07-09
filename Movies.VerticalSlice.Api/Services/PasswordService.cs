using System.Security.Cryptography;
using System.Text;

namespace Movies.VerticalSlice.Api.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        // Generate a random salt
        var salt = RandomNumberGenerator.GetBytes(16);
        
        // Hash the password with the salt using PBKDF2
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: 100000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        // Combine salt and hash
        var combined = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, combined, 0, salt.Length);
        Array.Copy(hash, 0, combined, salt.Length, hash.Length);

        return Convert.ToBase64String(combined);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            var combined = Convert.FromBase64String(hashedPassword);
            
            // Extract salt (first 16 bytes)
            var salt = new byte[16];
            Array.Copy(combined, 0, salt, 0, 16);
            
            // Extract hash (remaining 32 bytes)
            var storedHash = new byte[32];
            Array.Copy(combined, 16, storedHash, 0, 32);
            
            // Hash the provided password with the extracted salt
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt,
                iterations: 100000,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: 32);

            // Compare hashes
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
        catch
        {
            return false;
        }
    }
}