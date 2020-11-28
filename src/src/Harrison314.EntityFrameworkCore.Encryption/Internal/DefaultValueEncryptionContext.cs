using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    internal class DefaultValueEncryptionContext : IEncryptionContext
    {
        private readonly DefaultValuePropertyEncryptor encryptor;

        public DefaultValueEncryptionContext()
        {
            this.encryptor = new DefaultValuePropertyEncryptor();
        }

        public IPropertyEncryptor ForProperty(string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            return this.encryptor;
        }
    }
}
