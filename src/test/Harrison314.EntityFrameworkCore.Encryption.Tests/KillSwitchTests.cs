using Harrison314.EntityFrameworkCore.Encryption.CryptoProviders;
using Harrison314.EntityFrameworkCore.Encryption.Tests.TestModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Tests
{
    [TestClass]
    public class KillSwitchTests
    {
        [TestMethod]
        public async Task KillFromProvider()
        {
            bool hasExited = false;

            using TestModelContext context = TestModelContext.Create();
            using PasswordDbContextEncryptedCryptoProvider cryptoProvider = new PasswordDbContextEncryptedCryptoProvider("Password");
            ProxyDbContextEncryptedCryptoProvider proxyProvider = new ProxyDbContextEncryptedCryptoProvider(cryptoProvider);

            Services services = new Services(context, () => proxyProvider);
            IEncryptedContextLifetime encryptedContextLifetime = (IEncryptedContextLifetime)services.ServiceProvider.GetService(typeof(IEncryptedContextLifetime));
            encryptedContextLifetime.EmergencyKilling.Register(() => hasExited = true);

            IEncryptedScopeCreator provider = await services.GetProvider().EnshureEncrypted();
            using IDisposable scope = provider.IntoScope();

            const string Name = "John Doe";
            const string Ssi = "012546-45865-11-4585623-44";
            await context.Patients.AddAsync(new Patient()
            {
                Name = Name,
                SSI = Ssi
            });

            await context.SaveChangesAsync();

            await Task.Delay(25);
            proxyProvider.ActivateEmergencyKill();
            await Task.Delay(25);


            Assert.IsTrue(hasExited, "IEncryptedContextLifetime.EmergencyKilling has not executed.");
        }

        [TestMethod]
        public async Task KillFromProviderWithDefaultScope()
        {
            bool hasExited = false;

            using TestModelContext context = TestModelContext.Create();
            using PasswordDbContextEncryptedCryptoProvider cryptoProvider = new PasswordDbContextEncryptedCryptoProvider("Password");
            ProxyDbContextEncryptedCryptoProvider proxyProvider = new ProxyDbContextEncryptedCryptoProvider(cryptoProvider);

            Services services = new Services(context, () => proxyProvider);
            IEncryptedContextLifetime encryptedContextLifetime = (IEncryptedContextLifetime)services.ServiceProvider.GetService(typeof(IEncryptedContextLifetime));
            encryptedContextLifetime.EmergencyKilling.Register(() => hasExited = true);

            IEncryptedScopeCreator provider = services.GetProvider().EnshureDefaultValues();
            using IDisposable scope = provider.IntoScope();

            await Task.Delay(25);
            proxyProvider.ActivateEmergencyKill();
            await Task.Delay(25);

            Assert.IsTrue(hasExited, "IEncryptedContextLifetime.EmergencyKilling has not executed.");
        }

        class ProxyDbContextEncryptedCryptoProvider : IDbContextEncryptedCryptoProvider
        {
            private readonly IDbContextEncryptedCryptoProvider parent;
            public string ProviderName => "ProxyProvider";

            public event EventHandler<EventArgs> OnEmergencyKill;

            public ProxyDbContextEncryptedCryptoProvider(IDbContextEncryptedCryptoProvider parent)
            {
                this.parent = parent;
            }

            public ValueTask<byte[]> DecryptMasterKey(MasterKeyData masterKeyData, CancellationToken cancellationToken)
            {
                return this.parent.DecryptMasterKey(masterKeyData, cancellationToken);
            }

            public ValueTask<MasterKeyData> EncryptMasterKey(byte[] masterKey, CancellationToken cancellationToken)
            {
                return this.parent.EncryptMasterKey(masterKey, cancellationToken);
            }

            public ValueTask<string> FilterAcceptKeyIds(List<string> keyIds, CancellationToken cancellationToken)
            {
                return this.parent.FilterAcceptKeyIds(keyIds, cancellationToken);
            }

            internal void ActivateEmergencyKill()
            {
                this.OnEmergencyKill?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
