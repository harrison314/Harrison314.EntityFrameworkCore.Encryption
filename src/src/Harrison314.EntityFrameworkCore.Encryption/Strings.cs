using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption
{
    internal static class Strings
    {
        public const string UnprotectEncryptedException = "Unprotect encrypted data failed. Data is broken - not match aead signature.";

        public const string EncryptionScopeNotFound = "Operation with encrypted property is not in encryption scope.";

        public const string MultipleUsageIntoScope = "Multiple usage of IntoScope.";
    }
}
