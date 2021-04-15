using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Contrib.CryptoProviders.Dpapi
{
    public class DpapiData
    {
        public byte[] AdditionalEntrophy
        {
            get;
            set;
        }

        public DpapiData()
        {

        }
    }
}
