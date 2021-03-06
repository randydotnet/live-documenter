﻿
namespace TheBoxSoftware.Reflection.Tests.Unit.Signatures
{
    using NUnit.Framework;
    using Reflection.Signatures;

    [TestFixture]
    public class CustomAttributeSignitureTests
    {
        // this is not a full implemenation so we only need to check for a prolog value at the
        // moment.

        [Test]
        public void CustomAttributeToken_Create()
        {
            byte[] content = new byte[] { 0x01, 0x00 };

            CustomAttributeSignature token = new CustomAttributeSignature(content);

            Assert.AreEqual(1, token.Tokens.Count);
        }
    }
}
