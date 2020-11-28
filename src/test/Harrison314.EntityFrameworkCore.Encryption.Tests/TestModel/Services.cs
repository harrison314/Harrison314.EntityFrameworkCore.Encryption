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

        public Services(TestModelContext context)
        {
            this.ServiceProvider = new ServiceCollection()
                .AddSingleton(context)
                .Configure<DbContextEncryptedProviderOptions<TestModelContext>>(o=>{
                    
            })
                .AddTransient<IDbContextEncryptedProvider<TestModelContext>, DbContextEncryptedProvider<TestModelContext>>()
                .AddTransient<IDbContextEncryptedCryptoProvider>(_ => new PasswordDbContextEncryptedCryptoProvider("Password"))
            .BuildServiceProvider();
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
