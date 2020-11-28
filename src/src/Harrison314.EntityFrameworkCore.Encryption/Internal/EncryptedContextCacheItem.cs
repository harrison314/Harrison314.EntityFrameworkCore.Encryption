using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    internal sealed class EncryptedContextCacheItem : IDisposable
    {
        public IEncryptionContext Context
        {
            get;
            private set;
        }

        public DateTime? Expiration
        {
            get;
            private set;
        }

        public SemaphoreSlim Semaphore
        {
            get;
        }

        public EncryptedContextCacheItem()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.Context = null;
            this.Expiration = null;
        }

        public void Set(IEncryptionContext context, TimeSpan? slidingExpiration)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!object.ReferenceEquals(context, this.Context) && this.Context is IDisposable disposableContext)
            {
                disposableContext.Dispose();
            }

            this.Context = context;
            if (slidingExpiration.HasValue)
            {
                this.Expiration = DateTime.UtcNow + slidingExpiration.Value;
            }
            else
            {
                this.Expiration = null;
            }
        }

        public void RemoveContext()
        {
            if (this.Context is IDisposable disposableContext)
            {
                disposableContext.Dispose();
            }

            this.Context = null;
        }

        public bool RequireNewContext(DateTime utcNow)
        {
            if (this.Context == null)
            {
                return true;
            }

            if (this.Expiration.HasValue && this.Expiration.Value <= utcNow)
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            this.RemoveContext();
            this.Semaphore.Dispose();
        }
    }
}
