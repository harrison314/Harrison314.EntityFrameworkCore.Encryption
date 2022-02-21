using Harrison314.EntityFrameworkCore.Encryption.Internal.PropertyEncryptors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Tests.Internal.PropertyEncryptors
{
    [TestClass]
    public class PropertyEncryptorsConstantsTests
    {
        [TestMethod]
        public void DeterministicCounter_Constant()
        {
            Assert.AreEqual<uint>(4512141, PropertyEncryptorsConstants.DeterministicCounter);
        }

        [TestMethod]
        public void AesGcmDeterministicCounter_Constant()
        {
            Assert.AreEqual<uint>(4535181, PropertyEncryptorsConstants.AesGcmDeterministicCounter);
        }
    }
}
