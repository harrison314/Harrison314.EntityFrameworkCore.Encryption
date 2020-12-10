using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Pkcs11Data
{
    public class Pkcs11DataProviderOptions
    {
        public string Pkcs11LibPath
        {
            get;
            set;
        }

        public string TokenLabel
        {
            get;
            set;
        }

        public Predicate<(string, string)> DataObjectFilter
        {
            get;
            set;
        }

        public Func<IServiceProvider, CancellationToken, ValueTask<SecureString>> PinProvider
        {
            get;
            set;
        }

        public string MainDataKeyId
        {
            get;
            set;
        }

        public Pkcs11DataProviderOptions()
        {

        }
    }
}
