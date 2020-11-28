using Harrison314.EntityFrameworkCore.Encryption.Internal;
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

        public static PropertyBuilder<string> HasEncrypted(this PropertyBuilder<string> property, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));
            if (string.IsNullOrEmpty(purpose)) throw new ArgumentException($"Argument {nameof(purpose)} is empty string.");

            return property.HasConversion<byte[]>(t => EncodeAdnEncryptString(t, purpose, encrypetionType, encryptionMode),
                  t => DecryptAndDecodeString(t, purpose, encrypetionType, encryptionMode));
        }

        private static byte[] EncodeAdnEncryptString(string value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);

            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            return encryptor.Protect(data);
        }

        private static string DecryptAndDecodeString(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[] data = encryptor.Unprotect(value);
            if (data == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(data);
        }

        #endregion

        #region int

        //TODO: tests
        public static PropertyBuilder<int> HasEncrypted(this PropertyBuilder<int> property, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));
            if (string.IsNullOrEmpty(purpose)) throw new ArgumentException($"Argument {nameof(purpose)} is empty string.");

            return property.HasConversion<byte[]>(t => EncodeAdnEncryptInt32(t, purpose, encrypetionType, encryptionMode),
                  t => DecryptAndDecodeInt32(t, purpose, encrypetionType, encryptionMode));
        }

        private static byte[] EncodeAdnEncryptInt32(int value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            byte[] data = Encoding.ASCII.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            return encryptor.Protect(data);
        }

        private static int DecryptAndDecodeInt32(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[] data = encryptor.Unprotect(value);
            if (data == null)
            {
                return default;
            }

            string number = Encoding.ASCII.GetString(data);
            return int.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion

        #region long

        //TODO: tests
        public static PropertyBuilder<long> HasEncrypted(this PropertyBuilder<long> property, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));
            if (string.IsNullOrEmpty(purpose)) throw new ArgumentException($"Argument {nameof(purpose)} is empty string.");

            return property.HasConversion<byte[]>(t => EncodeAdnEncryptInt64(t, purpose, encrypetionType, encryptionMode),
                  t => DecryptAndDecodeInt64(t, purpose, encrypetionType, encryptionMode));
        }

        private static byte[] EncodeAdnEncryptInt64(long value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            byte[] data = Encoding.ASCII.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            return encryptor.Protect(data);
        }

        private static long DecryptAndDecodeInt64(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[] data = encryptor.Unprotect(value);
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

            return property.HasConversion<byte[]>(t => EncodeAdnEncryptDouble(t, purpose, encrypetionType, encryptionMode),
                  t => DecryptAndDecodeDouble(t, purpose, encrypetionType, encryptionMode));
        }

        private static byte[] EncodeAdnEncryptDouble(double value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            byte[] data = Encoding.ASCII.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            return encryptor.Protect(data);
        }

        private static double DecryptAndDecodeDouble(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[] data = encryptor.Unprotect(value);
            if (data == null)
            {
                return default;
            }

            string number = Encoding.ASCII.GetString(data);
            return double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion

        #region byte[]

        public static PropertyBuilder<byte[]> HasEncrypted(this PropertyBuilder<byte[]> property, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            if (purpose == null) throw new ArgumentNullException(nameof(purpose));
            if (string.IsNullOrEmpty(purpose)) throw new ArgumentException($"Argument {nameof(purpose)} is empty string.");

            return property.HasConversion<byte[]>(t => EncodeAdnEncryptByteArray(t, purpose, encrypetionType, encryptionMode),
                  t => DecryptAndDecodeByteArray(t, purpose, encrypetionType, encryptionMode));
        }

        private static byte[] EncodeAdnEncryptByteArray(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            return encryptor.Protect(value);
        }

        private static byte[] DecryptAndDecodeByteArray(byte[] value, string purpose, EncrypetionType encrypetionType, EncryptionMode encryptionMode)
        {
            IPropertyEncryptor encryptor = EncryptionScopeContext.Current.ForProperty(purpose, encrypetionType, encryptionMode);
            byte[] data = encryptor.Unprotect(value);

            return data;
        }

        #endregion
    }
}
