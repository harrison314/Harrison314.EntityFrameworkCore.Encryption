using Harrison314.EntityFrameworkCore.Encryption;
using Harrison314.EntityFrameworkCore.Encryption.CryptoProviders;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EncryptedContextBuilderExtensions
    {
        public static EncryptedContextBuilder WithProvider<TProvider>(this EncryptedContextBuilder builder)
            where TProvider : class, IDbContextEncryptedCryptoProvider
        {
            ServiceDescriptor descriptor = new ServiceDescriptor(typeof(IDbContextEncryptedCryptoProvider),
                typeof(TProvider),
                ServiceLifetime.Singleton);

            builder.ServiceCollection.Add(descriptor);
            return builder;
        }

        public static EncryptedContextBuilder WithPasswordEncryptionProvider(this EncryptedContextBuilder builder, string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(password)) throw new ArgumentException(nameof(password));

            builder.ServiceCollection.AddTransient<IDbContextEncryptedCryptoProvider>(_ => new PasswordDbContextEncryptedCryptoProvider(password));

            return builder;
        }

        public static EncryptedContextBuilder WithCertificateEncryptionProvider(this EncryptedContextBuilder builder, Func<IServiceProvider, X509Certificate2> certificateFactory)
        {
            if (certificateFactory == null) throw new ArgumentNullException(nameof(certificateFactory));

            builder.ServiceCollection.AddTransient<IDbContextEncryptedCryptoProvider>(sp => new CertificateDbContextEncryptedCryptoProvider(certificateFactory.Invoke(sp)));

            return builder;
        }

        public static EncryptedContextBuilder WithCertificateEncryptionProvider(this EncryptedContextBuilder builder, string thumbprint, CertStoreType storeType)
        {
            if (thumbprint == null) throw new ArgumentNullException(nameof(thumbprint));

            StoreLocation location = storeType switch
            {
                CertStoreType.MyCurrentUser => StoreLocation.CurrentUser,
                CertStoreType.MyLocalMachine => StoreLocation.LocalMachine,
                _ => throw new InvalidProgramException($"Enum value {storeType} is not supported.")
            };

            X509Certificate2? certificate = null;
            using (X509Store store = new X509Store(StoreName.My, location))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                certificate = (collection.Count > 0) ? collection[0] : null;
            }

            if (certificate == null)
            {
                throw new EfEncryptionException($"Certificate with thumprint {thumbprint} not found in {storeType}.");
            }

            builder.ServiceCollection.AddTransient<IDbContextEncryptedCryptoProvider>(sp => new CertificateDbContextEncryptedCryptoProvider(certificate));

            return builder;
        }
    }
}
