using System;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    internal sealed class DefaultValuePropertyEncryptor : IPropertyEncryptor
    {
        public DefaultValuePropertyEncryptor()
        {

        }

        public byte[]? Protect(byte[] data)
        {
            return null;
        }

        public byte[]? Unprotect(byte[] data)
        {
            return null;
        }

        public void Dispose()
        {

        }
    }
}
