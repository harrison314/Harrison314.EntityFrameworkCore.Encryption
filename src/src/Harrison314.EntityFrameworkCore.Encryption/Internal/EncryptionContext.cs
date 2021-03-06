﻿using Harrison314.EntityFrameworkCore.Encryption.Internal.PropertyEncryptors;
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
        private readonly Dictionary<string, IPropertyEncryptor> cahce;

        public EncryptionContext(byte[] masterKey)
        {
            this.masterKey = masterKey;
            this.cahce = new Dictionary<string, IPropertyEncryptor>();
        }

        public IPropertyEncryptor ForProperty(string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));

            if (this.cahce.TryGetValue(purpose, out IPropertyEncryptor? propertyEncryptor))
            {
                return propertyEncryptor;
            }
            else
            {
                propertyEncryptor = this.CreateForProperty(purpose, encrypetionType, encryptionMode);
            }

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

                foreach (KeyValuePair<string, IPropertyEncryptor> cacheItem in this.cahce)
                {
                    cacheItem.Value.Dispose();
                }

                this.cahce.Clear();
            }
        }

        private IPropertyEncryptor CreateForProperty(string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (encrypetionType != EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256)
            {
                throw new InvalidProgramException($"Enum value {encrypetionType} is not supported.");
            }

            byte[] purposeBytes = Encoding.UTF8.GetBytes(purpose);
            byte[] propertyKey = new byte[32];
            PkcsExtensions.Algorithms.SP800_108.DeriveKey(() => new HMACSHA256(), this.masterKey, purposeBytes, derivedOutput: propertyKey);

            if (encryptionMode == EncryptionMode.Deterministic)
            {
                byte[] iv = new byte[16];
                PkcsExtensions.Algorithms.SP800_108.DeriveKey(() => new HMACSHA256(), this.masterKey, purposeBytes, derivedOutput: iv, counter: 4512141);

                return new DeterministicPropertyEncryptor(propertyKey, iv);
            }

            if (encryptionMode == EncryptionMode.Randomized)
            {
                return new RandomizedPropertyEncryptor(propertyKey);
            }

            throw new InvalidProgramException($"Enum value {encryptionMode} not supported.");
        }
    }
}
