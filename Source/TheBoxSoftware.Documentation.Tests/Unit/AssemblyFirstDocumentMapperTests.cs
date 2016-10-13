﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TheBoxSoftware.Documentation;

namespace TheBoxSoftware.Documentation.Tests.Unit
{
    [TestFixture]
    public class AssemblyFirstDocumentMapperTests
    {
        [Test]
        public void AssemblyFirstDocumentMapper_WhenCreatedWithNoAssemblies_MapIsEmpty()
        {
            List<DocumentedAssembly> assemblies = new List<DocumentedAssembly>();
            EntryCreator entryCreator = new EntryCreator();
            bool useObservableCollection = false;

            AssemblyFirstDocumentMapper mapper = new AssemblyFirstDocumentMapper(assemblies, useObservableCollection, entryCreator);
            DocumentMap map = mapper.GenerateMap();

            Assert.AreEqual(0, entryCreator.Created);
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void AssemblyFirstDocumentMapper_When_Should()
        {
            List<DocumentedAssembly> assemblies = new List<DocumentedAssembly>();
            EntryCreator entryCreator = new EntryCreator();
            bool useObservableCollection = false;

            DocumentedAssembly documented = new DocumentedAssembly();
            documented.FileName = "test.dll";
            assemblies.Add(documented);

            AssemblyFirstDocumentMapper mapper = new AssemblyFirstDocumentMapper(assemblies, useObservableCollection, entryCreator);
            DocumentMap map = mapper.GenerateMap();

            Assert.AreEqual(0, entryCreator.Created);
        }
    }
}
