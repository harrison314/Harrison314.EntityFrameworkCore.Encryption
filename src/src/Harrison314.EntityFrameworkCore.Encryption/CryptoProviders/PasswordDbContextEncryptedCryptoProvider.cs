using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.CryptoProviders
{
    public sealed class PasswordDbContextEncryptedCryptoProvider : IDbContextEncryptedCryptoProvider, IDisposable
    {
        private readonly byte[] passwordData;
        private const string PasswordName = "MasterPassword";

        public event EventHandler<EventArgs>? OnEmergencyKill;

        public string ProviderName
        {
            get => "Password_v1";
        }

        public PasswordDbContextEncryptedCryptoProvider(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("The parameter password cannot be an empty string.");
            }

            this.passwordData = System.Text.Encoding.UTF8.GetBytes(password);
        }

        public ValueTask<MasterKeyData> EncryptMasterKey(byte[] masterKey, CancellationToken cancellationToken)
        {
            if (masterKey == null) throw new ArgumentNullException(nameof(masterKey));

            PasswordData passwordData = new PasswordData()
            {
                Iterations = 10000,
                PasswordSalt = new byte[16],
                AesGcmNonce = new byte[AesGcm.NonceByteSizes.MaxSize],
                AesGcmTag = new byte[AesGcm.TagByteSizes.MaxSize]
            };

            RandomNumberGenerator.Fill(passwordData.PasswordSalt);
            RandomNumberGenerator.Fill(passwordData.AesGcmNonce);

            byte[] key = this.DerieveKey(passwordData);

            byte[] encryptedKey = new byte[masterKey.Length];

            using AesGcm aes = new AesGcm(key, passwordData.AesGcmTag.Length);
            aes.Encrypt(passwordData.AesGcmNonce, masterKey, encryptedKey, passwordData.AesGcmTag);

            MasterKeyData masterKeyData = new MasterKeyData()
            {
                Data = encryptedKey,
                KeyId = PasswordName,
                Parameters = System.Text.Json.JsonSerializer.Serialize(passwordData)
            };

            return new ValueTask<MasterKeyData>(masterKeyData);
        }

        public ValueTask<byte[]> DecryptMasterKey(MasterKeyData masterKeyData, CancellationToken cancellationToken)
        {
            if (masterKeyData == null) throw new ArgumentNullException(nameof(masterKeyData));

            PasswordData? passwordData = System.Text.Json.JsonSerializer.Deserialize<PasswordData>(masterKeyData.Parameters);
            if (passwordData == null)
            {
                throw new InvalidOperationException("Invalid masterKeyData parameters.");
            }

            byte[] key = this.DerieveKey(passwordData);

            byte[] decryptedKey = new byte[masterKeyData.Data.Length];
            using AesGcm aes = new AesGcm(key, passwordData.AesGcmTag.Length);
            aes.Decrypt(passwordData.AesGcmNonce, masterKeyData.Data, passwordData.AesGcmTag, decryptedKey);

            return new ValueTask<byte[]>(decryptedKey);
        }

        private byte[] DerieveKey(PasswordData passwordData, int keySize = 32)
        {
            using Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(this.passwordData, passwordData.PasswordSalt, passwordData.Iterations, HashAlgorithmName.SHA1);
            return pbkdf2.GetBytes(keySize);
        }

        public ValueTask<string> FilterAcceptKeyIds(List<string> keyIds, CancellationToken cancellationToken)
        {
            if (keyIds == null) throw new ArgumentNullException(nameof(keyIds));

            if (keyIds.Contains(PasswordName))
            {
                return new ValueTask<string>(PasswordName);
            }
            else
            {
                throw new EfEncryptionException("Not found keyId in PasswordDbContextEncryptedCryptoProvider.");
            }
        }

        public void Dispose()
        {
            CryptographicOperations.ZeroMemory(this.passwordData);
        }
    }
}
