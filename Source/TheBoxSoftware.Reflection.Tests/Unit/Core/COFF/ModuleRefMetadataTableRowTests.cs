﻿
namespace TheBoxSoftware.Reflection.Tests.Unit.Core.COFF
{
    using Helpers;
    using NUnit.Framework;
    using Reflection.Core;
    using Reflection.Core.COFF;

    [TestFixture]
    public class ModuleRefMetadataTableRowTests
    {
        [Test]
        public void ModuleRef_WhenCreated_FieldsAreReadCorrectly()
        {
            byte[] contents = new byte[] {
                0x01, 0x00
            };
            Offset offset = 0;
            IIndexDetails indexDetails = IndexHelper.CreateIndexDetails(2);

            ModuleRefMetadataTableRow row = new ModuleRefMetadataTableRow(contents, offset, indexDetails);

            Assert.AreEqual(1, row.Name.Value);
        }

        [Test]
        public void ModuleRef_WhenCreated_OffsetIsMovedOn()
        {
            byte[] contents = new byte[2];
            Offset offset = 0;
            IIndexDetails indexDetails = IndexHelper.CreateIndexDetails(2);

            ModuleRefMetadataTableRow row = new ModuleRefMetadataTableRow(contents, offset, indexDetails);

            Assert.AreEqual(2, offset.Current);
        }
    }
}
