using Harrison314.EntityFrameworkCore.Encryption.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption
{
    internal sealed class EncryptedScopeCreator : IEncryptedScopeCreator
    {
        private IEncryptionContext context;

        public EncryptedScopeCreator(IEncryptionContext context)
        {
            this.context = context;
        }
        public IDisposable IntoScope()
        {
            IEncryptionContext ctx = this.context;
            this.context = null;
            if (ctx == null)
            {
                throw new InvalidOperationException(Strings.MultipleUsageIntoScope);
            }

            return EncryptionScopeContext.Push(ctx);
        }
    }
}
