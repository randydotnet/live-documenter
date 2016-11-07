﻿
namespace TheBoxSoftware.Reflection.Tests.Unit.Core.COFF
{
    using NUnit.Framework;
    using Reflection.Core;
    using Reflection.Core.COFF;
    using Helpers;

    [TestFixture]
    public class ImplMapMetadataTableRowTests
    {
        [Test]
        public void ImplMap_WhenCreated_FieldsAreReadCorrectly()
        {
            ICodedIndexResolver resolver = CodedIndexHelper.CreateCodedIndexResolver(2);
            byte[] contents = new byte[] {
                0x01, 0x00,
                0x01, 0x00,
                0x01, 0x00,
                0x01, 0x00
            };

            ImplMapMetadataTableRow row = new ImplMapMetadataTableRow(contents, 0, resolver, 2, 2);

            Assert.AreEqual(PInvokeAttributes.NoMangle, row.MappingFlags);
            Assert.IsNotNull(row.MemberForward);
            Assert.AreEqual(1, row.ImportName.Value);
            Assert.AreEqual(1, row.ImportScope.Value);
        }

        [Test]
        public void ImplMap_WhenCreated_OffsetIsMovedOn()
        {
            ICodedIndexResolver resolver = CodedIndexHelper.CreateCodedIndexResolver(2);
            Offset offset = 0;
            byte[] contents = new byte[10];

            ImplMapMetadataTableRow row = new ImplMapMetadataTableRow(contents, offset, resolver, 2, 2);

            Assert.AreEqual(8, offset.Current);
        }
    }
}
