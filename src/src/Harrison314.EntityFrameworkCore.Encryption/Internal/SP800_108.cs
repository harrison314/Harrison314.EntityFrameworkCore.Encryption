using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    internal static class SP800_108
    {
        const int SHORT_BYTECOPY_THRESHOLD = 32;
        const int COUNTER_LENGTH = sizeof(uint), DERIVED_KEY_LENGTH_LENGTH = sizeof(uint);
        const int StackAllocTreshold = 84;

        public static void DeriveKey(Func<HMAC> hmacFactory, byte[] key, ReadOnlySpan<byte> label = default, ReadOnlySpan<byte> context = default, Span<byte> derivedOutput = default, uint counter = 1)
        {
            using HMAC hmac = hmacFactory();
            hmac.Key = key;

            int bufferLen = CalculateBufferLenght(label, context);

#if NETCOREAPP
            Span<byte> buffer = (bufferLen > StackAllocTreshold) ? stackalloc byte[bufferLen] : GC.AllocateUninitializedArray<byte>(bufferLen, false);
#else
            Span<byte> buffer = (bufferLen > StackAllocTreshold) ? stackalloc byte[bufferLen] : new byte[bufferLen];
#endif
            FillBuffer(buffer, label, context, checked((uint)(derivedOutput.Length << 3)));

            DeriveKey(hmac, buffer, derivedOutput, counter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculateBufferLenght(ReadOnlySpan<byte> label, ReadOnlySpan<byte> context)
        {
            return COUNTER_LENGTH + (label.Length + 1) + context.Length + DERIVED_KEY_LENGTH_LENGTH;
        }

        private static void FillBuffer(Span<byte> buffer, ReadOnlySpan<byte> label, ReadOnlySpan<byte> context, uint keyLengthInBits)
        {
            int start = COUNTER_LENGTH;
            buffer.Slice(0, start).Fill(0);
            label.CopyTo(buffer.Slice(start));
            start += label.Length;
            buffer[start] = 0;
            start++;

            context.CopyTo(buffer.Slice(start));
            start += context.Length;
            ToBeBytes(keyLengthInBits, buffer.Slice(start));
        }

        private static void ToBeBytes(uint value, Span<byte> destination)
        {
            if (BitConverter.IsLittleEndian)
            {
                Span<byte> lb = stackalloc byte[sizeof(uint)];
                BitConverter.TryWriteBytes(lb, value);
                lb.Reverse();
                lb.CopyTo(destination);
            }
            else
            {
                BitConverter.TryWriteBytes(destination, value);
            }
        }

        private static void DeriveKey(HMAC keyedHmac, Span<byte> bufferArray, Span<byte> derivedOutput, uint counter = 1)
        {
            int derivedOutputCount = derivedOutput.Length;
            int derivedOutputOffset = 0;
            Span<byte> K_i = stackalloc byte[keyedHmac.HashSize / 8];
            checked
            {
                for (uint counterStruct = counter; derivedOutputCount > 0; ++counterStruct)
                {
                    ToBeBytes(counterStruct, bufferArray);// update the counter within the buffer
                    keyedHmac.TryComputeHash(bufferArray, K_i, out _);

                    // copy the leftmost bits of K_i into the output buffer
                    int numBytesToCopy = derivedOutputCount > K_i.Length ? K_i.Length : derivedOutputCount;//Math.Min(derivedOutputCount, K_i.Length);

                    //Utils.BlockCopy(K_i, 0, derivedOutput.Array, derivedOutputOffset, numBytesToCopy);
                    for (int i = 0; i < numBytesToCopy; ++i) derivedOutput[derivedOutputOffset + i] = K_i[i];

                    derivedOutputOffset += numBytesToCopy;
                    derivedOutputCount -= numBytesToCopy;
                }// for
            }// checked
            K_i.Fill(0); /* clean up needed only when HMAC implementation is not HMAC2 */
        }// 
    }
}