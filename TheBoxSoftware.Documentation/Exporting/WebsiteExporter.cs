﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Saxon.Api;
using System.IO;

namespace TheBoxSoftware.Documentation.Exporting {
	using TheBoxSoftware.Reflection;
	using TheBoxSoftware.Reflection.Comments;

	public class WebsiteExporter : Exporter {
		private System.Text.RegularExpressions.Regex illegalFileCharacters;
		private string tempdirectory = string.Empty;
		private ExportConfigFile config = null;
		private int currentExportStep = 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="WebsiteExporter"/> class.
		/// </summary>
		/// <param name="assemblies">The assemblies.</param>
		public WebsiteExporter(List<DocumentedAssembly> assemblies, ExportSettings settings, ExportConfigFile config) 
			: base(assemblies, settings) {
			string regex = string.Format("{0}{1}",
				 new string(Path.GetInvalidFileNameChars()),
				 new string(Path.GetInvalidPathChars()));
			illegalFileCharacters = new System.Text.RegularExpressions.Regex(
				string.Format("[{0}]", System.Text.RegularExpressions.Regex.Escape(regex))
				);
			this.config = config;
		}

		#region Document Map
		protected override void GenerateDocumentMap() {
			this.DocumentMap = new List<Entry>();
			int fileCounter = 1;

			Entry parentEntry = new Entry(null, "Title provided by user", null);
			this.DocumentMap.Add(parentEntry);

			// For each of the documentedfiles generate the document map and add
			// it to the parent node of the document map
			for (int i = 0; i < this.CurrentFiles.Count; i++) {
				Entry assemblyEntry = this.GenerateDocumentForAssembly(
					this.CurrentFiles[i], ref fileCounter
					);
			}
			parentEntry.Children.Sort();
		}

		public Entry GenerateDocumentForAssembly(DocumentedAssembly current, ref int fileCounter) {
			AssemblyDef assembly = AssemblyDef.Create(current.FileName);
			current.LoadedAssembly = assembly;

			XmlCodeCommentFile xmlComments = null;
			bool fileExists = System.IO.File.Exists(current.XmlFileName);
			if (fileExists) {
				xmlComments = new XmlCodeCommentFile(current.XmlFileName).GetReusableFile();
			}
			else {
				xmlComments = new XmlCodeCommentFile();
			}

			Entry assemblyEntry = new Entry(assembly, System.IO.Path.GetFileName(current.FileName), xmlComments);
			assembly.UniqueId = fileCounter++;
			assemblyEntry.Key = this.GetUniqueKey(assembly);
			assemblyEntry.IsSearchable = false;
			assemblyEntry.HasXmlComments = fileExists;
			Entry namespaceEntry = null;

			// Add the namespaces to the document map
			foreach (KeyValuePair<string, List<TypeDef>> currentNamespace in assembly.GetTypesInNamespaces()) {
				if (string.IsNullOrEmpty(currentNamespace.Key)) {
					continue;
				}
				namespaceEntry = this.FindByKey(assemblyEntry.Key, currentNamespace.Key, false);
				//namespaceEntry.Item = currentNamespace;
				if (namespaceEntry == null) {
					namespaceEntry = new Entry(currentNamespace, currentNamespace.Key, xmlComments);
					namespaceEntry.Key = assemblyEntry.Key;
					namespaceEntry.SubKey = currentNamespace.Key;
					namespaceEntry.IsSearchable = false;
					namespaceEntry.FullName = currentNamespace.Key;
					this.DocumentMap[0].Children.Add(namespaceEntry);
				}

				// Add the types from that namespace to its map
				foreach (TypeDef currentType in currentNamespace.Value) {
					if (currentType.Name.StartsWith("<")) {
						continue;
					}
					Entry typeEntry = new Entry(currentType, currentType.GetDisplayName(false), xmlComments, namespaceEntry);
					typeEntry.Key = this.GetUniqueKey(assembly, currentType);
					typeEntry.IsSearchable = true;
					typeEntry.FullName = currentType.GetFullyQualifiedName();
					namespaceEntry.Children.Add(typeEntry);

					// For some elements we will not want to load the child objects
					// this is currently for System.Enum derived values.
					if (
						currentType.InheritsFrom != null && currentType.InheritsFrom.GetFullyQualifiedName() == "System.Enum" ||
						currentType.IsDelegate) {
						// Ignore children
					}
					else {
						this.GenerateTypeMap(currentType, typeEntry, xmlComments);
						typeEntry.Children.Sort();
					}
				}
				if (namespaceEntry.Children.Count > 0) {
					namespaceEntry.Children.Sort();
				}
			}

			return namespaceEntry;
		}

