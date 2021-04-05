using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    internal class EncryptedContextLifetime : IEncryptedContextLifetime, IDisposable
    {
        private readonly CancellationTokenSource tokenSource;
        private bool disposedValue;

        public CancellationToken EmergencyKilling
        {
            get => this.tokenSource.Token;
        }

        public EncryptedContextLifetime()
        {
            this.tokenSource = new CancellationTokenSource();
        }

        public void EmergencyKill()
        {
            this.tokenSource.Cancel(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.tokenSource.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}
