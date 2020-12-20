using Harrison314.EntityFrameworkCore.Encryption.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    public class DbContextEncryptedProvider<TDbContext> : IDbContextEncryptedProvider<TDbContext>, IDisposable
        where TDbContext : DbContext
    {
        private readonly IDbContextEncryptedCryptoProvider crypetoProvider;
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<DbContextEncryptedProviderOptions<TDbContext>> providerOptions;
        private readonly EncryptedContextCacheItem cacheItem;


        public DbContextEncryptedProvider(IDbContextEncryptedCryptoProvider crypetoProvider,
            IServiceProvider serviceProvider,
            IOptions<DbContextEncryptedProviderOptions<TDbContext>> providerOptions)
        {
            this.crypetoProvider = crypetoProvider ?? throw new ArgumentNullException(nameof(crypetoProvider));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.providerOptions = providerOptions ?? throw new ArgumentNullException(nameof(providerOptions));

            this.cacheItem = new EncryptedContextCacheItem();
        }

        public async Task<IEncryptedScopeCreator> EnshureEncrypted(CancellationToken cancellationToken = default)
        {
            DateTime utcNow = DateTime.UtcNow;
            if (this.cacheItem.RequireNewContext(utcNow))
            {
                await this.cacheItem.Semaphore.WaitAsync();
                try
                {
                    if (this.cacheItem.RequireNewContext(utcNow))
                    {
                        this.cacheItem.RemoveContext();

                        await this.EnshureEncryptionKeyInDb(cancellationToken);
                        MasterKeyData masterKeyData = await this.GetMasterKeyData(cancellationToken);

                        byte[] decryptedMasterKey = await this.crypetoProvider.DecryptMasterKey(masterKeyData, cancellationToken);
                        IEncryptionContext context = new EncryptionContext(decryptedMasterKey);
                        this.cacheItem.Set(context, this.providerOptions.Value.EncryptionContextExpirtaion);
                    }
                }
                finally
                {
                    this.cacheItem.Semaphore.Release();
                }
            }

            return new EncryptedScopeCreator(this.cacheItem.Context!);
        }

        public async Task ReEncrypted(IDbContextEncryptedCryptoProvider fromProvider, IDbContextEncryptedCryptoProvider toProvider, CancellationToken cancellationToken = default)
        {
            if (fromProvider == null) throw new ArgumentNullException(nameof(fromProvider));
            if (toProvider == null) throw new ArgumentNullException(nameof(toProvider));

            MasterKeyData masterKeyData = await this.GetMasterKeyData(cancellationToken);
            byte[] masterKey = await fromProvider.DecryptMasterKey(masterKeyData, cancellationToken);
            try
            {
                MasterKeyData newMasterKeyData = await toProvider.EncryptMasterKey(masterKey, cancellationToken);

                (TDbContext context, IDisposable? scope) = this.CreateContext();
                try
                {
                    DbSet<EcCdeMasterKey> keySet = context.Set<EcCdeMasterKey>();

                    EcCdeMasterKey keyEntity = new EcCdeMasterKey()
                    {
                        Data = newMasterKeyData.Data,
                        KeyId = newMasterKeyData.KeyId,
                        Paramaters = newMasterKeyData.Parameters,
                        ProviderName = toProvider.ProviderName
                    };

                    await keySet.AddAsync(keyEntity, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
                finally
                {
                    this.CleanContextSafe(context, scope);
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(masterKey);
            }
        }

        public IEncryptedScopeCreator EnshureDefaultValues()
        {
            return new EncryptedScopeCreator(new DefaultValueEncryptionContext());
        }

        public void Dispose()
        {
            this.cacheItem.Dispose();
            if (this.serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private async Task EnshureEncryptionKeyInDb(CancellationToken cancellationToken)
        {
            (TDbContext context, IDisposable? scope) = this.CreateContext();
            try
            {
                DbSet<EcCdeMasterKey> keySet = context.Set<EcCdeMasterKey>();
                if (!await keySet.AnyAsync(cancellationToken))
                {
                    byte[] masterKeyBytes = new byte[32];
                    try
                    {
                        RandomNumberGenerator.Fill(masterKeyBytes);

                        MasterKeyData mk = await this.crypetoProvider.EncryptMasterKey(masterKeyBytes, cancellationToken);
                        EcCdeMasterKey masterKey = new EcCdeMasterKey()
                        {
                            Data = mk.Data,
                            KeyId = mk.KeyId,
                            Paramaters = mk.Parameters,
                            ProviderName = this.crypetoProvider.ProviderName
                        };

                        await keySet.AddAsync(masterKey, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                    finally
                    {
                        CryptographicOperations.ZeroMemory(masterKeyBytes);
                    }
                }
            }
            finally
            {
                this.CleanContextSafe(context, scope);
            }
        }

        private async Task<MasterKeyData> GetMasterKeyData(CancellationToken cancellationToken)
        {
            (TDbContext context, IDisposable? scope) = this.CreateContext();
            try
            {
                DbSet<EcCdeMasterKey> keySet = context.Set<EcCdeMasterKey>();
                List<string> keyIds = await keySet.Where(t => t.ProviderName == this.crypetoProvider.ProviderName).Select(t => t.KeyId).ToListAsync(cancellationToken);
                if (keyIds.Count == 0)
                {
                    throw new InvalidOperationException("Database master key not found in database.");
                }

                string acceptKeyId = await this.crypetoProvider.FilterAcceptKeyIds(keyIds, cancellationToken);
                EcCdeMasterKey localMasterKeyData = await keySet.SingleAsync(t => t.ProviderName == this.crypetoProvider.ProviderName && t.KeyId == acceptKeyId, cancellationToken);
                MasterKeyData masterKeyData = new MasterKeyData()
                {
                    Data = localMasterKeyData.Data,
                    KeyId = localMasterKeyData.KeyId,
                    Parameters = localMasterKeyData.Paramaters
                };

                return masterKeyData;
            }
            finally
            {
                this.CleanContextSafe(context, scope);
            }
        }

        private (TDbContext context, IDisposable? scope) CreateContext()
        {
            DbContextEncryptedProviderOptions<TDbContext> configuration = this.providerOptions.Value;
            if (configuration.DbContextFactory != null)
            {
                return (configuration.DbContextFactory.Invoke(this.serviceProvider), null);
            }
            else
            {
                IServiceScopeFactory serviceScopeFactory = this.serviceProvider.GetRequiredService<IServiceScopeFactory>();
                IServiceScope scope = serviceScopeFactory.CreateScope();
                TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

                return (context, scope);
            }
        }

        private void CleanContextSafe(TDbContext context, IDisposable? scope)
        {
            DbContextEncryptedProviderOptions<TDbContext> configuration = this.providerOptions.Value;

            if (context != null && configuration.DbContextCreanup != null)
            {
                configuration.DbContextCreanup.Invoke(context);
            }

            scope?.Dispose();
        }
    }
}
