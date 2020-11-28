using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal.PropertyEncryptors
{
    public class RandomizedPropertyEncryptor : IPropertyEncryptor
    {
        private readonly byte[] key;

        public RandomizedPropertyEncryptor(byte[] key)
        {
            this.key = key;
        }

        public byte[] Protect(byte[] data)
        {
            byte[] seed = new byte[32];
            RandomNumberGenerator.Fill(seed);
            byte[] internalKey = new byte[32];

            PkcsExtensions.Algorithms.SP800_108.DeriveKey(() => new HMACSHA256(),
                this.key,
                seed,
                derivedOutput: internalKey);

            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = internalKey;
            aes.GenerateIV();

            using HMAC hmac = new HMACSHA256();
            hmac.Key = internalKey;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

            byte[] rv = new byte[32 + seed.Length + aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(seed, 0, rv, 32, seed.Length);
            Buffer.BlockCopy(aes.IV, 0, rv, 32 + seed.Length, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, rv, 32 + seed.Length + aes.IV.Length, encrypted.Length);

            hmac.TryComputeHash(rv.AsSpan(32), rv.AsSpan(0, 32), out _);

            return rv;
        }

        public byte[] Unprotect(byte[] data)
        {
            byte[] internalKey = new byte[32]; //Do sharovaneho objektu

            PkcsExtensions.Algorithms.SP800_108.DeriveKey(() => new HMACSHA256(),
                this.key,
                data.AsSpan(32, 32),
                derivedOutput: internalKey);

            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = internalKey;

            byte[] iv = new byte[16]; // dosherovaneho objektu
            Buffer.BlockCopy(data, 32 + 32, iv, 0, 16);
            aes.IV = iv;

            using HMAC hmac = new HMACSHA256();
            hmac.Key = internalKey;

            try
            {
                using ICryptoTransform decryptor = aes.CreateDecryptor();
                byte[] decrypted = decryptor.TransformFinalBlock(data, 32 + 32 + 16, data.Length - 32 - 32 - 16);
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
