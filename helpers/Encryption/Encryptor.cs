using System.IO;
using System.Security.Cryptography;

using Newtonsoft.Json;

namespace helpers.Encryption
{
    public static class Encryptor
    {
        private static readonly Aes _aes = Aes.Create();

        public static byte[] Encrypt(byte[] input)
        {
            byte[] encrypted = null;

            using (var memory = new MemoryStream())
            using (var stream = new CryptoStream(memory, _aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(JsonConvert.SerializeObject(input));
                encrypted = memory.ToArray();
            }

            return encrypted;
        }

        public static byte[] Decrypt(byte[] input)
        {
            byte[] decrypted = null;

            using (var memory = new MemoryStream(input))
            using (var stream = new CryptoStream(memory, _aes.CreateEncryptor(), CryptoStreamMode.Read))
            using (var reader = new StreamReader(stream))
            {
                decrypted = JsonConvert.DeserializeObject<byte[]>(reader.ReadToEnd());
            }

            return decrypted;
        }
    }
}