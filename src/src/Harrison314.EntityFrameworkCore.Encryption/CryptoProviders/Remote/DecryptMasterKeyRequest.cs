﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.CryptoProviders.Remote
{
    internal class DecryptMasterKeyRequest
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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DecryptMasterKeyRequest()
        {

        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    }
}
