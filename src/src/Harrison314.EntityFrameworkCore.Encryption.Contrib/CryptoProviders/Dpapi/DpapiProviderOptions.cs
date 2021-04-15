using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Dpapi
{
    [SupportedOSPlatform("windows")]
    public class DpapiProviderOptions
    {
        public DataProtectionScope Scope
        {
            get;
            set;
        }

        public DpapiProviderOptions()
        {
            this.Scope = DataProtectionScope.CurrentUser;
        }
    }
}
