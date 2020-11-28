using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    public interface IEncryptionContext
    {
        IPropertyEncryptor ForProperty(string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode);
    }
}
