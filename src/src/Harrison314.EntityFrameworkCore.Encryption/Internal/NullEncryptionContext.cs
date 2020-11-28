using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    internal class NullEncryptionContext : IEncryptionContext
    {
        public NullEncryptionContext()
        {

        }

        public IPropertyEncryptor ForProperty(string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            throw new EfEncryptionScopeNotFoundException(Strings.EncryptionScopeNotFound);
        }
    }
}
