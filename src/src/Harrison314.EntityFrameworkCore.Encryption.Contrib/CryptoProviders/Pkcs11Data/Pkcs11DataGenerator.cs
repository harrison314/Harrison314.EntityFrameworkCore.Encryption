using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Security;
using System.Security.Cryptography;
using PkcsExtensions.Algorithms;

namespace Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Pkcs11Data
{
    public class Pkcs11DataGenerator : IDisposable
    {
        private readonly IPkcs11Library pkcs11Library;
        private readonly ISlot slot;
        private readonly ISession masterSession;

        public Pkcs11DataGenerator(string pkcs11LibPath, string tokenLabel, SecureString pin)
        {
            if (pkcs11LibPath == null) throw new ArgumentNullException(nameof(pkcs11LibPath));
            if (tokenLabel == null) throw new ArgumentNullException(nameof(tokenLabel));
            if (pin == null) throw new ArgumentNullException(nameof(pin));

            Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
            this.pkcs11Library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
                pkcs11LibPath,
                AppType.MultiThreaded);

            this.slot = this.pkcs11Library.GetSlotList(SlotsType.WithOrWithoutTokenPresent)
                    .Single(t =>
                    {
                        try
                        {
                            string label = t.GetTokenInfo().Label;
                            return string.Equals(label, tokenLabel, StringComparison.Ordinal);
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    });

            this.masterSession = this.slot.OpenSession(SessionType.ReadOnly);
            PkcsExtensions.SecureStringHelper.ExecuteWithSecureString(pin, Encoding.UTF8, rawPin =>
            {
                this.masterSession.Login(CKU.CKU_USER, rawPin);
            });
        }

        public void GenerateDataObject(string label, string ckaId, int dataSize = 32)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            if (ckaId == null) throw new ArgumentNullException(nameof(ckaId));

            using ISession session = this.slot.OpenSession(SessionType.ReadWrite);

            using DigestRandomGenerator generator = new DigestRandomGenerator(HashAlgorithmName.SHA256);
            generator.GenerateSeed(32);
            generator.AddSeedMaterial(session.GenerateRandom(32));

            byte[] data = new byte[dataSize];

            try
            {
                generator.NextBytes(data);

                List<IObjectAttribute> attributeValues = new List<IObjectAttribute>()
                {
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_DATA),
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckaId),
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_APPLICATION, "Harrison314.Encryption"),
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, data)
                };

                session.CreateObject(attributeValues);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(data);
            }
        }

        public void Dispose()
        {
            this.masterSession?.Dispose();
            this.pkcs11Library?.Dispose();
        }
    }
}
