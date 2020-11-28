using Harrison314.EntityFrameworkCore.Encryption;
using Harrison314.EntityFrameworkCore.Encryption.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static EncryptedContextBuilder AddEncryptedContext<TDbContext>(this IServiceCollection serviceCollection, Action<DbContextEncryptedProviderOptions<TDbContext>> setup = null)
            where TDbContext : DbContext
        {
            serviceCollection.Configure<DbContextEncryptedProviderOptions<TDbContext>>(setup ?? (_ => { }));
            serviceCollection.AddSingleton<IDbContextEncryptedProvider<TDbContext>, DbContextEncryptedProvider<TDbContext>>();

            return new EncryptedContextBuilder(serviceCollection);
        }
    }
}
