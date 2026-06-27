using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Harrison314.EntityFrameworkCore.Encryption.Extensions
{
    internal static class X509Certificate2Extensions
    {
        public static bool IsForUsage(this X509Certificate2 certificate, X509KeyUsageFlags usageFlag)
        {
            foreach (X509Extension certificateExtension in certificate.Extensions)
            {
                if (certificateExtension is X509KeyUsageExtension usage)
                {
                    if (usage.KeyUsages.HasFlag(usageFlag))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsForEncryption(this X509Certificate2 certificate)
        {
            return IsForUsage(certificate, X509KeyUsageFlags.KeyEncipherment) || IsForUsage(certificate, X509KeyUsageFlags.DataEncipherment);
        }
    }
}
