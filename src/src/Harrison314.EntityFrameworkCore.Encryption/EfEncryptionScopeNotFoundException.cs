using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption
{
    public class EfEncryptionScopeNotFoundException : EfEncryptionException
    {
        public EfEncryptionScopeNotFoundException()
        {
        }

        public EfEncryptionScopeNotFoundException(string message) : base(message)
        {
        }

        public EfEncryptionScopeNotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
