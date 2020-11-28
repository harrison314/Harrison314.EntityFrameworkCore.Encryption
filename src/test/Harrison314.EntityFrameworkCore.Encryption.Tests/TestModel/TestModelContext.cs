using Harrison314.EntityFrameworkCore.Encryption.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Tests.TestModel
{
    public class TestModelContext : DbContext
    {
        private readonly IDisposable disposable;

        public static TestModelContext Create(string connectionString = "Filename=:memory:")
        {
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            DbContextOptionsBuilder<TestModelContext> options = new DbContextOptionsBuilder<TestModelContext>();
            options.UseSqlite(connection);

            TestModelContext context = new TestModelContext(options.Options, connection);
            context.Database.EnsureCreated();
            context.Database.Migrate();

            return context;
        }

        public DbSet<CityTest> Cities
        {
            get;
            set;
        }

        public DbSet<Patient> Patients
        {
            get;
            set;
        }

        public DbSet<IntEntity> IntEntitys
        {
            get;
            set;
        }

        public DbSet<LongEntity> LongEntitys
        {
            get;
            set;
        }

        public DbSet<DoubleEntity> DoubleEntitys
        {
            get;
            set;
        }

        public DbSet<ByteArrayEntity> ByteArrayEntitys
        {
            get;
            set;
        }

        public TestModelContext([NotNull] DbContextOptions options, IDisposable disposable)
            : base(options)
        {
            this.disposable = disposable;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddEncryptionContext();

            modelBuilder.Entity<CityTest>(p =>
            {
                p.HasKey(t => t.Id);
            });

            modelBuilder.Entity<Patient>(p =>
            {
                p.HasKey(t => t.Id);
                p.Property(t => t.Name)
                  .HasEncrypted("Ashka458_asajcsh-47mK", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Randomized);
                p.Property(t => t.SSI)
                  .HasEncrypted("kNdDd2_45856320", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Deterministic);
            });

            modelBuilder.Entity<IntEntity>(p =>
            {
                p.HasKey(t => t.Id);
                p.Property(t => t.Value)
                .HasEncrypted("IntEntity.Value.2020", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Deterministic);
                p.Property(t => t.Value2)
               .HasEncrypted("IntEntity.Value2.2020", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Randomized);
            });

            modelBuilder.Entity<LongEntity>(p =>
            {
                p.HasKey(t => t.Id);
                p.Property(t => t.Value)
                .HasEncrypted("LongEntity.Value.2020", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Deterministic);
                p.Property(t => t.Value2)
               .HasEncrypted("LongEntity.Value2.2020", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Randomized);
            });

            modelBuilder.Entity<DoubleEntity>(p =>
            {
                p.HasKey(t => t.Id);
                p.Property(t => t.Value)
                .HasEncrypted("DoubleEntity.Value.2020", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Deterministic);
                p.Property(t => t.Value2)
               .HasEncrypted("DoubleEntity.Value2.2020", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Randomized);
            });

            modelBuilder.Entity<ByteArrayEntity>(p =>
            {
                p.HasKey(t => t.Id);
                p.Property(t => t.Value)
                .HasEncrypted("ByteArrayEntity.Value.2020", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Deterministic);
                p.Property(t => t.Value2)
               .HasEncrypted("ByteArrayEntity.Value2.2020", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Randomized);
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            this.disposable?.Dispose();
        }
    }

    public class CityTest
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string ShortName
        {
            get;
            set;
        }

        public CityTest()
        {

        }
    }

    public class Patient
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string SSI
        {
            get;
            set;
        }

        public Patient()
        {

        }
    }

    public class IntEntity
    {
        public int Id
        {
            get;
            set;
        }

        public int Value
        {
            get;
            set;
        }

        public int Value2
        {
            get;
            set;
        }

        public IntEntity()
        {

        }
    }

    public class LongEntity
    {
        public int Id
        {
            get;
            set;
        }

        public long Value
        {
            get;
            set;
        }

        public long Value2
        {
            get;
            set;
        }

        public LongEntity()
        {

        }
    }

    public class DoubleEntity
    {
        public int Id
        {
            get;
            set;
        }

        public double Value
        {
            get;
            set;
        }

        public double Value2
        {
            get;
            set;
        }

        public DoubleEntity()
        {

        }
    }

    public class ByteArrayEntity
    {
        public int Id
        {
            get;
            set;
        }

        public byte[] Value
        {
            get;
            set;
        }

        public byte[] Value2
        {
            get;
            set;
        }

        public ByteArrayEntity()
        {

        }
    }
}
