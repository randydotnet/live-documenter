﻿
namespace TheBoxSoftware.Reflection.Core.COFF
{
    /// <summary>
    /// Used to store compile time, constant values for fields, parameters and properties
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that constant information does not directly influence runtime behaviour, although
    /// it is visible via reflection. Compilers inspect this information, at compile time, when
    /// importing metadata, but the value of the constant itself, if used, becomes embedded in
    /// into the CIL stream the compiler emits. There are no CIL instructions to access constant
    /// table at runtime.
    /// </para>
    /// </remarks>
    public class ConstantMetadataTableRow : MetadataRow
    {
        private BlobIndex _valueIndex;
        private CodedIndex _parentIndex;
        private Signatures.ElementTypes _type;

        /// <summary>
        /// Initialises a new instance of the ConstantMetadataTableRow
        /// </summary>
        /// <param name="contents">The contents fo the file</param>
        /// <param name="offset">The offset for the current row</param>
        public ConstantMetadataTableRow(byte[] contents, Offset offset, ICodedIndexResolver resolver, IIndexDetails indexDetails)
        {
            this.FileOffset = offset;

            int hasConstantIndexSize = resolver.GetSizeOfIndex(CodedIndexes.HasConstant);
            byte sizeOfBlobIndex = indexDetails.GetSizeOfBlobIndex();

            _type = (Signatures.ElementTypes)contents[offset.Shift(1)];
            offset.Shift(1);
            _parentIndex = resolver.Resolve(
                CodedIndexes.HasConstant, 
                FieldReader.ToUInt32(contents, offset.Shift(hasConstantIndexSize), hasConstantIndexSize)
                );
            _valueIndex = new BlobIndex(sizeOfBlobIndex, contents, Signatures.Signatures.MethodDef, offset);
        }

        /// <summary>
        /// The type of field that represents the constant.
        /// </summary>
        /// <remarks>
        /// For a <b>nullref</b> value for <i>FieldInit</i> in <i>ilasm</i> is <c>ELEMENT_TYPE_CLASS</c>
        /// with a 4-byte zero. Unlike uses of <c>ELEMENT_TYPE_CLASS</c> in Signatures, this one is
        /// <i>not</i> followed by a type token.
        /// </remarks>
        public Signatures.ElementTypes Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// An index in to the Param, Field, or Property table. More precisely
        /// a HasConstant coded index
        /// </summary>
        public CodedIndex Parent
        {
            get { return _parentIndex; }
            set { _parentIndex = value; }
        }

        /// <summary>
        /// An index in to the Blob heap
        /// </summary>
        public BlobIndex Value
        {
            get { return _valueIndex; }
            set { _valueIndex = value; }
        }
    }
}