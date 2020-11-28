using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    internal static class ByteArrayUtils
    {
        public static bool StartWith(byte[] array, byte[] startWith)
        {
            if (array.Length < startWith.Length)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(startWith.AsSpan(), array.AsSpan(0, startWith.Length));
        }
    }
}
