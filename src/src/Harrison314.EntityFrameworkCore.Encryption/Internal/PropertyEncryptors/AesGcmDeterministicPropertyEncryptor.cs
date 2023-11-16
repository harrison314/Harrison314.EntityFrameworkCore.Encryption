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
        private byte[] nonce;

        public AesGcmDeterministicPropertyEncryptor(byte[] key, byte[] masterKey, byte[] purposeBytes)
        {
            byte[] nonce = new byte[NonceLen];
            PkcsExtensions.Algorithms.SP800_108.DeriveKey(() => new HMACSHA256(),
                masterKey,
                purposeBytes,
                derivedOutput: nonce,
                counter: PropertyEncryptorsConstants.AesGcmDeterministicCounter);

            this.key = key;
            this.nonce = nonce;
        }

        public byte[]? Protect(byte[] data)
        {
            byte[] reult = new byte[data.Length + TagLen];

            using AesGcm aesgcm = new AesGcm(this.key, TagLen);
            aesgcm.Encrypt(this.nonce.AsSpan(),
                data,
                reult.AsSpan(TagLen),
                reult.AsSpan(0, TagLen),
                default);

            return reult;
        }

        public byte[]? Unprotect(byte[] data)
        {
            byte[] plaintext = new byte[data.Length - TagLen];

            using AesGcm aesgcm = new AesGcm(this.key, TagLen);

            try
            {
                aesgcm.Decrypt(this.nonce.AsSpan(),
                    data.AsSpan(TagLen),
                    data.AsSpan(0, TagLen),
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
                CryptographicOperations.ZeroMemory(this.nonce);
            }
        }
    }
}
