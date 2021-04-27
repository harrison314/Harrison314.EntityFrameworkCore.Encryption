using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PkcsExtensions.X509Certificates;

namespace Harrison314.EntityFrameworkCore.Encryption.CryptoProviders
{
    public sealed class CertificateDbContextEncryptedCryptoProvider : IDbContextEncryptedCryptoProvider, IDisposable
    {
        private readonly X509Certificate2 certificate;

        public event EventHandler<EventArgs>? OnEmergencyKill;

        public string ProviderName
        {
            get => "LocalCertificate_v1";
        }

        public CertificateDbContextEncryptedCryptoProvider(X509Certificate2 certificate)
        {
            this.certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
            if (this.certificate.GetRSAPublicKey() is null)
            {
                throw new ArgumentException("Certificate does not have RSA keys.");
            }

            if (!this.certificate.IsForEncryption())
            {
                throw new ArgumentException("Certificate is not issued for encryption.");
            }
        }

        public ValueTask<byte[]> DecryptMasterKey(MasterKeyData masterKeyData, CancellationToken cancellationToken)
        {
            if (masterKeyData == null) throw new ArgumentNullException(nameof(masterKeyData));

            byte[] key = this.certificate.GetRSAPrivateKey()!.Decrypt(masterKeyData.Data, RSAEncryptionPadding.OaepSHA256);
            return new ValueTask<byte[]>(key);
        }

        public ValueTask<MasterKeyData> EncryptMasterKey(byte[] masterKey, CancellationToken cancellationToken)
        {
            if (masterKey == null) throw new ArgumentNullException(nameof(masterKey));

            MasterKeyData masterKeyData = new MasterKeyData()
            {
                Data = this.certificate.GetRSAPublicKey()!.Encrypt(masterKey, RSAEncryptionPadding.OaepSHA256),
                KeyId = this.certificate.Thumbprint,
                Parameters = "{\"RsaParams\":\"OaepSHA256\"}"
            };

            return new ValueTask<MasterKeyData>(masterKeyData);
        }

        public ValueTask<string> FilterAcceptKeyIds(List<string> keyIds, CancellationToken cancellationToken)
        {
            if (keyIds == null) throw new ArgumentNullException(nameof(keyIds));

            if (keyIds.Contains(this.certificate.Thumbprint))
            {
                return new ValueTask<string>(this.certificate.Thumbprint);
            }
            else
            {
                throw new EfEncryptionException("Not found keyId in CertificateDbContextEncryptedCryptoProvider.");
            }
        }

        public void Dispose()
        {
            this.certificate.Dispose();
        }
    }
}