		/// <summary>
		/// Generates the document map for all of the types child elements, fields, properties
		/// and methods.
		/// </summary>
		/// <param name="typeDef">The type to generate the map for.</param>
		/// <param name="typeEntry">The entry to add the child elements to.</param>
		/// <param name="commentsXml">The assembly comment file.</param>
		private void GenerateTypeMap(TypeDef typeDef, Entry typeEntry, XmlCodeCommentFile commentsXml) {
			List<MethodDef> methods = typeDef.GetMethods();
			List<MethodDef> constructors = typeDef.GetConstructors();
			List<FieldDef> fields = typeDef.GetFields();
			List<PropertyDef> properties = typeDef.GetProperties();
			List<EventDef> events = typeDef.GetEvents();
			List<MethodDef> operators = typeDef.GetOperators();

			if (typeDef.HasMembers) {
				Entry membersEntry = new Entry(typeDef, "Members", commentsXml, typeEntry);
				membersEntry.Key = this.GetUniqueKey(typeDef.Assembly, typeDef);
				membersEntry.SubKey = "Members";
				typeEntry.Children.Add(membersEntry);
			}

			if (constructors.Count > 0) {
				Entry constructorsEntry = new Entry(constructors, "Constructors", commentsXml, typeEntry);
				constructorsEntry.Key = this.GetUniqueKey(typeDef.Assembly, typeDef);
				constructorsEntry.SubKey = "Constructors";
				constructorsEntry.IsSearchable = false;
				typeEntry.Children.Add(constructorsEntry);

				// Add the method pages child page entries to the map
				int count = constructors.Count;
				for (int i = 0; i < count; i++) {
					// foreach (MethodDef currentMethod in constructors) {
					MethodDef currentMethod = constructors[i];
					Entry constructorEntry = new Entry(currentMethod, currentMethod.GetDisplayName(false, false), commentsXml, constructorsEntry);
					constructorEntry.IsSearchable = true;
					constructorEntry.Key = this.GetUniqueKey(typeDef.Assembly, currentMethod);
					constructorsEntry.Children.Add(constructorEntry);
				}
				constructorsEntry.Children.Sort();
			}

			// Add a methods containing page and the associated methods
			if (methods.Count > 0) {
				Entry methodsEntry = new Entry(methods, "Methods", commentsXml, typeEntry);
				methodsEntry.Key = this.GetUniqueKey(typeDef.Assembly, typeDef);
				methodsEntry.SubKey = "Methods";
				methodsEntry.IsSearchable = false;
				typeEntry.Children.Add(methodsEntry);

				// Add the method pages child page entries to the map
				int count = methods.Count;
				for (int i = 0; i < count; i++) {
					MethodDef currentMethod = methods[i];
					Entry methodEntry = new Entry(currentMethod, currentMethod.Name, commentsXml, methodsEntry);
					methodEntry.IsSearchable = true;
					methodEntry.Key = this.GetUniqueKey(typeDef.Assembly, currentMethod);
					methodsEntry.Children.Add(methodEntry);
				}
				methodsEntry.Children.Sort();
			}

			if (operators.Count > 0) {
				Entry operatorsEntry = new Entry(operators, "Operators", commentsXml, typeEntry);
				operatorsEntry.Key = this.GetUniqueKey(typeDef.Assembly, typeDef);
				operatorsEntry.SubKey = "Operators";
				operatorsEntry.IsSearchable = false;
				typeEntry.Children.Add(operatorsEntry);

				int count = operators.Count;
				for (int i = 0; i < count; i++) {
					MethodDef current = operators[i];
					Entry operatorEntry = new Entry(current, current.GetDisplayName(false, false), commentsXml, operatorsEntry);
					operatorEntry.Key = this.GetUniqueKey(typeDef.Assembly, current);
					operatorEntry.IsSearchable = true;
					operatorsEntry.Children.Add(operatorEntry);
				}
				operatorsEntry.Children.Sort();
			}

			// Add entries to allow the viewing of the types fields			
			if (fields.Count > 0) {
				Entry fieldsEntry = new Entry(fields, "Fields", commentsXml, typeEntry);
				fieldsEntry.Key = this.GetUniqueKey(typeDef.Assembly, typeDef);
				fieldsEntry.SubKey = "Fields";
				fieldsEntry.IsSearchable = false;
				typeEntry.Children.Add(fieldsEntry);

				foreach (FieldDef currentField in fields) {
					Entry fieldEntry = new Entry(currentField, currentField.Name, commentsXml, fieldsEntry);
					fieldEntry.Key = this.GetUniqueKey(typeDef.Assembly, currentField);
					fieldEntry.IsSearchable = true;
					fieldsEntry.Children.Add(fieldEntry);
				}
				fieldsEntry.Children.Sort();
			}

			// Display the properties defined in the current type
			if (properties.Count > 0) {
				Entry propertiesEntry = new Entry(properties, "Properties", commentsXml, typeEntry);
				propertiesEntry.IsSearchable = false;
				propertiesEntry.Key = this.GetUniqueKey(typeDef.Assembly, typeDef);
				propertiesEntry.SubKey = "Properties";
				typeEntry.Children.Add(propertiesEntry);

				foreach (PropertyDef currentProperty in properties) {
					Entry propertyEntry = new Entry(currentProperty, currentProperty.Name, commentsXml, propertiesEntry);
					propertyEntry.IsSearchable = true;
					propertyEntry.Key = this.GetUniqueKey(typeDef.Assembly, currentProperty);
					propertiesEntry.Children.Add(propertyEntry);
				}
				propertiesEntry.Children.Sort();
			}

			// Display the properties defined in the current type
			if (events.Count > 0) {
				Entry propertiesEntry = new Entry(events, "Events", commentsXml, typeEntry);
				propertiesEntry.IsSearchable = false;
				propertiesEntry.Key = this.GetUniqueKey(typeDef.Assembly, typeDef);
				propertiesEntry.SubKey = "Events";
				typeEntry.Children.Add(propertiesEntry);

				foreach (EventDef currentProperty in events) {
					Entry propertyEntry = new Entry(currentProperty, currentProperty.Name, commentsXml, propertiesEntry);
					propertyEntry.IsSearchable = true;
					propertyEntry.Key = this.GetUniqueKey(typeDef.Assembly, currentProperty);
					propertiesEntry.Children.Add(propertyEntry);
				}
				propertiesEntry.Children.Sort();
			}
		}
		#endregion

