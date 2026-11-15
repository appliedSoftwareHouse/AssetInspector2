using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Crypto
{
    public static class Decrypt
    {
        private const int SaltSize = 16;
        private const int IvSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public static string DecryptString(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText)) throw new ArgumentException("Cipher text is required.", nameof(cipherText));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key is required.", nameof(key));

            byte[] all = Convert.FromBase64String(cipherText);
            if (all.Length <= SaltSize + IvSize)
                throw new CryptographicException("Invalid cipher text.");

            byte[] salt = new byte[SaltSize];
            byte[] iv = new byte[IvSize];
            Buffer.BlockCopy(all, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(all, SaltSize, iv, 0, IvSize);

            using (var kdf = new Rfc2898DeriveBytes(key, salt, Iterations, HashAlgorithmName.SHA256))
            using (var aes = Aes.Create())
            {
                aes.Key = kdf.GetBytes(KeySize);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var ms = new MemoryStream(all, SaltSize + IvSize, all.Length - SaltSize - IvSize))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}