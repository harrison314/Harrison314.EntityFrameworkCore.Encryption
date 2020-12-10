using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Pkcs11Data
{
    public class Pkcs11SecritData
    {
        public byte[] PasswordSalt
        {
            get;
            set;
        }

        public int Iterations
        {
            get;
            set;
        }

        public byte[] AesGcmNonce
        {
            get;
            set;
        }

        public byte[] AesGcmTag
        {
            get;
            set;
        }

        public Pkcs11SecritData()
        {

        }

        internal void Validate()
        {
            //TODO: validate
        }
    }
}
