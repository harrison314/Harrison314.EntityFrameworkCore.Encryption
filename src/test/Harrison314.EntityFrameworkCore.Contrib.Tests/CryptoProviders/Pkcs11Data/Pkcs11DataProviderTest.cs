using FluentAssertions;
using Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Pkcs11Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SoftHSMv2ForTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Contrib.Tests.CryptoProviders.Pkcs11Data
{
    [TestClass]
    public class Pkcs11DataProviderTest
    {
        public const string TokenName = "TestCardToken";
        public const string TokenSoPin = "abcdef";
        public const string TokenUserPin = "abc123*!~";

        private static SoftHsmContext softHsmContext = null;

        public static string Pkcs11LibPath
        {
            get => softHsmContext.Pkcs11LibPath;
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            string deployPath = Path.Combine(Path.GetTempPath(), $"SoftHSMv2-{Guid.NewGuid():D}");
            softHsmContext = SoftHsmInitializer.Init(opt =>
            {
                opt.DeployFolder = deployPath;

                opt.LabelName = TokenName;
                opt.Pin = TokenUserPin;
                opt.SoPin = TokenSoPin;
            });

            context.WriteLine("Deploy SoftHSMv2 to {0}", deployPath);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            softHsmContext?.Dispose();
        }

        [TestMethod]
        public void GenerateData()
        {
            using Pkcs11DataGenerator pkcs11DataGenerator = new Pkcs11DataGenerator(Pkcs11LibPath,
                TokenName,
                this.GetPin());

            pkcs11DataGenerator.GenerateDataObject("TestGenerateData", Guid.NewGuid().ToString("D"));
        }

        [TestMethod]
        public async Task EncryptMasterKey()
        {
            string masterKeyId = Guid.NewGuid().ToString("D");
            string label = "TestLabel";

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Loose);
            Mock<ILogger<Pkcs11DataProvider>> loggerMock = new Mock<ILogger<Pkcs11DataProvider>>(MockBehavior.Loose);
            Pkcs11DataProviderOptions options = new Pkcs11DataProviderOptions()
            {
                DataObjectFilter = info => string.Equals(info.Label, label, StringComparison.Ordinal),
                MainDataKeyId = masterKeyId,
                PinProvider = (_, _) => new ValueTask<SecureString>(this.GetPin()),
                Pkcs11LibPath = Pkcs11LibPath,
                TokenLabel = TokenName
            };

            using (Pkcs11DataGenerator pkcs11DataGenerator = new Pkcs11DataGenerator(Pkcs11LibPath,
                TokenName,
                this.GetPin()))
            {
                pkcs11DataGenerator.GenerateDataObject(label, masterKeyId);
            }

            using Pkcs11DataProvider provider = new Pkcs11DataProvider(serviceProviderMock.Object,
                Options.Create(options),
                loggerMock.Object);

            byte[] masterKey = new byte[32];
            this.DeterministicFill(masterKey);

            Encryption.MasterKeyData masterKeyData = await provider.EncryptMasterKey(masterKey, default);

            masterKeyData.Should().NotBeNull();
            masterKeyData.Data.Should().NotBeNullOrEmpty();
            masterKeyData.KeyId.Should().NotBeNullOrEmpty().And.BeEquivalentTo(masterKeyId);
            masterKeyData.Parameters.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task DecryptMasterKey()
        {
            string masterKeyId = Guid.NewGuid().ToString("D");
            string label = "TestLabel";

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Loose);
            Mock<ILogger<Pkcs11DataProvider>> loggerMock = new Mock<ILogger<Pkcs11DataProvider>>(MockBehavior.Loose);
            Pkcs11DataProviderOptions options = new Pkcs11DataProviderOptions()
            {
                DataObjectFilter = info => string.Equals(info.Label, label, StringComparison.Ordinal),
                MainDataKeyId = masterKeyId,
                PinProvider = (_, _) => new ValueTask<SecureString>(this.GetPin()),
                Pkcs11LibPath = Pkcs11LibPath,
                TokenLabel = TokenName
            };

            using (Pkcs11DataGenerator pkcs11DataGenerator = new Pkcs11DataGenerator(Pkcs11LibPath,
                TokenName,
                this.GetPin()))
            {
                pkcs11DataGenerator.GenerateDataObject(label, masterKeyId);
            }

            using Pkcs11DataProvider provider = new Pkcs11DataProvider(serviceProviderMock.Object,
                Options.Create(options),
                loggerMock.Object);

            byte[] masterKey = new byte[32];
            this.DeterministicFill(masterKey);

            Encryption.MasterKeyData masterKeyData = await provider.EncryptMasterKey(masterKey, default);

            byte[] decrypted = await provider.DecryptMasterKey(masterKeyData, default);

            decrypted.Should().NotBeNull().And.NotBeEmpty().And.BeEquivalentTo(masterKey);
        }

        [TestMethod]
        public async Task FilterAcceptKeyIds_Success()
        {
            string masterKeyId = Guid.NewGuid().ToString("D");
            string label = "TestLabel";

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Loose);
            Mock<ILogger<Pkcs11DataProvider>> loggerMock = new Mock<ILogger<Pkcs11DataProvider>>(MockBehavior.Loose);
            Pkcs11DataProviderOptions options = new Pkcs11DataProviderOptions()
            {
                DataObjectFilter = info => string.Equals(info.Label, label, StringComparison.Ordinal),
                MainDataKeyId = masterKeyId,
                PinProvider = (_, _) => new ValueTask<SecureString>(this.GetPin()),
                Pkcs11LibPath = Pkcs11LibPath,
                TokenLabel = TokenName
            };

            using (Pkcs11DataGenerator pkcs11DataGenerator = new Pkcs11DataGenerator(Pkcs11LibPath,
                TokenName,
                this.GetPin()))
            {
                pkcs11DataGenerator.GenerateDataObject(label, Guid.NewGuid().ToString("D"));
                pkcs11DataGenerator.GenerateDataObject(label, masterKeyId);
                pkcs11DataGenerator.GenerateDataObject(label, Guid.NewGuid().ToString("D"));
            }

            List<string> potencialKeys = new List<string>()
            {
                "TestKey1",
                "TestKey2",
                masterKeyId,
                "TestKey3"
            };

            using Pkcs11DataProvider provider = new Pkcs11DataProvider(serviceProviderMock.Object,
                Options.Create(options),
                loggerMock.Object);

            string selectedKeyId = await provider.FilterAcceptKeyIds(potencialKeys, default);

            selectedKeyId.Should().BeEquivalentTo(masterKeyId);
        }

        [TestMethod]
        public async Task FilterAcceptKeyIds_Failed()
        {
            string masterKeyId = Guid.NewGuid().ToString("D");
            string label = "TestLabel";

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Loose);
            Mock<ILogger<Pkcs11DataProvider>> loggerMock = new Mock<ILogger<Pkcs11DataProvider>>(MockBehavior.Loose);
            Pkcs11DataProviderOptions options = new Pkcs11DataProviderOptions()
            {
                DataObjectFilter = info => string.Equals(info.Label, label, StringComparison.Ordinal),
                MainDataKeyId = masterKeyId,
                PinProvider = (_, _) => new ValueTask<SecureString>(this.GetPin()),
                Pkcs11LibPath = Pkcs11LibPath,
                TokenLabel = TokenName
            };

            using (Pkcs11DataGenerator pkcs11DataGenerator = new Pkcs11DataGenerator(Pkcs11LibPath,
                TokenName,
                this.GetPin()))
            {
                pkcs11DataGenerator.GenerateDataObject(label, Guid.NewGuid().ToString("D"));
                pkcs11DataGenerator.GenerateDataObject(label, masterKeyId);
                pkcs11DataGenerator.GenerateDataObject(label, Guid.NewGuid().ToString("D"));
            }

            List<string> potencialKeys = new List<string>()
            {
                "TestKey1",
                "TestKey2",
                "TestKey3"
            };

            using Pkcs11DataProvider provider = new Pkcs11DataProvider(serviceProviderMock.Object,
                Options.Create(options),
                loggerMock.Object);

            string selectedKeyId = await provider.FilterAcceptKeyIds(potencialKeys, default);

            selectedKeyId.Should().BeNull();
        }

        private SecureString GetPin()
        {
            SecureString secureString = new SecureString();
            foreach (char c in TokenUserPin)
            {
                secureString.AppendChar(c);
            }

            return secureString;
        }

        private void DeterministicFill(byte[] data)
        {
            Random rand = new Random(42);
            rand.NextBytes(data);
        }
    }
}
