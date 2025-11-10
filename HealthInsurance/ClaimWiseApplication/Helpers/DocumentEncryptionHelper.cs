using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace ClaimWise.Application.Helpers
{
    public static class DocumentEncryptionHelper
    {
        public static byte[] Encrypt(byte[] data, string key)
        {
            using var aes = Aes.Create();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            aes.Key = keyBytes.Take(32).ToArray();
            aes.IV = keyBytes.Take(16).ToArray();

            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public static byte[] Decrypt(byte[] encryptedData, string key)
        {
            using var aes = Aes.Create();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            aes.Key = keyBytes.Take(32).ToArray();
            aes.IV = keyBytes.Take(16).ToArray();

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }
    }


}
