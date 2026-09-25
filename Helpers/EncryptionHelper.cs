using System.Security.Cryptography;
using System.Text;

namespace WEBLINK_CRM.Helpers
{
    public static class EncryptionHelper
    {
        //private static readonly string SecretKey = "7F3K9P2X8M4Q6R1T5Y8N3C7V9L2B6D4A";
        private static readonly string SecretKey = "WL@SecureKey#128";

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            byte[] key = Encoding.UTF8.GetBytes(SecretKey);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);

            // AES-GCM requires a 12-byte nonce
            byte[] nonce = RandomNumberGenerator.GetBytes(12);

            // Authentication tag
            byte[] tag = new byte[16];

            byte[] ciphertext = new byte[plaintextBytes.Length];

            using (var aes = new AesGcm(key))
            {
                aes.Encrypt(
                    nonce,
                    plaintextBytes,
                    ciphertext,
                    tag
                );
            }

            // nonce + tag + ciphertext
            byte[] result = new byte[
                nonce.Length +
                tag.Length +
                ciphertext.Length
            ];

            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
            Buffer.BlockCopy(
                ciphertext,
                0,
                result,
                nonce.Length + tag.Length,
                ciphertext.Length
            );

            return Convert.ToBase64String(result)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            string base64 = encryptedText
                .Replace("-", "+")
                .Replace("_", "/");

            // Restore Base64 padding
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;

                case 3:
                    base64 += "=";
                    break;
            }

            byte[] data = Convert.FromBase64String(base64);

            // 12 nonce + 16 tag
            const int nonceSize = 12;
            const int tagSize = 16;

            if (data.Length < nonceSize + tagSize)
                throw new CryptographicException("Invalid encrypted value.");

            byte[] nonce = new byte[nonceSize];
            byte[] tag = new byte[tagSize];
            byte[] ciphertext = new byte[data.Length - nonceSize - tagSize];

            Buffer.BlockCopy(data, 0, nonce, 0, nonceSize);

            Buffer.BlockCopy(
                data,
                nonceSize,
                tag,
                0,
                tagSize
            );

            Buffer.BlockCopy(
                data,
                nonceSize + tagSize,
                ciphertext,
                0,
                ciphertext.Length
            );

            byte[] plaintext = new byte[ciphertext.Length];

            byte[] key = Encoding.UTF8.GetBytes(SecretKey);

            using (var aes = new AesGcm(key))
            {
                aes.Decrypt(
                    nonce,
                    ciphertext,
                    tag,
                    plaintext
                );
            }

            return Encoding.UTF8.GetString(plaintext);
        }

        public static string Encrypts(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(data)
                .Replace("+", "-").Replace("/", "_").Replace("=", "~");
        }

        public static string Decrypts(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return encryptedText;
            string base64 = encryptedText
                .Replace("-", "+").Replace("_", "/").Replace("~", "=");
            byte[] data = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(data);
        }



      
    }
}
