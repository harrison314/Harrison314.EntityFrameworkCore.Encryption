using FluentAssertions;
using Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Dpapi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Contrib.Tests.CryptoProviders.Dpapi
{
    [TestClass]
    [SupportedOSPlatform("windows")]
    public class DpapiDbContextEncryptedCryptoProviderTests
    {
        [TestMethod]
        public async Task EncryptMasterKey()
        {
            this.AreWindowsPlatform();

            DpapiDbContextEncryptedCryptoProvider provider = new DpapiDbContextEncryptedCryptoProvider(System.Security.Cryptography.DataProtectionScope.CurrentUser);

            byte[] masterKey = new byte[32];
            this.DeterministicFill(masterKey);

            Encryption.MasterKeyData masterKeyData = await provider.EncryptMasterKey(masterKey, default);

            masterKeyData.Should().NotBeNull();
            masterKeyData.Data.Should().NotBeNullOrEmpty();
            masterKeyData.KeyId.Should().NotBeNullOrEmpty();
            masterKeyData.Parameters.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task DecryptMasterKey()
        {
            this.AreWindowsPlatform();

            DpapiDbContextEncryptedCryptoProvider provider = new DpapiDbContextEncryptedCryptoProvider(System.Security.Cryptography.DataProtectionScope.CurrentUser);

            byte[] masterKey = new byte[32];
            this.DeterministicFill(masterKey);

            Encryption.MasterKeyData masterKeyData = await provider.EncryptMasterKey(masterKey, default);

            byte[] decrypted = await provider.DecryptMasterKey(masterKeyData, default);

            decrypted.Should().NotBeNull().And.NotBeEmpty().And.BeEquivalentTo(masterKey);
        }

        [TestMethod]
        public async Task FilterAcceptKeyIds_Success()
        {
            this.AreWindowsPlatform();

            DpapiDbContextEncryptedCryptoProvider provider = new DpapiDbContextEncryptedCryptoProvider(System.Security.Cryptography.DataProtectionScope.CurrentUser);
            List<string> potencialKeys = new List<string>()
            {
                "TestKey1",
                "TestKey2",
                provider.KeyName,
                "TestKey3"
            };

            string selectedKeyId = await provider.FilterAcceptKeyIds(potencialKeys, default);

            selectedKeyId.Should().Be(provider.KeyName);
        }

        [TestMethod]
        public async Task FilterAcceptKeyIds_Failed()
        {
            this.AreWindowsPlatform();

            DpapiDbContextEncryptedCryptoProvider provider = new DpapiDbContextEncryptedCryptoProvider(System.Security.Cryptography.DataProtectionScope.CurrentUser);
            List<string> potencialKeys = new List<string>()
            {
                "TestKey1",
                "TestKey2",
                "TestKey3"
            };

            string selectedKeyId = await provider.FilterAcceptKeyIds(potencialKeys, default);

            selectedKeyId.Should().BeNull();
        }

        private void DeterministicFill(byte[] data)
        {
            Random rand = new Random(42);
            rand.NextBytes(data);
        }

        private void AreWindowsPlatform()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Tests woring only on windows.");
            }
        }
    }
}
