using Harrison314.EntityFrameworkCore.Encryption.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Tests.Internal
{
    [TestClass]
    public class EncryptionScopeContextTests
    {
        [TestMethod]
        public async Task CurrentIsNotNull()
        {
            await Task.Delay(0);
            Assert.IsNotNull(EncryptionScopeContext.Current);
        }

        [TestMethod]
        public async Task IsInScope_False()
        {
            await Task.Delay(0);
            Assert.IsFalse(EncryptionScopeContext.IsInScope);
        }

        [TestMethod]
        public async Task Current()
        {
            Mock<IEncryptionContext> context = new Mock<IEncryptionContext>(MockBehavior.Strict);

            await Task.Delay(0);
            Assert.IsNotNull(EncryptionScopeContext.Current);
            using (EncryptionScopeContext.Push(context.Object))
            {
                await Task.Delay(0);
                Assert.IsNotNull(EncryptionScopeContext.Current);

                await Task.Delay(0);
                Assert.AreEqual(context.Object, EncryptionScopeContext.Current);
            }

            await Task.Delay(0);
            Assert.IsNotNull(EncryptionScopeContext.Current);
            Assert.AreNotEqual(context.Object, EncryptionScopeContext.Current);
        }

        [TestMethod]
        public async Task IsInScope()
        {
            Mock<IEncryptionContext> context = new Mock<IEncryptionContext>(MockBehavior.Strict);

            await Task.Delay(0);
            Assert.IsFalse(EncryptionScopeContext.IsInScope);
            using (EncryptionScopeContext.Push(context.Object))
            {
                await Task.Delay(0);
                Assert.IsTrue(EncryptionScopeContext.IsInScope);
            }

            await Task.Delay(0);
            Assert.IsFalse(EncryptionScopeContext.IsInScope);
        }

        [TestMethod]
        public async Task Suspend()
        {
            Mock<IEncryptionContext> context = new Mock<IEncryptionContext>(MockBehavior.Strict);

            using (EncryptionScopeContext.Push(context.Object))
            {
                await Task.Delay(0);
                using (EncryptionScopeContext.Suspend())
                {
                    Assert.IsFalse(EncryptionScopeContext.IsInScope);
                }
            }
        }
    }
}
