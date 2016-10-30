﻿
namespace TheBoxSoftware.Reflection
{
    using System;
    using Reflection.Core.COFF;
    using Reflection.Signitures;

    /// <summary>
    /// Details the specification of a type. This is generally used by the metadata
    /// to allow for types to derive from and implement interfaces and classes that
    /// are generic.
    /// </summary>
    internal sealed class TypeSpec : TypeRef
    {
        private TypeDetails _details = null;
        private BlobIndex _signitureIndexInBlob;

        /// <summary>
        /// Creates a new instance of the TypeSpec class, using the provided information.
        /// </summary>
        /// <param name="assembly">The assembly this type specification is defined.</param>
        /// <param name="metatadata">The metadata directory containing the information.</param>
        /// <param name="row">The metadata row containing the actual details.</param>
        /// <returns>The instantiated type specification.</returns>
        public static TypeSpec CreateFromMetadata(AssemblyDef assembly, MetadataDirectory metatadata, TypeSpecMetadataTableRow row)
        {
            TypeSpec spec = new TypeSpec();
            spec.UniqueId = assembly.CreateUniqueId();
            spec.Assembly = assembly;
            spec._signitureIndexInBlob = row.Signiture;
            return spec;
        }

        /// <summary>
        /// Loads the details of the underlying type and specification.
        /// </summary>
        private void LoadDetails()
        {
            TypeSpecificationSigniture signiture = this.Signiture;
            ElementTypes elementType = signiture.TypeToken.ElementType.ElementType;

            _details = signiture.GetTypeDetails(this);
        }

        /// <summary>
        /// Gets the name of the underlying type in this specification.
        /// </summary>
        public override string Name
        {
            get
            {
                if(_details == null)
                {
                    LoadDetails();
                }
                return _details.Type.Name;
            }
            set { base.Name = value; }
        }

        /// <summary>
        /// Gets the namespace of the underlying type in this specification.
        /// </summary>
        public override string Namespace
        {
            get
            {
                if(_details == null)
                {
                    LoadDetails();
                }
                return _details.Type.Namespace;
            }
            set { base.Namespace = value; }
        }

        /// <summary>
        /// Indicates if this type is a generic type.
        /// </summary>
        public override bool IsGeneric
        {
            get
            {
                if(_details == null)
                {
                    LoadDetails();
                }
                return _details.IsGenericInstance;
            }
            set { base.IsGeneric = value; }
        }

        /// <summary>
        /// The signiture defined for this member.
        /// </summary>
        internal TypeSpecificationSigniture Signiture
        {
            get
            {
                if(!this.Assembly.File.IsMetadataLoaded)
                {
                    throw new InvalidOperationException(Resources.ExceptionMessages.Ex_SignatureParsing_NoMetadata);
                }

                BlobStream stream = (BlobStream)((CLRDirectory)this.Assembly.File.Directories[
                    Core.PE.DataDirectories.CommonLanguageRuntimeHeader]).Metadata.Streams[Streams.BlobStream];
                return stream.GetSigniture(
                    _signitureIndexInBlob.Value, _signitureIndexInBlob.SignitureType
                    ) as TypeSpecificationSigniture;
            }
        }

        /// <summary>
        /// Gets a reference to the details of the underlying type.
        /// </summary>
        public TypeDetails TypeDetails
        {
            get
            {
                if(_details == null)
                {
                    LoadDetails();
                }
                return _details;
            }
        }

        /// <summary>
        /// Gets or sets the type which is implementing this base type or interface.
        /// </summary>
        public TypeDef ImplementingType { get; set; }
    }
}