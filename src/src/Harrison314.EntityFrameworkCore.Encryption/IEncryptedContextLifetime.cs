using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption
{
    public interface IEncryptedContextLifetime
    {
        CancellationToken EmergencyKilling
        { 
            get; 
        }

        void EmergencyKill();
    }
}
