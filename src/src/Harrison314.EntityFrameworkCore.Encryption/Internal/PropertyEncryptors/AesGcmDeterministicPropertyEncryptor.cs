using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal.PropertyEncryptors
{
    public class AesGcmDeterministicPropertyEncryptor : IPropertyEncryptor
    {
        private const int NonceLen = 12;
        private const int TagLen = 16;

        private byte[] key;
        private byte[] nonceAndTag;

        public AesGcmDeterministicPropertyEncryptor(byte[] key, byte[] masterKey, byte[] purposeBytes)
        {
            byte[] nonceAndTag = new byte[NonceLen + TagLen];
            PkcsExtensions.Algorithms.SP800_108.DeriveKey(() => new HMACSHA256(), masterKey, purposeBytes, derivedOutput: nonceAndTag, counter: 4512181);

            this.key = key;
            this.nonceAndTag = nonceAndTag;
        }

        public byte[]? Protect(byte[] data)
        {
            byte[] reult = new byte[data.Length];

            using AesGcm aesgcm = new AesGcm(this.key);
            aesgcm.Encrypt(this.nonceAndTag.AsSpan(0, NonceLen),
                data,
                reult,
                this.nonceAndTag.AsSpan(NonceLen, TagLen),
                default);


            return reult;
        }

        public byte[]? Unprotect(byte[] data)
        {
            byte[] plaintext = new byte[data.Length];

            using AesGcm aesgcm = new AesGcm(this.key);

            try
            {
                aesgcm.Decrypt(this.nonceAndTag.AsSpan(0, NonceLen),
                    data,
                    this.nonceAndTag.AsSpan(NonceLen, TagLen),
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
                CryptographicOperations.ZeroMemory(this.nonceAndTag);
            }
        }
    }
}
