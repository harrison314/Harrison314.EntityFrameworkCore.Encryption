using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption
{
    public class DbContextEncryptedProviderOptions<TDbContext>
        where TDbContext : DbContext
    {
        public Func<IServiceProvider, TDbContext> DbContextFactory
        {
            get;
            set;
        }

        public Action<TDbContext> DbContextCreanup
        {
            get;
            set;
        }

        public TimeSpan? EncryptionContextExpirtaion
        {
            get;
            set;
        }

        public DbContextEncryptedProviderOptions()
        {

        }
    }
}
