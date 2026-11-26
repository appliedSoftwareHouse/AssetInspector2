using System.Security.Cryptography;
using System.Text;

namespace Crypto
{
    public static class Encrypt
    {
        private const int SaltSize = 16;
        private const int IvSize = 16;
        private const int KeySize = 32;      // AES-256
        private const int Iterations = 100_000;



        public static string EncryptString(string plainText, string key)
        {
            if (plainText == null) throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key is required.", nameof(key));

            byte[] salt = new byte[SaltSize];
            byte[] iv = new byte[IvSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
                rng.GetBytes(iv);
            }

            using (var kdf = new Rfc2898DeriveBytes(key, salt, Iterations, HashAlgorithmName.SHA256))
            using (var aes = Aes.Create())
            {
                aes.Key = kdf.GetBytes(KeySize);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var ms = new MemoryStream())
                {
                    ms.Write(salt, 0, salt.Length);
                    ms.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
}