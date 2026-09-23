using System.Text;

namespace WEBLINK_CRM.Helpers
{
    public static class EncryptionHelper
    {
        private static readonly string Key = "WL@SecureKey#128";

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(data)
                .Replace("+", "-").Replace("/", "_").Replace("=", "~");
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return encryptedText;
            string base64 = encryptedText
                .Replace("-", "+").Replace("_", "/").Replace("~", "=");
            byte[] data = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(data);
        }
    }
}
