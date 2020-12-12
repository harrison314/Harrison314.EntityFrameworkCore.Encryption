using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Security;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Pkcs11Data
{
    public class Pkcs11DataProvider : IDbContextEncryptedCryptoProvider, IDisposable
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<Pkcs11DataProviderOptions> pkcs11Options;
        private readonly ILogger<Pkcs11DataProvider> logger;

        private readonly IPkcs11Library pkcs11Library;
        private ISlot slot;
        private ISession masterSession;

        public string ProviderName
        {
            get => "Pkcs11-Data";
        }

        public Pkcs11DataProvider(IServiceProvider serviceProvider, IOptions<Pkcs11DataProviderOptions> pkcs11Options, ILogger<Pkcs11DataProvider> logger)
        {
            this.serviceProvider = serviceProvider;
            this.pkcs11Options = pkcs11Options;
            this.logger = logger;

            this.ValidateKeyId(pkcs11Options.Value.MainDataKeyId);

            Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
            this.pkcs11Library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
                pkcs11Options.Value.Pkcs11LibPath,
                AppType.MultiThreaded);

            this.slot = null;
            this.masterSession = null;

            this.logger.LogDebug("Created Pkcs11DataProvider.");
        }

        public async ValueTask<byte[]> DecryptMasterKey(MasterKeyData masterKeyData, CancellationToken cancellationToken)
        {
            this.logger.LogTrace("Entering to DecryptMasterKey. KeyId: {keyId}", masterKeyData?.KeyId);

            if (masterKeyData == null) throw new ArgumentNullException(nameof(masterKeyData));

            this.ValidateKeyId(masterKeyData.KeyId);

            await this.EnshureLogged(cancellationToken);

            using ISession session = this.slot.OpenSession(SessionType.ReadOnly);

            List<IObjectAttribute> attributesTemplate = new List<IObjectAttribute>()
            {
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_DATA),
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_OBJECT_ID, masterKeyData.KeyId),
            };

            List<CKA> findAttributesTempalte = new List<CKA>()
            {
                CKA.CKA_OBJECT_ID,
                CKA.CKA_APPLICATION,
                CKA.CKA_VALUE
            };

            foreach (IObjectHandle dataHandle in session.FindAllObjects(attributesTemplate))
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<IObjectAttribute> values = session.GetAttributeValue(dataHandle, findAttributesTempalte);

                string ckaId = values[0].GetValueAsString();
                string ckaLabel = values[1].GetValueAsString();

                if (this.pkcs11Options.Value.DataObjectFilter(new DataInfo(ckaId, ckaLabel)))
                {
                    byte[] data = values[2].GetValueAsByteArray();
                    Pkcs11SecritData pkcs11SecritData = System.Text.Json.JsonSerializer.Deserialize<Pkcs11SecritData>(masterKeyData.Parameters);
                    pkcs11SecritData.Validate();

                    byte[] key = this.DerieveKey(data, pkcs11SecritData);

                    byte[] decryptedKey = new byte[masterKeyData.Data.Length];
                    using AesGcm aes = new AesGcm(key);
                    aes.Decrypt(pkcs11SecritData.AesGcmNonce, masterKeyData.Data, pkcs11SecritData.AesGcmTag, decryptedKey);

                    return decryptedKey;
                }
                else
                {
                    this.logger.LogDebug("Skip data object witj keyId: {keyId} label: {label}. Not match data object filter.", ckaId, ckaLabel);
                }
            }

            this.logger.LogError("Not found valid data object with KeyId: {keyId}", masterKeyData.KeyId);
            throw new EfEncryptionException("KeyId not found.");
        }

        public async ValueTask<MasterKeyData> EncryptMasterKey(byte[] masterKey, CancellationToken cancellationToken)
        {
            this.logger.LogTrace("Entering to EncryptMasterKey.");

            if (masterKey == null) throw new ArgumentNullException(nameof(masterKey));

            await this.EnshureLogged(cancellationToken);

            using ISession session = this.slot.OpenSession(SessionType.ReadOnly);

            Pkcs11SecritData pkcs11SeecritData = new Pkcs11SecritData()
            {
                Iterations = 10000,
                PasswordSalt = new byte[16],
                AesGcmNonce = session.GenerateRandom(AesGcm.NonceByteSizes.MaxSize),
                AesGcmTag = session.GenerateRandom(AesGcm.TagByteSizes.MaxSize)
            };

            List<IObjectAttribute> attributesTemplate = new List<IObjectAttribute>()
            {
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_DATA),
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_OBJECT_ID, this.pkcs11Options.Value.MainDataKeyId),
            };

            List<CKA> findAttributesTempalte = new List<CKA>()
            {
                CKA.CKA_OBJECT_ID,
                CKA.CKA_APPLICATION,
                CKA.CKA_VALUE
            };

            foreach (IObjectHandle dataHandle in session.FindAllObjects(attributesTemplate))
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<IObjectAttribute> values = session.GetAttributeValue(dataHandle, findAttributesTempalte);

                string ckaObjectId = values[0].GetValueAsString();
                string ckaApplication = values[1].GetValueAsString();

                if (this.pkcs11Options.Value.DataObjectFilter(new DataInfo(ckaObjectId, ckaApplication)))
                {
                    byte[] data = values[2].GetValueAsByteArray();
                    byte[] key = this.DerieveKey(data, pkcs11SeecritData);

                    byte[] encryptedKey = new byte[masterKey.Length];

                    using AesGcm aes = new AesGcm(key);
                    aes.Encrypt(pkcs11SeecritData.AesGcmNonce, masterKey, encryptedKey, pkcs11SeecritData.AesGcmTag);

                    MasterKeyData masterKeyData = new MasterKeyData()
                    {
                        Data = encryptedKey,
                        KeyId = ckaObjectId,
                        Parameters = System.Text.Json.JsonSerializer.Serialize(pkcs11SeecritData)
                    };

                    return masterKeyData;
                }
                else
                {
                    this.logger.LogDebug("Skip data object witj keyId: {keyId} label: {label}. Not match data object filter.", ckaObjectId, ckaApplication);
                }
            }

            throw new EfEncryptionException("Internal provider error.");
        }

        public async ValueTask<string> FilterAcceptKeyIds(List<string> keyIds, CancellationToken cancellationToken)
        {
            this.logger.LogTrace("Enetring to FilterAcceptKeyIds.");

            if (keyIds == null) throw new ArgumentNullException(nameof(keyIds));

            await this.EnshureLogged(cancellationToken);

            using ISession session = this.slot.OpenSession(SessionType.ReadOnly);

            List<IObjectAttribute> attributesTemplate = new List<IObjectAttribute>()
            {
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_DATA),
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
                session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true)
            };

            List<CKA> findAttributesTempalte = new List<CKA>()
            {
                CKA.CKA_OBJECT_ID,
                CKA.CKA_APPLICATION
            };

            foreach (IObjectHandle dataHandle in session.FindAllObjects(attributesTemplate))
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<IObjectAttribute> values = session.GetAttributeValue(dataHandle, findAttributesTempalte);

                string ckaObjectId = values[0].GetValueAsString();
                string ckaApplication = values[1].GetValueAsString();

                if (this.pkcs11Options.Value.DataObjectFilter(new DataInfo(ckaObjectId, ckaApplication)))
                {
                    if (keyIds.Any(t => string.Equals(t, ckaObjectId)))
                    {
                        this.logger.LogTrace("Found ckaObjectId: {ckaObjectId}", ckaObjectId);
                        return ckaObjectId;
                    }
                }
            }

            this.logger.LogDebug("Not found supported ckaObjectId.");
            return null;
        }

        public void Dispose()
        {
            this.logger.LogTrace("Entering to Dispose.");

            this.masterSession?.Dispose();
            this.pkcs11Library?.Dispose();
        }

        private byte[] DerieveKey(byte[] data, Pkcs11SecritData passwordData, int keySize = 32)
        {
            this.logger.LogTrace("Entering to DerieveKey.");

            using Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(data, passwordData.PasswordSalt, passwordData.Iterations);
            return pbkdf2.GetBytes(keySize);
        }

        private void ValidateKeyId(string keyId)
        {
            if (keyId == null)
            {
                throw new ArgumentNullException(nameof(keyId));
            }

            if (!Regex.IsMatch(keyId, "^[A-Za-z0-9_-]{5,36}$", RegexOptions.Singleline | RegexOptions.Multiline, TimeSpan.FromMilliseconds(200)))
            {
                this.logger.LogError("Key id is invalid. KeyId:{0}", keyId);
                throw new EfEncryptionException("Invalid keyId.");
            }
        }

        private async ValueTask EnshureLogged(CancellationToken cancellationToken)
        {
            this.logger.LogTrace("Enetring to EnshureLogged");

            if (this.masterSession == null)
            {
                this.logger.LogDebug("Start logging to PKCS11 device.");

                string tokenLabel = this.pkcs11Options.Value.TokenLabel;
                Func<IServiceProvider, CancellationToken, ValueTask<SecureString>> pinProvider = this.pkcs11Options.Value.PinProvider;
                this.slot = this.pkcs11Library.GetSlotList(SlotsType.WithOrWithoutTokenPresent)
                     .Single(t =>
                     {
                         try
                         {
                             string label = t.GetTokenInfo().Label;
                             return string.Equals(label, tokenLabel, StringComparison.Ordinal);
                         }
                         catch (Exception ex)
                         {
                             this.logger.LogWarning(ex, "Error with GetTokenInfo.");
                             return false;
                         }
                     });

                this.masterSession = this.slot.OpenSession(SessionType.ReadOnly);

                if (pinProvider == null)
                {
                    this.logger.LogWarning("PIN provider in options is null. Use nullptr login to PKCS11 device.");
                    pinProvider = (_, _) => new ValueTask<SecureString>(null as SecureString);
                }

                SecureString pin = await pinProvider.Invoke(this.serviceProvider, cancellationToken);
                try
                {
                    PkcsExtensions.SecureStringHelper.ExecuteWithSecureString(pin, Encoding.UTF8, rawPin =>
                    {
                        this.masterSession.Login(CKU.CKU_USER, rawPin);
                    });
                }
                finally
                {
                    pin?.Dispose();
                }

                this.logger.LogDebug("Sucessfull loged to PKCS11 device.");
            }
        }
    }
}
