using Harrison314.EntityFrameworkCore.Encryption.Tests.TestModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Tests
{
    [TestClass]
    public class DbContextUsageTests
    {
        [TestMethod]
        public async Task UsingWithoutScope()
        {
            using TestModelContext context = TestModelContext.Create();
            await context.Cities.AddAsync(new CityTest()
            {
                Name = "Bratislava",
                ShortName = "BA"
            });

            await context.SaveChangesAsync();

            CityTest city = await context.Cities.AsNoTracking().FirstAsync();
            Assert.IsNotNull(city);

            city.ShortName = "BL";

            await context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task EncryptedWithoutScope()
        {
            using TestModelContext context = TestModelContext.Create();
            await context.Patients.AddAsync(new Patient()
            {
                Name = "John Doe",
                SSI = "000000"
            });

            await Assert.ThrowsExceptionAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
        }

        [TestMethod]
        public async Task Encrypted()
        {
            const string Name = "John Doe";
            const string Ssi = "012546-45865-11-4585623-44";

            using TestModelContext context = TestModelContext.Create();
            Services services = new Services(context);

            IEncryptedScopeCreator provider = await services.GetProvider().EnshureEncrypted();
            using IDisposable scope = provider.IntoScope();

            await context.Patients.AddAsync(new Patient()
            {
                Name = Name,
                SSI = Ssi
            });

            await context.SaveChangesAsync();
            Patient patient = await context.Patients.FirstAsync();
            Assert.AreEqual(Name, patient.Name);
            Assert.AreEqual(Ssi, patient.SSI);
            patient.SSI = "Ssdi2";

            await context.SaveChangesAsync();

            Patient patient2 = await context.Patients.AsNoTracking().FirstAsync();
            Assert.AreEqual("Ssdi2", patient2.SSI);
        }

        [TestMethod]
        public async Task FindByEncrypted()
        {
            const string Name = "John Doe";
            const string Ssi = "012546-45865-11-4585623-44";

            using TestModelContext context = TestModelContext.Create();
            Services services = new Services(context);

            IEncryptedScopeCreator provider = await services.GetProvider().EnshureEncrypted();
            using IDisposable scope = provider.IntoScope();

            await context.Patients.AddAsync(new Patient()
            {
                Name = Name,
                SSI = Ssi
            });

            await context.Patients.AddAsync(new Patient()
            {
                Name = "Any name",
                SSI = "455666663322555"
            });

            await context.Patients.AddAsync(new Patient()
            {
                Name = "Other Man",
                SSI = "45566664584263322555"
            });

            await context.SaveChangesAsync();
            Patient patient = await context.Patients.AsNoTracking().Where(t => t.SSI == Ssi).SingleAsync();

            Assert.AreEqual(Name, patient.Name);
        }

        [TestMethod]
        public async Task EnshureUnencrpted()
        {
            const string Name = "John Doe";
            const string Ssi = "012546-45865-11-4585623-44";

            using TestModelContext context = TestModelContext.Create();
            Services services = new Services(context);

            IEncryptedScopeCreator provider = await services.GetProvider().EnshureEncrypted();
            using (IDisposable scope = provider.IntoScope())
            {

                await context.Patients.AddAsync(new Patient()
                {
                    Name = Name,
                    SSI = Ssi
                });

                await context.SaveChangesAsync();
            }

            using (IDisposable scope = services.GetProvider().EnshureDefaultValues().IntoScope())
            {
                Patient patient = await context.Patients.AsNoTracking().FirstAsync();

                Assert.AreNotEqual(0, patient.Id);
            }
        }

        [DataTestMethod]
        [DataRow(-153)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(1245896)]
        public async Task TestEncryptInt32(int value)
        {
            await this.TestValues<IntEntity>(() => new IntEntity() { Value = value, Value2 = value },
                e =>
                {
                    Assert.AreEqual(default(int), e.Value);
                    Assert.AreEqual(default(int), e.Value2);
                },
                e =>
                {
                    Assert.AreEqual(value, e.Value);
                    Assert.AreEqual(value, e.Value2);
                });
        }

        [DataTestMethod]
        [DataRow(-153)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(1245896)]
        public async Task TestEncryptInt64(long value)
        {
            await this.TestValues<LongEntity>(() => new LongEntity() { Value = value, Value2 = value },
                e =>
                {
                    Assert.AreEqual(default(long), e.Value);
                    Assert.AreEqual(default(long), e.Value2);
                },
                e =>
                {
                    Assert.AreEqual(value, e.Value);
                    Assert.AreEqual(value, e.Value2);
                });
        }

        [DataTestMethod]
        [DataRow(-153.0)]
        [DataRow(0.0)]
        [DataRow(1.0)]
        [DataRow(1245896.4586623)]
        [DataRow(3.145896236)]
        public async Task TestEncryptDouble(double value)
        {
            await this.TestValues<DoubleEntity>(() => new DoubleEntity() { Value = value, Value2 = value },
                e =>
                {
                    Assert.AreEqual(default(double), e.Value);
                    Assert.AreEqual(default(double), e.Value2);
                },
                e =>
                {
                    Assert.AreEqual(value, e.Value);
                    Assert.AreEqual(value, e.Value2);
                });
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(12)]
        [DataRow(34)]
        [DataRow(2481)]
        [DataRow(1245896)]
        public async Task TestEncryptByteArray(int lenght)
        {
            Random rand = new Random(14);
            byte[] constData1 = new byte[lenght];
            byte[] constData2 = new byte[lenght];

            rand.NextBytes(constData1);
            rand.NextBytes(constData2);

            await this.TestValues<ByteArrayEntity>(() => new ByteArrayEntity() { Value = constData1, Value2 = constData2 },
                e =>
                {
                    Assert.AreEqual(null, e.Value);
                    Assert.AreEqual(null, e.Value2);
                },
                e =>
                {
                    CollectionAssert.AreEquivalent(constData1, e.Value);
                    CollectionAssert.AreEquivalent(constData2, e.Value2);
                });
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(12)]
        [DataRow(34)]
        [DataRow(2481)]
        [DataRow(1245896)]
        public async Task TestEncryptByteArrayWithLzw(int lenght)
        {
            Random rand = new Random(14);
            byte[] constData1 = new byte[lenght];
            byte[] constData2 = new byte[lenght];

            rand.NextBytes(constData1);
            rand.NextBytes(constData2);

            await this.TestValues<ByteArrayLzwEntity>(() => new ByteArrayLzwEntity() { Value = constData1, Value2 = constData2 },
                e =>
                {
                    Assert.AreEqual(null, e.Value);
                    Assert.AreEqual(null, e.Value2);
                },
                e =>
                {
                    CollectionAssert.AreEquivalent(constData1, e.Value);
                    CollectionAssert.AreEquivalent(constData2, e.Value2);
                });
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(12)]
        [DataRow(34)]
        [DataRow(2481)]
        [DataRow(1245896)]
        public async Task TestEncryptByteArrayWithGZip(int lenght)
        {
            Random rand = new Random(14);
            byte[] constData1 = new byte[lenght];
            byte[] constData2 = new byte[lenght];

            rand.NextBytes(constData1);
            rand.NextBytes(constData2);

            await this.TestValues<ByteArrayGzipEntity>(() => new ByteArrayGzipEntity() { Value = constData1, Value2 = constData2 },
                e =>
                {
                    Assert.AreEqual(null, e.Value);
                    Assert.AreEqual(null, e.Value2);
                },
                e =>
                {
                    CollectionAssert.AreEquivalent(constData1, e.Value);
                    CollectionAssert.AreEquivalent(constData2, e.Value2);
                });
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("a")]
        [DataRow("š")]
        [DataRow("Sed eleifend lorem neque, sit amet pharetra leo pellentesque cursus. Sed semper varius dui, ac fringilla turpis posuere nec.")]
        [DataRow("Nunc nunc massa, euismod id diam in, iaculis faucibus diam. Etiam id leo sed est fringilla tempor. Aliquam ut consectetur lacus. Sed dictum felis eu erat volutpat gravida. Suspendisse eu vestibulum ligula. Cras accumsan elit nisi, vel pulvinar lorem venenatis ac. In facilisis eget nibh nec iaculis. Nulla condimentum libero sit amet tellus iaculis, eu rhoncus ex dictum. Vivamus id convallis orci, eget porta mauris. Donec tristique dui at massa rhoncus ornare. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Nullam ut eros ut eros fringilla lobortis. ")]
        public async Task TestEncryptStringWithLzw(string value)
        {
            await this.TestValues<StringLzwEntity>(() => new StringLzwEntity() { Value = value, Value2 = value },
                e =>
                {
                    Assert.AreEqual(default(string), e.Value);
                    Assert.AreEqual(default(string), e.Value2);
                },
                e =>
                {
                    Assert.AreEqual(value, e.Value);
                    Assert.AreEqual(value, e.Value2);
                });
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("a")]
        [DataRow("š")]
        [DataRow("Sed eleifend lorem neque, sit amet pharetra leo pellentesque cursus. Sed semper varius dui, ac fringilla turpis posuere nec.")]
        [DataRow("Nunc nunc massa, euismod id diam in, iaculis faucibus diam. Etiam id leo sed est fringilla tempor. Aliquam ut consectetur lacus. Sed dictum felis eu erat volutpat gravida. Suspendisse eu vestibulum ligula. Cras accumsan elit nisi, vel pulvinar lorem venenatis ac. In facilisis eget nibh nec iaculis. Nulla condimentum libero sit amet tellus iaculis, eu rhoncus ex dictum. Vivamus id convallis orci, eget porta mauris. Donec tristique dui at massa rhoncus ornare. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Nullam ut eros ut eros fringilla lobortis. ")]
        public async Task TestEncryptStringWithGzip(string value)
        {
            await this.TestValues<StringLzwEntity>(() => new StringLzwEntity() { Value = value, Value2 = value },
                e =>
                {
                    Assert.AreEqual(default(string), e.Value);
                    Assert.AreEqual(default(string), e.Value2);
                },
                e =>
                {
                    Assert.AreEqual(value, e.Value);
                    Assert.AreEqual(value, e.Value2);
                });
        }

        private async Task TestValues<TEntity>(Func<TEntity> create, Action<TEntity> checkDefault, Action<TEntity> checkDecrypted)
            where TEntity : class
        {
            using TestModelContext context = TestModelContext.Create();
            Services services = new Services(context);

            IEncryptedScopeCreator provider = await services.GetProvider().EnshureEncrypted();
            using (IDisposable scope = provider.IntoScope())
            {

                await context.Set<TEntity>().AddAsync(create());

                await context.SaveChangesAsync();
            }

            using (IDisposable scope = services.GetProvider().EnshureDefaultValues().IntoScope())
            {
                TEntity unencrypted = await context.Set<TEntity>().AsNoTracking().FirstAsync();

                checkDefault(unencrypted);
            }

            IEncryptedScopeCreator provider2 = await services.GetProvider().EnshureEncrypted();
            using (IDisposable scope = provider2.IntoScope())
            {

                TEntity decrypted = await context.Set<TEntity>().AsNoTracking().FirstAsync();
                checkDecrypted(decrypted);
            }
        }
    }
}
