using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption
{
    public class EfEncryptionException : ApplicationException
    {
        public EfEncryptionException()
        {
        }

        public EfEncryptionException(string message) 
            : base(message)
        {
        }

        public EfEncryptionException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected EfEncryptionException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
