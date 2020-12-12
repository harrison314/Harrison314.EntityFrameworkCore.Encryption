using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Pkcs11Data
{
    public struct DataInfo
    {
        public string Id
        {
            get;
            private set;
        }

        public string Label
        {
            get;
            private set;
        }

        public DataInfo(string id, string label)
        {
            this.Id = id;
            this.Label = label;
        }
    }
}
