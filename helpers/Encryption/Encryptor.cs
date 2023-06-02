using System.IO;
using System.Security.Cryptography;

namespace helpers.Encryption
{
    [LogSource("Encryptor")]
    public static class Encryptor
    {
        public static Aes Aes { get; } = Aes.Create();

        public static byte[] Encrypt(byte[] input)
        {
            byte[] encrypted = null;

            using (var memory = new MemoryStream())
            using (var stream = new CryptoStream(memory, Aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(input);
                writer.Write(input.Length);

                encrypted = memory.ToArray();
            }

            return encrypted;
        }

        public static byte[] Decrypt(byte[] input)
        {
            byte[] decrypted = null;

            using (var memory = new MemoryStream(input))
            using (var stream = new CryptoStream(memory, Aes.CreateEncryptor(), CryptoStreamMode.Read))
            using (var reader = new BinaryReader(stream))
            {
                decrypted = reader.ReadBytes(reader.ReadInt32());
            }

            return decrypted;
        }
    }
}