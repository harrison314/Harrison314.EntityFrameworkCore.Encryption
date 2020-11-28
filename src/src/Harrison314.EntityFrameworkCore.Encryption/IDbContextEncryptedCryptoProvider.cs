using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption
{
    public interface IDbContextEncryptedCryptoProvider
    {
        public string ProviderName
        {
            get;
        }

        ValueTask<MasterKeyData> EncryptMasterKey(byte[] masterKey, CancellationToken cancellationToken);

        ValueTask<string> FilterAcceptKeyIds(List<string> keyIds, CancellationToken cancellationToken);

        ValueTask<byte[]> DecryptMasterKey(MasterKeyData masterKeyData, CancellationToken cancellationToken);
    }
}
