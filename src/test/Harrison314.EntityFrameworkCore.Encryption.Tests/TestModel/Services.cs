using Harrison314.EntityFrameworkCore.Encryption.CryptoProviders;
using Harrison314.EntityFrameworkCore.Encryption.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Tests.TestModel
{
    public class Services
    {
        public IServiceProvider ServiceProvider
        {
            get;
            set;
        }

        public Services(TestModelContext context, Func<IDbContextEncryptedCryptoProvider> providerFactory = null)
        {
            IServiceCollection sc = new ServiceCollection()
                .AddSingleton(context)
                .Configure<DbContextEncryptedProviderOptions<TestModelContext>>(o => {

                });

            EncryptedContextBuilder builder = sc.AddEncryptedContext<TestModelContext>();

            if (providerFactory != null)
            {
                sc.AddTransient<IDbContextEncryptedCryptoProvider>(_ => providerFactory());
            }
            else
            {
                builder.WithPasswordEncryptionProvider("Password");
            }

            this.ServiceProvider = sc.BuildServiceProvider();
        }

        public TestModelContext GetDontext()
        {
            return this.ServiceProvider.GetRequiredService<TestModelContext>();
        }

        public IDbContextEncryptedProvider<TestModelContext> GetProvider()
        {
            return this.ServiceProvider.GetRequiredService<IDbContextEncryptedProvider<TestModelContext>>();
        }
    }
}
