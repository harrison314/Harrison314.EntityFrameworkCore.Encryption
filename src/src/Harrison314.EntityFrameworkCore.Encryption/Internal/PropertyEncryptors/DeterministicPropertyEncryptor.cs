using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal.PropertyEncryptors
{
    public class DeterministicPropertyEncryptor : IPropertyEncryptor
    {
        private byte[] key;
        private byte[] iv;

        public DeterministicPropertyEncryptor(byte[] key, byte[] iv)
        {
            this.key = key;
            this.iv = iv;
        }

        public byte[] Protect(byte[] data)
        {
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = this.key;
            aes.IV = this.iv;

            using HMAC hmac = new HMACSHA256();
            hmac.Key = this.key;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
            // byte[] aead = hmac.ComputeHash(encrypted); //TODO compute inplace

            byte[] rv = new byte[32 + encrypted.Length];
            // System.Buffer.BlockCopy(aead, 0, rv, 0, aead.Length);
            System.Buffer.BlockCopy(encrypted, 0, rv, 32, encrypted.Length);
            hmac.TryComputeHash(encrypted, rv.AsSpan(0, 32), out _);

            return rv;
        }

        public byte[] Unprotect(byte[] data)
        {
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = this.key;
            aes.IV = this.iv;

            using HMAC hmac = new HMACSHA256();
            hmac.Key = this.key;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            try
            {
                byte[] decrypted = decryptor.TransformFinalBlock(data, 32, data.Length - 32);
                byte[] aead = hmac.ComputeHash(data, 32, data.Length - 32);

                if (!ByteArrayUtils.StartWith(data, aead))
                {
                    throw new EfEncryptionException(Strings.UnprotectEncryptedException);
                }

                return decrypted;
            }
            catch (CryptographicException ex)
            {
                throw new EfEncryptionException(Strings.UnprotectEncryptedException, ex);
            }
        }
    }
}
