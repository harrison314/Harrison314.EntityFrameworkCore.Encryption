using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal.Lzw
{
    internal static class LzwEncoder
    {
        public static byte[] LzwCompress(byte[] iBuf)
        {
            if (iBuf == null)
            {
                throw new ArgumentNullException(nameof(iBuf));
            }

            if (iBuf.Length == 0)
            {
                return Array.Empty<byte>();
            }

            Dictionary<List<byte>, int> dictionary = new Dictionary<List<byte>, int>(new ByteListComparer());
            for (int i = 0; i < 256; i++)
            {
                List<byte> e = new List<byte> { (byte)i };
                dictionary.Add(e, i);
            }

            List<byte> window = new List<byte>();
            List<int> oBuf = new List<int>();
            foreach (byte b in iBuf)
            {
                List<byte> windowChain = new List<byte>(window) { b };
                if (dictionary.ContainsKey(windowChain))
                {
                    window.Clear();
                    window.AddRange(windowChain);
                }
                else
                {
                    if (dictionary.ContainsKey(window))
                    {
                        oBuf.Add(dictionary[window]);
                    }
                    else
                    {
                        throw new Exception("Error Encoding.");
                    }

                    dictionary.Add(windowChain, dictionary.Count);
                    window.Clear();
                    window.Add(b);
                }
            }
            if (window.Count != 0)
            {
                oBuf.Add(dictionary[window]);
            }

            return GetBytes(oBuf.ToArray());
        }

        public static byte[] LzwDecompress(byte[] Bufi)
        {
            if (Bufi == null)
            {
                throw new ArgumentNullException(nameof(Bufi));
            }

            if (Bufi.Length == 0)
            {
                return Array.Empty<byte>();
            }

            int[] iBufi = ResizeToPadding(Bufi);
            List<int> iBuf = new List<int>(iBufi);

            Dictionary<int, List<byte>> dictionary = new Dictionary<int, List<byte>>();

            for (int i = 0; i < 256; i++)
            {
                List<byte> e = new List<byte> { (byte)i };
                dictionary.Add(i, e);
            }

            List<byte> window = dictionary[iBuf[0]];
            iBuf.RemoveAt(0);

            List<byte> oBuf = new List<byte>(window);
            foreach (int k in iBuf)
            {
                List<byte> entry = new List<byte>();
                if (dictionary.ContainsKey(k))
                {
                    entry.AddRange(dictionary[k]);
                }
                else if (k == dictionary.Count)
                {
                    entry.AddRange(ConcatArrays(window.ToArray(), new[] { window.ToArray()[0] }));
                }

                if (entry.Count > 0)
                {
                    oBuf.AddRange(entry);
                    dictionary.Add(dictionary.Count, new List<byte>(ConcatArrays(window.ToArray(), new[] { entry.ToArray()[0] })));
                    window = entry;
                }
            }

            return oBuf.ToArray();
        }

        private static byte[] GetBytes(int[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            byte[] numArray = new byte[value.Length * 4];
            Buffer.BlockCopy(value, 0, numArray, 0, numArray.Length);
            return numArray;
        }

        private static byte[] ConcatArrays(byte[] left, byte[] right)
        {
            int l1 = left.Length;
            int l2 = right.Length;
            byte[] ret = new byte[l1 + l2];
            Buffer.BlockCopy(left, 0, ret, 0, l1);
            Buffer.BlockCopy(right, 0, ret, l1, l2);

            return ret;
        }

        private static int[] ResizeToPadding(byte[] ba)
        {
            int bal = ba.Length;
            int int32Count = bal / 4 + (bal % 4 == 0 ? 0 : 1);
            int[] arr = new int[int32Count];
            Buffer.BlockCopy(ba, 0, arr, 0, bal);

            return arr;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private static bool Compare(byte[] a1, byte[] a2)
        {
            if (a1 == null && a2 == null)
            {
                return true;
            }

            if (a1 == null || a2 == null || a1.Length != a2.Length)
            {
                return false;
            }

            ReadOnlySpan<UIntPtr> uintA1 = MemoryMarshal.Cast<byte, UIntPtr>(a1);
            ReadOnlySpan<UIntPtr> uintA2 = MemoryMarshal.Cast<byte, UIntPtr>(a2);

            for (int i = 0; i < uintA1.Length; i++)
            {
                if (uintA1[i] != uintA2[i])
                {
                    return false;
                }
            }

            for (int i = a1.Length - uintA1.Length * Marshal.SizeOf<UIntPtr>(); i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
