using System;
using System.Collections.Generic;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal.Lzw
{
    internal class ByteListComparer : IEqualityComparer<List<byte>>
    {
        public ByteListComparer()
        {

        }

        public bool Equals(List<byte> left, List<byte> right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;

        }
        public int GetHashCode(List<byte> obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            uint hash = 0x811C9DC5;
            for (int i = 0; i < obj.Count; i++)
            {
                hash ^= obj[i];
                hash *= 0x1000193;
            }

            return (int)hash;
        }
    }
}
