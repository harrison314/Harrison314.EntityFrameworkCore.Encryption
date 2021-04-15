using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Dpapi
{
    [SupportedOSPlatform("windows")]
    public class DpapiDbContextEncryptedCryptoProvider : IDbContextEncryptedCryptoProvider
    {
        private readonly DataProtectionScope dataProtectionScope;
        private readonly string keyName;

        public string ProviderName
        {
            get => "DPAPI";
        }

        public string KeyName
        {
            get => this.keyName;
        }

        public event EventHandler<EventArgs> OnEmergencyKill;

        public DpapiDbContextEncryptedCryptoProvider(DataProtectionScope dataProtectionScope)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("DPAPI is suuported only on Windows platform.");
            }

            this.dataProtectionScope = dataProtectionScope;
            this.keyName = this.GetKeyName(dataProtectionScope);
        }

        public ValueTask<byte[]> DecryptMasterKey(MasterKeyData masterKeyData, CancellationToken cancellationToken)
        {
            if (masterKeyData == null) throw new ArgumentNullException(nameof(masterKeyData));

            if (!string.Equals(this.keyName, masterKeyData.KeyId, StringComparison.Ordinal))
            {
                throw new EfEncryptionException(""); //TODO nezhoduju sa kluce
            }

            DpapiData parameters = System.Text.Json.JsonSerializer.Deserialize<DpapiData>(masterKeyData.Parameters);

            byte[] key = ProtectedData.Unprotect(masterKeyData.Data, parameters.AdditionalEntrophy, this.dataProtectionScope);

            return new ValueTask<byte[]>(key);
        }

        public ValueTask<MasterKeyData> EncryptMasterKey(byte[] masterKey, CancellationToken cancellationToken)
        {
            if (masterKey == null) throw new ArgumentNullException(nameof(masterKey));

            byte[] entrophy = new byte[16];
            RandomNumberGenerator.Fill(entrophy);
            byte[] data = ProtectedData.Protect(masterKey, entrophy, this.dataProtectionScope);

            DpapiData parameters = new DpapiData()
            {
                AdditionalEntrophy = entrophy
            };

            MasterKeyData masterKeyData = new MasterKeyData()
            {
                Data = data,
                KeyId = this.keyName,
                Parameters = System.Text.Json.JsonSerializer.Serialize(parameters)
            };

            return new ValueTask<MasterKeyData>(masterKeyData);
        }

        public ValueTask<string> FilterAcceptKeyIds(List<string> keyIds, CancellationToken cancellationToken)
        {
            if (keyIds == null) throw new ArgumentNullException(nameof(keyIds));

            if (keyIds.Contains(this.keyName, StringComparer.OrdinalIgnoreCase))
            {
                return new ValueTask<string>(this.keyName);
            }
            else
            {
                return new ValueTask<string>(result: null);
            }
        }

        private string GetKeyName(DataProtectionScope dataProtectionScope)
        {
            string keyName = dataProtectionScope switch
            {
                DataProtectionScope.CurrentUser => string.Concat("DPAPIKEY_", Environment.MachineName, "_", Environment.UserName),
                DataProtectionScope.LocalMachine => string.Concat("DPAPIKEY_", Environment.MachineName),
                _ => throw new InvalidProgramException($"Enum value {dataProtectionScope} is not supported.")
            };

            keyName = Regex.Replace(keyName.Replace('\\', '_'), "[^A-Za-z0-9_-]", string.Empty);

            if (keyName.Length > 255)
            {
                keyName = keyName.Substring(0, 255);
            }

            return keyName;
        }
    }
}
