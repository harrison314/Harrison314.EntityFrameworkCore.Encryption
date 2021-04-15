using Harrison314.EntityFrameworkCore.Encryption;
using Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Dpapi;
using Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Pkcs11Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CryptoProviderExtensions
    {
        public static EncryptedContextBuilder WithPkcs11DataProvider(this EncryptedContextBuilder builder, Action<Pkcs11DataProviderOptions> setup)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            builder.ServiceCollection.Configure<Pkcs11DataProviderOptions>(setup);
            builder.ServiceCollection.AddSingleton<IDbContextEncryptedCryptoProvider, Pkcs11DataProvider>();

            return builder;
        }

        public static EncryptedContextBuilder WithDpapiProvider(this EncryptedContextBuilder builder, Action<DpapiProviderOptions> setup = null)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("DPAPI is suuported only on Windows platform.");
            }

            if (setup == null)
            {
                setup = _ => { };
            }

            builder.ServiceCollection.Configure<DpapiProviderOptions>(setup);
            builder.ServiceCollection.AddSingleton<IDbContextEncryptedCryptoProvider, DpapiDbContextEncryptedCryptoProvider>();

            return builder;
        }
    }
}