		/// <summary>
		/// Exports the full contained documentation.
		/// </summary>
		public override void Export() {
			// the temp output directory
			this.tempdirectory = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName()) + "\\";

			try {
				this.PrepareDirectory(tempdirectory);

				this.GenerateDocumentMap();
				this.OnExportCalculated(new ExportCalculatedEventArgs(6));
				this.currentExportStep = 1;

				Documentation.Exporting.Rendering.DocumentMapXmlRenderer map = new Documentation.Exporting.Rendering.DocumentMapXmlRenderer(
					this.DocumentMap
					);

				// export the document map
				this.OnExportStep(new ExportStepEventArgs("Export as XML...", ++this.currentExportStep));
				using (XmlWriter writer = XmlWriter.Create(string.Format("{0}/toc.xml", this.tempdirectory))) {
					map.Render(writer);
				}				

				// export the index page
				IndexXmlRenderer indexPage = new IndexXmlRenderer(this.DocumentMap);
				using (XmlWriter writer = XmlWriter.Create(string.Format("{0}/index.xml", this.tempdirectory))) {
					indexPage.Render(writer);
				}

				// export each of the members
				foreach (Entry current in this.DocumentMap) {
					this.RecursiveEntryExport(current);
				}

				Processor p = new Processor();
				Uri xsltLocation = new Uri(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location), "ApplicationData/livexmltohtml.xslt");
				XsltTransformer transform = p.NewXsltCompiler().Compile(this.config.GetXslt()).Load();
				transform.SetParameter(new QName(new XmlQualifiedName("directory")), new XdmAtomicValue(System.IO.Path.GetFullPath(tempdirectory)));

				// Finally perform the user selected output xslt
				this.OnExportStep(new ExportStepEventArgs("Preparing output directory", ++this.currentExportStep));
				this.PrepareDirectory(@"temp\output");

				// set output files
				this.OnExportStep(new ExportStepEventArgs("Saving output files...", ++this.currentExportStep));
				this.config.SaveOutputFilesTo(@"temp\output");

