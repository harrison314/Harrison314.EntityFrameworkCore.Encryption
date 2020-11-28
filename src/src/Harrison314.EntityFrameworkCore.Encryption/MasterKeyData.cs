using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption
{
    public class MasterKeyData
    {
        public byte[] Data
        {
            get;
            set;
        }

        public string KeyId
        {
            get;
            set;
        }

        public string Parameters
        {
            get;
            set;
        }

        public MasterKeyData()
        {

        }
    }
}
