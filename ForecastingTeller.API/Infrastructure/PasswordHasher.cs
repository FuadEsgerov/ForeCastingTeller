// Infrastructure/PasswordHasher.cs
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace ForecastingTeller.API.Infrastructure
{
    public interface IPasswordHasher
    {
        string HashPassword(string password, out string salt);
        bool VerifyPassword(string password, string passwordHash, string salt);
    }

    public class PasswordHasher : IPasswordHasher
    {
        // Number of iterations for PBKDF2
        private const int IterationCount = 10000;
        
        // Number of bytes to generate for salt
        private const int SaltSize = 16;
        
        // Number of bytes to generate for the hash
        private const int HashSize = 32;

        public string HashPassword(string password, out string salt)
        {
            // Generate a random salt
            byte[] saltBytes = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            // Convert salt to base64 string for storage
            salt = Convert.ToBase64String(saltBytes);

            // Hash the password with PBKDF2
            byte[] hashBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: HashSize);

            // Convert hash to base64 string for storage
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string passwordHash, string salt)
        {
            // Convert salt from base64 string
            byte[] saltBytes = Convert.FromBase64String(salt);

            // Hash the provided password with the stored salt
            byte[] hashBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: HashSize);

            // Convert the computed hash to base64 string
            string computedHash = Convert.ToBase64String(hashBytes);

            // Compare the computed hash with the stored hash
            return computedHash == passwordHash;
        }
    }
}