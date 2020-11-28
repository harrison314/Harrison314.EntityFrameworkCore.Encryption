using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption
{
    public interface IDbContextEncryptedProvider<TDbContext>
        where TDbContext : DbContext
    {
        Task<IEncryptedScopeCreator> EnshureEncrypted(CancellationToken cancellationToken = default);

        Task ReEncrypted(IDbContextEncryptedCryptoProvider fromProvider, IDbContextEncryptedCryptoProvider toProvider, CancellationToken cancellationToken = default);

        IEncryptedScopeCreator EnshureDefaultValues();
    }
}
