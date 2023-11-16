using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal.PropertyEncryptors
{
    public class AesGcmRandomizedPropertyEncryptor : IPropertyEncryptor
    {
        private const int SeedLen = 32;
        private const int NonceLen = 12;
        private const int TagLen = 16;
        private const int KeyLen = 32;

        private readonly byte[] key;

        public AesGcmRandomizedPropertyEncryptor(byte[] key)
        {
            this.key = key;
        }

        public byte[]? Protect(byte[] data)
        {
            byte[] reult = new byte[SeedLen + NonceLen + TagLen + data.Length];
            Span<byte> seed = reult.AsSpan(0, SeedLen);
            Span<byte> internalKey = stackalloc byte[KeyLen];

            RandomNumberGenerator.Fill(reult.AsSpan(0, SeedLen + NonceLen));

            PkcsExtensions.Algorithms.SP800_108.DeriveKey(() => new HMACSHA256(),
                this.key,
                seed,
                derivedOutput: internalKey);

            using AesGcm aesgcm = new AesGcm(internalKey, TagLen);

            aesgcm.Encrypt(reult.AsSpan(SeedLen, NonceLen),
                data,
                reult.AsSpan(SeedLen + NonceLen + TagLen),
                reult.AsSpan(SeedLen + NonceLen, TagLen),
                default);

            return reult;
        }

        public byte[]? Unprotect(byte[] data)
        {
            Span<byte> seed = data.AsSpan(0, SeedLen);
            Span<byte> internalKey = stackalloc byte[KeyLen];

            PkcsExtensions.Algorithms.SP800_108.DeriveKey(() => new HMACSHA256(),
                this.key,
                seed,
                derivedOutput: internalKey);

            byte[] plaintext = new byte[data.Length - (SeedLen + NonceLen + TagLen)];

            using AesGcm aesgcm = new AesGcm(internalKey, TagLen);
            try
            {
                aesgcm.Decrypt(data.AsSpan(SeedLen, NonceLen),
                    data.AsSpan(SeedLen + NonceLen + TagLen),
                    data.AsSpan(SeedLen + NonceLen, TagLen),
                    plaintext,
                    default);
            }
            catch (CryptographicException ex)
            {
                throw new EfEncryptionException(Strings.UnprotectEncryptedException, ex);
            }

            return plaintext;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CryptographicOperations.ZeroMemory(this.key);
            }
        }
    }
}
