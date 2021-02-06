using Harrison314.EntityFrameworkCore.Encryption.Internal;
using Harrison314.EntityFrameworkCore.Encryption.Internal.GZip;
using Harrison314.EntityFrameworkCore.Encryption.Internal.Lzw;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Extensions
{
    public static class PropertyBuilderExtensions
    {
        #region String

        public static PropertyBuilder<string> HasEncrypted(this PropertyBuilder<string> property, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode, CompressionMode compressionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));
            if (string.IsNullOrEmpty(purpose)) throw new ArgumentException($"Argument {nameof(purpose)} is empty string.");

            return property.HasConversion<byte[]>(t => EncodeAdnEncryptString(t, purpose, encrypetionType, encryptionMode, compressionMode),
#pragma warning disable CS8603 // Possible null reference return.
                  t => DecryptAndDecodeString(t, purpose, encrypetionType, encryptionMode, compressionMode));
#pragma warning restore CS8603 // Possible null reference return.
        }

        private static byte[] EncodeAdnEncryptString(string value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode, CompressionMode compressionMode)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            byte[] compressedData = compressionMode switch
            {
                CompressionMode.None => data,
                CompressionMode.Lzw => LzwEncoder.LzwCompress(data),
                CompressionMode.GZip => GzipEncoder.Compress(data),
                _ => throw new InvalidProgramException($"Enum value {compressionMode} not supported.")
            };

            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
#pragma warning disable CS8603 // Possible null reference return.
            return encryptor.Protect(compressedData);
#pragma warning restore CS8603 // Possible null reference return.
        }

        private static string? DecryptAndDecodeString(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode, CompressionMode compressionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[]? data = encryptor.Unprotect(value);
            if (data == null)
            {
                return null;
            }

            byte[] decompressedData = compressionMode switch
            {
                CompressionMode.None => data,
                CompressionMode.Lzw => LzwEncoder.LzwDecompress(data),
                CompressionMode.GZip => GzipEncoder.Decompress(data),
                _ => throw new InvalidProgramException($"Enum value {compressionMode} not supported.")
            };

            return Encoding.UTF8.GetString(decompressedData);
        }

        #endregion

        #region int

        //TODO: tests
        public static PropertyBuilder<int> HasEncrypted(this PropertyBuilder<int> property, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));
            if (string.IsNullOrEmpty(purpose)) throw new ArgumentException($"Argument {nameof(purpose)} is empty string.");

#pragma warning disable CS8603 // Possible null reference return.
            return property.HasConversion<byte[]>(t => EncodeAdnEncryptInt32(t, purpose, encrypetionType, encryptionMode),
#pragma warning restore CS8603 // Possible null reference return.
                  t => DecryptAndDecodeInt32(t, purpose, encrypetionType, encryptionMode));
        }

        private static byte[]? EncodeAdnEncryptInt32(int value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            byte[] data = Encoding.ASCII.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            return encryptor.Protect(data);
        }

        private static int DecryptAndDecodeInt32(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[]? data = encryptor.Unprotect(value);
            if (data == null)
            {
                return default;
            }

            string number = Encoding.ASCII.GetString(data);
            return int.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion

        #region long

        public static PropertyBuilder<long> HasEncrypted(this PropertyBuilder<long> property, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));
            if (string.IsNullOrEmpty(purpose)) throw new ArgumentException($"Argument {nameof(purpose)} is empty string.");

#pragma warning disable CS8603 // Possible null reference return.
            return property.HasConversion<byte[]>(t => EncodeAdnEncryptInt64(t, purpose, encrypetionType, encryptionMode),
#pragma warning restore CS8603 // Possible null reference return.
                  t => DecryptAndDecodeInt64(t, purpose, encrypetionType, encryptionMode));
        }

        private static byte[]? EncodeAdnEncryptInt64(long value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            byte[] data = Encoding.ASCII.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            return encryptor.Protect(data);
        }

        private static long DecryptAndDecodeInt64(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[]? data = encryptor.Unprotect(value);
            if (data == null)
            {
                return default;
            }

            string number = Encoding.ASCII.GetString(data);
            return long.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion

        #region double

        // TODO: Tests
        public static PropertyBuilder<double> HasEncrypted(this PropertyBuilder<double> property, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));
            if (string.IsNullOrEmpty(purpose)) throw new ArgumentException($"Argument {nameof(purpose)} is empty string.");

#pragma warning disable CS8603 // Possible null reference return.
            return property.HasConversion<byte[]>(t => EncodeAdnEncryptDouble(t, purpose, encrypetionType, encryptionMode),
                  t => DecryptAndDecodeDouble(t, purpose, encrypetionType, encryptionMode));
#pragma warning restore CS8603 // Possible null reference return.
        }

        private static byte[]? EncodeAdnEncryptDouble(double value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            byte[] data = Encoding.ASCII.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            return encryptor.Protect(data);
        }

        private static double DecryptAndDecodeDouble(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[]? data = encryptor.Unprotect(value);
            if (data == null)
            {
                return default;
            }

            string number = Encoding.ASCII.GetString(data);
            return double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion

        #region byte[]

        public static PropertyBuilder<byte[]> HasEncrypted(this PropertyBuilder<byte[]> property, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode, CompressionMode compressionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));
            if (string.IsNullOrEmpty(purpose)) throw new ArgumentException($"Argument {nameof(purpose)} is empty string.");

#pragma warning disable CS8603 // Possible null reference return.
            return property.HasConversion<byte[]>(t => EncodeAdnEncryptByteArray(t, purpose, encrypetionType, encryptionMode, compressionMode),
                  t => DecryptAndDecodeByteArray(t, purpose, encrypetionType, encryptionMode, compressionMode));
#pragma warning restore CS8603 // Possible null reference return.
        }

        private static byte[]? EncodeAdnEncryptByteArray(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode, CompressionMode compressionMode)
        {
            byte[] compressedData = compressionMode switch
            {
                CompressionMode.None => value,
                CompressionMode.Lzw => LzwEncoder.LzwCompress(value),
                CompressionMode.GZip => GzipEncoder.Compress(value),
                _ => throw new InvalidProgramException($"Enum value {compressionMode} not supported.")
            };

            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            return encryptor.Protect(compressedData);
        }

        private static byte[]? DecryptAndDecodeByteArray(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode, CompressionMode compressionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[]? data = encryptor.Unprotect(value);
            if (data == null)
            {
                return null;
            }

            return compressionMode switch
            {
                CompressionMode.None => data,
                CompressionMode.Lzw => LzwEncoder.LzwDecompress(data),
                CompressionMode.GZip => GzipEncoder.Decompress(data),
                _ => throw new InvalidProgramException($"Enum value {compressionMode} not supported.")
            };
        }

        #endregion
    }
}
