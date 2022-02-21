using Harrison314.EntityFrameworkCore.Encryption.Internal.PropertyEncryptors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Tests.Internal.PropertyEncryptors
{
    [TestClass]
    public class AesGcmRandomizedPropertyEncryptorTests
    {

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(10)]
        [DataRow(1024)]
        [DataRow(1024000)]
        public void Protect(int size)
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);
            byte[] data = this.GetFastRandom(size);

            AesGcmRandomizedPropertyEncryptor encryptor = new AesGcmRandomizedPropertyEncryptor(key);

            byte[] encrypted = encryptor.Protect(data);
            Assert.IsNotNull(encrypted);
            Assert.AreNotEqual(0, encrypted.Length);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(1024)]
        public async Task ProtectsAreNoteqals(int size)
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);
            byte[] data = this.GetFastRandom(size);

            AesGcmRandomizedPropertyEncryptor encryptor = new AesGcmRandomizedPropertyEncryptor(key);

            byte[] encrypted1 = encryptor.Protect(data);
            await Task.Delay(10);
            byte[] encrypted2 = encryptor.Protect(data);
            CollectionAssert.AreNotEquivalent(encrypted1, encrypted2);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(16)]
        [DataRow(32)]
        [DataRow(1600)]
        [DataRow(1024)]
        [DataRow(1024000)]
        public void Unprotect(int size)
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);

            byte[] data = this.GetFastRandom(size);

            AesGcmRandomizedPropertyEncryptor encryptor = new AesGcmRandomizedPropertyEncryptor(key);

            byte[] encrypted = encryptor.Protect(data);
            byte[] decrypted = encryptor.Unprotect(encrypted);

            Assert.IsNotNull(decrypted);
            CollectionAssert.AreEquivalent(data, decrypted);
        }

        [TestMethod]
        public void UnprotectBrokenData()
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);

            byte[] data = this.GetFastRandom(251);

            AesGcmRandomizedPropertyEncryptor encryptor = new AesGcmRandomizedPropertyEncryptor(key);

            byte[] encrypted = encryptor.Protect(data);

            for (int i = 0; i < encrypted.Length; i++)
            {
                byte[] broken = (byte[])encrypted.Clone();
                unchecked
                {
                    broken[i] = (byte)(broken[i] + 1);
                }

                Assert.ThrowsException<EfEncryptionException>(() => encryptor.Unprotect(broken));
            }
        }

        private byte[] GetFastRandom(int size)
        {
            Random random = new Random(size * 21 + 1);
            byte[] data = new byte[size];
            random.NextBytes(data);

            return data;
        }
    }
}
