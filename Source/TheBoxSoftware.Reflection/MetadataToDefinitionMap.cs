﻿
namespace TheBoxSoftware.Reflection
{
    using System.Collections.Generic;

    /// <summary>
    /// A mapper class that allows the quick retrieval of metadata or
    /// definition information, from either direction.
    /// </summary>
    internal sealed class MetadataToDefinitionMap
    {
        private long _uniqueCounter = 0;
        private AssemblyDef _assembly;

        // This is currently used to map a table and index to an instantiated definition
        // of the metadata (e.g. for TypeRef, TypeDef, TypeSpec, MemberRef, MethodDef, 
        // MethodSpec, and field.
        //
        // We however also need to obtain a definition for an unknown table and index, where
        // wo only know the name of that metadata.
        private Dictionary<Core.COFF.MetadataTables, Dictionary<int, ReflectedMember>> internalMap = new Dictionary<Core.COFF.MetadataTables, Dictionary<int, ReflectedMember>>();
        
        /// <summary>
        /// Adds a new entry in to the definition map.
        /// </summary>
        /// <param name="table">The table this row and definition are from</param>
        /// <param name="metadataRow">The row the definition is associated with</param>
        /// <param name="definition">The definition the row is associated with</param>
        /// <remarks>
        /// If a table is created and no unique key is provided for it, you will not be able to use
        /// a unique key for that table at any point.
        /// </remarks>
        public void Add(Core.COFF.MetadataTables table, Core.COFF.MetadataRow metadataRow, ReflectedMember definition)
        {
            // Make sure the table store is initialised
            if(!internalMap.ContainsKey(table))
            {
                internalMap[table] = new Dictionary<int, ReflectedMember>();
            }
            internalMap[table].Add(metadataRow.FileOffset, definition);
            _uniqueCounter++;
        }

        /// <summary>
        /// Obatains the definition for the specified metadataRow and table.
        /// </summary>
        /// <param name="table">The table the row and definition are from</param>
        /// <param name="metadataRow">The row the definition was instantiated from</param>
        /// <returns>The definition or null if not found</returns>
        public ReflectedMember GetDefinition(Core.COFF.MetadataTables table, Core.COFF.MetadataRow metadataRow)
        {
            return this.GetDefinition(table, metadataRow.FileOffset);
        }

        public ReflectedMember GetDefinition(Core.COFF.MetadataTables table, int metadataRowOffset)
        {
            ReflectedMember definition = null;
            if(internalMap.ContainsKey(table))
            {
                Dictionary<int, ReflectedMember> tableStore = internalMap[table];
                if(tableStore.ContainsKey(metadataRowOffset))
                {
                    definition = tableStore[metadataRowOffset];
                }
            }
            return definition;
        }

        /// <summary>
        /// The instance of the Assembly this defenition map is used for.
        /// </summary>
        public AssemblyDef Assembly
        {
            get { return _assembly; }
            set { _assembly = value; }
        }
    }
}