				this.OnExportStep(new ExportStepEventArgs("Transforming XML...", ++this.currentExportStep));
				foreach (string current in Directory.GetFiles(this.tempdirectory)) {
					if (current == "toc.xml")
						continue;
					using (FileStream fs = File.OpenRead(current)) {
						Serializer s = new Serializer();
						s.SetOutputFile(@"temp\output\" + Path.GetFileNameWithoutExtension(current) + ".htm");
						transform.SetInputStream(fs, new Uri(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location), @"temp\output\"));
						transform.Run(s);
					}
				}
			}
			catch (Exception ex) {
				throw new ExportException(ex.Message, ex);
			}
			finally {
				// clean up the temp directory
				this.OnExportStep(new ExportStepEventArgs("Cleaning up", ++this.currentExportStep));
				System.IO.Directory.Delete(this.tempdirectory, true);
			}
		}

		/// <summary>
		/// A method that recursively, through the documentation tree, exports all of the
		/// found pages for each of the entries in the documentation.
		/// </summary>
		/// <param name="currentEntry">The current entry to export</param>
		private void RecursiveEntryExport(Entry currentEntry) {
			this.Export(currentEntry);
			for (int i = 0; i < currentEntry.Children.Count; i++) {
				this.RecursiveEntryExport(currentEntry.Children[i]);
			}
		}

		private void Export(Entry current) {
			System.Diagnostics.Debug.WriteLine("SaveXaml: " + current.Name + "[" + current.Key + ", " + current.SubKey + "]");
			System.Diagnostics.Debug.Indent();

			Rendering.XmlRenderer r = null;
			if (current.Item is ReflectedMember) {
				System.Diagnostics.Debug.WriteLine("Is ReflectedMember");
				r = Rendering.XmlRenderer.Create(current, (ReflectedMember)current.Item, current.XmlCommentFile, this);
			}
			else if (current.Item is List<MethodDef>) {
				TypeDef parent = current.Parent.Item as TypeDef;
				r = Rendering.XmlRenderer.Create(current, parent,
					new List<ReflectedMember>(((List<MethodDef>)current.Item).ToArray()), current.XmlCommentFile, this);
			}
			else if (current.Item is List<FieldDef>) {
				TypeDef parent = current.Parent.Item as TypeDef;
				r = Rendering.XmlRenderer.Create(current, parent,
					new List<ReflectedMember>(((List<FieldDef>)current.Item).ToArray()), current.XmlCommentFile, this);
			}
			else if (current.Item is List<PropertyDef>) {
				TypeDef parent = current.Parent.Item as TypeDef;
				r = Rendering.XmlRenderer.Create(current, parent,
					new List<ReflectedMember>(((List<PropertyDef>)current.Item).ToArray()), current.XmlCommentFile, this);
			}
			else if (current.Item is List<EventDef>) {
				TypeDef parent = current.Parent.Item as TypeDef;
				r = Rendering.XmlRenderer.Create(current, parent,
					new List<ReflectedMember>(((List<EventDef>)current.Item).ToArray()), current.XmlCommentFile, this);
			}

			if (r != null) {
				string filename = string.Format("{0}/{1}{2}.xml", this.tempdirectory, current.Key, string.IsNullOrEmpty(current.SubKey) ? string.Empty : "-" + current.SubKey);
				using (System.Xml.XmlWriter writer = XmlWriter.Create(filename)) {
					r.Render(writer);
				}
				System.Diagnostics.Debug.WriteLine("File: " + filename);
			}

			System.Diagnostics.Debug.Unindent();
		}

		/// <summary>
		/// Ensures there is an empty temp directory to proccess the documentation in.
		/// </summary>
		private void PrepareDirectory(string directory) {
			if (!Directory.Exists(directory)) {
				Directory.CreateDirectory(directory);
			}
			else {
				Directory.Delete(directory, true);
				Directory.CreateDirectory(directory);
			}
		}

		private class IndexXmlRenderer : Rendering.XmlRenderer {
			private List<Entry> documentMap = null;
			
			public IndexXmlRenderer(List<Entry> documentMap) {
				this.documentMap = documentMap;
			}

			public override void Render(XmlWriter writer) {
				writer.WriteStartDocument();
				writer.WriteStartElement("frontpage");
				writer.WriteAttributeString("id", "0");
				writer.WriteAttributeString("subId", string.Empty);

				writer.WriteStartElement("title");
				writer.WriteEndElement();

				// write all the namespaces
				writer.WriteStartElement("namespaces");
				foreach (Entry current in this.documentMap[0].Children) {
					writer.WriteStartElement("namespace");
					writer.WriteAttributeString("key", current.Key.ToString());
					writer.WriteAttributeString("subkey", current.SubKey);

					writer.WriteStartElement("name");
					writer.WriteString(current.SubKey);
					writer.WriteEndElement();

					writer.WriteEndElement();
				}
				writer.WriteEndElement();

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
		}
	}
}