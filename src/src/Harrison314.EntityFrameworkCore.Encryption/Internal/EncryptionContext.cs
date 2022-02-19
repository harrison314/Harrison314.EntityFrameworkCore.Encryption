using Harrison314.EntityFrameworkCore.Encryption.Internal.PropertyEncryptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    internal class EncryptionContext : IEncryptionContext, IDisposable
    {
        private readonly byte[] masterKey;
        //private readonly Dictionary<string, IPropertyEncryptor> cahce;

        public EncryptionContext(byte[] masterKey)
        {
            this.masterKey = masterKey;
           // this.cahce = new Dictionary<string, IPropertyEncryptor>();
        }

        public IPropertyEncryptor ForProperty(string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));

            //if (this.cahce.TryGetValue(purpose, out IPropertyEncryptor? propertyEncryptor))
            //{
            //    return propertyEncryptor;
            //}
            //else
            //{
            IPropertyEncryptor propertyEncryptor;
                propertyEncryptor = this.CreateForProperty(purpose, encrypetionType, encryptionMode);
            //}

            return propertyEncryptor;
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
                CryptographicOperations.ZeroMemory(this.masterKey);

                //foreach (KeyValuePair<string, IPropertyEncryptor> cacheItem in this.cahce)
                //{
                //    cacheItem.Value.Dispose();
                //}

                //this.cahce.Clear();
            }
        }

        private IPropertyEncryptor CreateForProperty(string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            byte[] purposeBytes = Encoding.UTF8.GetBytes(purpose);
            byte[] propertyKey = new byte[32];
            PkcsExtensions.Algorithms.SP800_108.DeriveKey(() => new HMACSHA256(), this.masterKey, purposeBytes, derivedOutput: propertyKey);

            return (encrypetionType, encryptionMode) switch
            {
                (EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Deterministic)
                    => new DeterministicPropertyEncryptor(propertyKey, this.masterKey, purposeBytes),
                (EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Randomized)
                    => new RandomizedPropertyEncryptor(propertyKey),
                (EncrypetionType.AES_GCM, EncryptionMode.Deterministic)
                    => new AesGcmDeterministicPropertyEncryptor(propertyKey, this.masterKey, purposeBytes),
                (EncrypetionType.AES_GCM, EncryptionMode.Randomized)
                    => new AesGcmRandomizedPropertyEncryptor(propertyKey),
                _ => throw new InvalidProgramException($"Enum value encrypetionType: {encrypetionType} or encryptionMode: {encryptionMode} is not supported.")
            };
        }
    }
}
