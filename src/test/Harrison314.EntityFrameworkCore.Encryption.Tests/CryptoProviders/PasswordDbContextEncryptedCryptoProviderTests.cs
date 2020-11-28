using Harrison314.EntityFrameworkCore.Encryption.CryptoProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Tests.CryptoProviders
{
    [TestClass]
    public class PasswordDbContextEncryptedCryptoProviderTests
    {
        [TestMethod]
        public void GetProviderName()
        {
            using PasswordDbContextEncryptedCryptoProvider provider = new PasswordDbContextEncryptedCryptoProvider("Password");
            Assert.IsNotNull(provider.ProviderName);
        }

        [TestMethod]
        public async Task EncryptMasterKey()
        {
            using PasswordDbContextEncryptedCryptoProvider provider = new PasswordDbContextEncryptedCryptoProvider("Password");

            byte[] key = new byte[32];
            Random rand = new Random(32);
            rand.NextBytes(key);

            MasterKeyData masterKey = await provider.EncryptMasterKey(key, default);
            Assert.IsNotNull(masterKey);
            Assert.IsNotNull(masterKey.Data);
            Assert.AreNotEqual(0, masterKey.Data.Length);
            Assert.IsNotNull(masterKey.KeyId);
            Assert.IsNotNull(masterKey.Parameters);
        }

        [TestMethod]
        public async Task DecryptMasterKey()
        {
            using PasswordDbContextEncryptedCryptoProvider provider = new PasswordDbContextEncryptedCryptoProvider("Password");

            byte[] key = new byte[32];
            Random rand = new Random(32);
            rand.NextBytes(key);

            MasterKeyData masterKey = await provider.EncryptMasterKey(key, default);

            string keyId = await provider.FilterAcceptKeyIds(new List<string>() { masterKey.KeyId }, default);

            Assert.AreEqual(masterKey.KeyId, keyId);

            byte[] decryptedKey = await provider.DecryptMasterKey(masterKey, default);

            CollectionAssert.AreEqual(key, decryptedKey);
        }
    }
}
