using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder AddEncryptionContext(this ModelBuilder modelBuilder, Action<EntityTypeBuilder<EcCdeMasterKey>>? buildAction = null)
        {
            modelBuilder.Entity<EcCdeMasterKey>(p =>
            {
                p.Property(t => t.KeyId).HasMaxLength(256).IsRequired();
                p.Property(t => t.ProviderName).HasMaxLength(256).IsRequired();

                buildAction?.Invoke(p);
            });

            return modelBuilder;
        }
    }
}
