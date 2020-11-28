using Harrison314.EntityFrameworkCore.Encryption;
using Harrison314.EntityFrameworkCore.Encryption.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWebApiProject.Dal
{
    public class SampleDbContext : DbContext
    {
        public DbSet<Patient> Patients
        {
            get;
            set;
        }

        public DbSet<Visist> Visits
        {
            get;
            set;
        }

        public SampleDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddEncryptionContext();

            modelBuilder.Entity<Patient>(p =>
            {
                p.HasKey(t => t.Id);
                p.Property(t => t.FirstName).HasEncrypted("Patient.FirstName", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Randomized).IsRequired().HasMaxLength(150);
                p.Property(t => t.LastName).HasEncrypted("Patient.LastName", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Randomized).IsRequired().HasMaxLength(150);
                p.Property(t => t.SocialSecurityNumber).HasEncrypted("Patient.LastName", EncrypetionType.AEAD_AES_256_CBC_HMAC_SHA_256, EncryptionMode.Deterministic).IsRequired().HasMaxLength(150);
                p.Property(t => t.Notes).HasDefaultValue(string.Empty);
                p.HasMany(t => t.Visists).WithOne(t => t.Patient).HasForeignKey(t => t.PatientId);
            });

            modelBuilder.Entity<Visist>(p =>
            {
                p.HasKey(t => t.Id);
            });
        }
    }
}
