
using System;
using System.Security.Cryptography;

namespace VocabMaster.Helpers
{
    public static class PasswordHasher
    {
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        // Tao mat khau bam PBKDF2-SHA256
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password không được để trống.");

            byte[] salt = new byte[SaltSize];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                return "PBKDF2$SHA256$"
                    + Iterations + "$"
                    + Convert.ToBase64String(salt) + "$"
                    + Convert.ToBase64String(hash);
            }
        }

        // Kiem tra mat khau khi dang nhap
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                string[] parts = storedHash.Split('$');

                if (parts.Length != 5 ||
                    parts[0] != "PBKDF2" ||
                    parts[1] != "SHA256")
                    return false;

                int iterations = int.Parse(parts[2]);
                byte[] salt = Convert.FromBase64String(parts[3]);
                byte[] expectedHash = Convert.FromBase64String(parts[4]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256))
                {
                    byte[] actualHash = pbkdf2.GetBytes(expectedHash.Length);

                    if (actualHash.Length != expectedHash.Length)
                        return false;

                    int difference = 0;

                    for (int i = 0; i < actualHash.Length; i++)
                    {
                        difference |= actualHash[i] ^ expectedHash[i];
                    }

                    return difference == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
