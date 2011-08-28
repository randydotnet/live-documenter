﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace TheBoxSoftware.Documentation {
	using TheBoxSoftware.Reflection;
	using TheBoxSoftware.Reflection.Comments;

	public abstract class DocumentMapper {
		protected System.Text.RegularExpressions.Regex illegalFileCharacters;
		private EventHandler<PreEntryAddedEventArgs> preEntryAddedEvent;

		public static DocumentMapper Create(List<DocumentedAssembly> assemblies,
			Mappers typeOfMapper,
			bool useObservableCollection,
			EntryCreator creator) {

			DocumentMapper mapper = null;

			switch (typeOfMapper) {
				case Mappers.AssemblyFirst:
					mapper = new AssemblyFirstDocumentMapper(assemblies, useObservableCollection, creator);
					break;
				case Mappers.NamespaceFirst:
				default:
					mapper = new NamespaceFirstDocumentMapper(assemblies, useObservableCollection, creator);
					break;
			}

			if (mapper == null)
				throw new InvalidOperationException("No document mapper was created.");

			return mapper;
		}

		public static DocumentMap Generate(
			List<DocumentedAssembly> assemblies,
			Mappers typeOfMapper,
			DocumentSettings settings,
			bool useObservableCollection,
			EntryCreator creator) {
			DocumentMapper mapper = DocumentMapper.Create(assemblies, typeOfMapper, useObservableCollection, creator);
			mapper.GenerateMap();
			return mapper.DocumentMap;
		}

		protected List<DocumentedAssembly> CurrentFiles { get; set; }
		public DocumentMap DocumentMap { get; set; }
		protected bool UseObservableCollection { get; set; }
		protected EntryCreator EntryCreator { get; set; }

		protected DocumentMapper(List<DocumentedAssembly> assemblies, bool useObservableCollection, EntryCreator creator) {
			string regex = string.Format("{0}{1}",
				 new string(Path.GetInvalidFileNameChars()),
				 new string(Path.GetInvalidPathChars()));
			illegalFileCharacters = new System.Text.RegularExpressions.Regex(
				string.Format("[{0}]", System.Text.RegularExpressions.Regex.Escape(regex))
				);
			this.CurrentFiles = assemblies;
			this.UseObservableCollection = useObservableCollection;
			this.EntryCreator = creator;
		}

		/// <summary>
		/// Generates the document map based on the <see cref="CurrentFiles"/>.
		/// </summary>
		public virtual void GenerateMap() {
			this.DocumentMap = this.UseObservableCollection ? new ObservableDocumentMap() : new DocumentMap();
			int fileCounter = 1;

			// For each of the documentedfiles generate the document map and add
			// it to the parent node of the document map
			for (int i = 0; i < this.CurrentFiles.Count; i++) {
				if (!this.CurrentFiles[i].IsCompiled)
					continue;
				Entry assemblyEntry = this.GenerateDocumentForAssembly(
					this.CurrentFiles[i], ref fileCounter
					);
				if (assemblyEntry.Children.Count > 0) {
					this.DocumentMap.Add(assemblyEntry);
				}
			}
			this.DocumentMap.OrderBy(e => e.Name);
		}

		/// <summary>
		/// Finds the entry in the document map with the specified key.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="checkChildren">Wether or not to check the child entries</param>
		/// <returns>The entry that relates to the key or null if not found</returns>
		protected Entry FindByKey(long key, string subKey, bool checkChildren) {
			Entry found = null;
			for (int i = 0; i < this.DocumentMap.Count; i++) {
				found = this.DocumentMap[i].FindByKey(key, subKey, checkChildren);
				if (found != null) {
					break;
				}
			}
			return found;
		}

		public virtual Entry GenerateDocumentForAssembly(DocumentedAssembly current, ref int fileCounter) {
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

			Entry assemblyEntry = this.EntryCreator.Create(assembly, System.IO.Path.GetFileName(current.FileName), xmlComments);
			assembly.UniqueId = fileCounter++;
			assemblyEntry.Key = assembly.GetGloballyUniqueId();
			assemblyEntry.IsSearchable = false;
			assemblyEntry.HasXmlComments = fileExists;
			Entry namespaceEntry = null;

			// Add the namespaces to the document map
			foreach (KeyValuePair<string, List<TypeDef>> currentNamespace in assembly.GetTypesInNamespaces()) {
				if (string.IsNullOrEmpty(currentNamespace.Key) || currentNamespace.Value.Count == 0) {
					continue;
				}
				string namespaceSubKey = this.BuildSubkey(currentNamespace);

				namespaceEntry = this.FindByKey(assemblyEntry.Key, namespaceSubKey, false);
				if (namespaceEntry == null) {
					namespaceEntry = this.EntryCreator.Create(currentNamespace, currentNamespace.Key, xmlComments);
					namespaceEntry.Key = assemblyEntry.Key;
					namespaceEntry.SubKey = namespaceSubKey;
					namespaceEntry.IsSearchable = false;
					this.DocumentMap[0].Children.Add(namespaceEntry);
				}

				// Add the types from that namespace to its map
				foreach (TypeDef currentType in currentNamespace.Value) {
					if (currentType.Name.StartsWith("<")) {
						continue;
					}
					PreEntryAddedEventArgs e = new PreEntryAddedEventArgs(currentType);
					this.OnPreEntryAdded(e);

					if (!e.Filter) {
						Entry typeEntry = this.EntryCreator.Create(currentType, currentType.GetDisplayName(false), xmlComments, namespaceEntry);
						typeEntry.Key = currentType.GetGloballyUniqueId();
						typeEntry.IsSearchable = true;

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

						namespaceEntry.Children.Add(typeEntry);
					}
				}
				if (namespaceEntry.Children.Count > 0) {
					namespaceEntry.Children.Sort();
				}
			}

			// Make sure we dont display any empty namespaces
			for (int i = this.DocumentMap[0].Children.Count - 1; i >= 0; i--) {
				Entry entry = this.DocumentMap[0].Children[i];
				if (namespaceEntry.Children.Count == 0) {
					this.DocumentMap[0].Children.RemoveAt(i);
				}
			}

			return namespaceEntry;
		}

		protected string BuildSubkey(KeyValuePair<string, List<TypeDef>> namespaceEntry) {
			TypeDef def = namespaceEntry.Value[0];
			if (def.IsNested) {
				TypeDef container = def.ContainingClass;
				string baseNamespace = container.Namespace;
				List<string> containingClassNames = new List<string>();
				do {
					containingClassNames.Add(container.Name);
					container = container.ContainingClass;
					if (container != null) {
						baseNamespace = container.Namespace;
					}
				}
				while (container != null);
				containingClassNames.Add(baseNamespace);
				containingClassNames.Reverse();

				return string.Join(".", containingClassNames.ToArray());
			}
			else {
				return def.Namespace;
			}
		}

		/// <summary>
		/// Generates the document map for all of the types child elements, fields, properties
		/// and methods.
		/// </summary>
		/// <param name="typeDef">The type to generate the map for.</param>
		/// <param name="typeEntry">The entry to add the child elements to.</param>
		/// <param name="commentsXml">The assembly comment file.</param>
		protected virtual void GenerateTypeMap(TypeDef typeDef, Entry typeEntry, XmlCodeCommentFile commentsXml) {
			List<MethodDef> methods = typeDef.GetMethods();
			List<MethodDef> constructors = typeDef.GetConstructors();
			List<FieldDef> fields = typeDef.GetFields();
			List<PropertyDef> properties = typeDef.GetProperties();
			List<EventDef> events = typeDef.GetEvents();
			List<MethodDef> operators = typeDef.GetOperators();

			if (constructors.Count > 0) {
				Entry constructorsEntry = this.EntryCreator.Create(constructors, "Constructors", commentsXml, typeEntry);
				constructorsEntry.Key = typeDef.GetGloballyUniqueId();
				constructorsEntry.SubKey = "Constructors";
				constructorsEntry.IsSearchable = false;

				// Add the method pages child page entries to the map
				int count = constructors.Count;
				for (int i = 0; i < count; i++) {
					MethodDef currentMethod = constructors[i];
					PreEntryAddedEventArgs e = new PreEntryAddedEventArgs(currentMethod);
					this.OnPreEntryAdded(e);
					if (!e.Filter) {
						Entry constructorEntry = this.EntryCreator.Create(currentMethod, currentMethod.GetDisplayName(false, false), commentsXml, constructorsEntry);
						constructorEntry.IsSearchable = true;
						constructorEntry.Key = currentMethod.GetGloballyUniqueId();
						constructorsEntry.Children.Add(constructorEntry);					
					}
				}
				if (constructorsEntry.Children.Count > 0) {
					constructorsEntry.Children.Sort();
					typeEntry.Children.Add(constructorsEntry);
				}
			}

			// Add a methods containing page and the associated methods
			if (methods.Count > 0) {
				Entry methodsEntry = this.EntryCreator.Create(methods, "Methods", commentsXml, typeEntry);
				methodsEntry.Key = typeDef.GetGloballyUniqueId();
				methodsEntry.SubKey = "Methods";
				methodsEntry.IsSearchable = false;

				// Add the method pages child page entries to the map
				int count = methods.Count;
				for (int i = 0; i < count; i++) {
					MethodDef currentMethod = methods[i];
					PreEntryAddedEventArgs e = new PreEntryAddedEventArgs(currentMethod);
					this.OnPreEntryAdded(e);
					if (!e.Filter) {
						Entry methodEntry = this.EntryCreator.Create(currentMethod, currentMethod.Name, commentsXml, methodsEntry);
						methodEntry.IsSearchable = true;
						methodEntry.Key = currentMethod.GetGloballyUniqueId();
						methodsEntry.Children.Add(methodEntry);
					}
				}
				if (methodsEntry.Children.Count > 0) {
					methodsEntry.Children.Sort();
					typeEntry.Children.Add(methodsEntry);
				}
			}

			if (operators.Count > 0) {
				Entry operatorsEntry = this.EntryCreator.Create(operators, "Operators", commentsXml, typeEntry);
				operatorsEntry.Key = typeDef.GetGloballyUniqueId();
				operatorsEntry.SubKey = "Operators";
				operatorsEntry.IsSearchable = false;

				int count = operators.Count;
				for (int i = 0; i < count; i++) {
					MethodDef current = operators[i];
					PreEntryAddedEventArgs e = new PreEntryAddedEventArgs(current);
					this.OnPreEntryAdded(e);
					if (!e.Filter) {
						Entry operatorEntry = this.EntryCreator.Create(current, current.GetDisplayName(false, false), commentsXml, operatorsEntry);
						operatorEntry.Key = current.GetGloballyUniqueId();
						operatorEntry.IsSearchable = true;
						operatorsEntry.Children.Add(operatorEntry);
					}
				}
				if (operatorsEntry.Children.Count > 0) {
					operatorsEntry.Children.Sort();
					typeEntry.Children.Add(operatorsEntry);
				}
			}

			// Add entries to allow the viewing of the types fields			
			if (fields.Count > 0) {
				Entry fieldsEntry = this.EntryCreator.Create(fields, "Fields", commentsXml, typeEntry);
				fieldsEntry.Key = typeDef.GetGloballyUniqueId();
				fieldsEntry.SubKey = "Fields";
				fieldsEntry.IsSearchable = false;

				foreach (FieldDef currentField in fields) {
					PreEntryAddedEventArgs e = new PreEntryAddedEventArgs(currentField);
					this.OnPreEntryAdded(e);
					if (!e.Filter) {
						Entry fieldEntry = this.EntryCreator.Create(currentField, currentField.Name, commentsXml, fieldsEntry);
						fieldEntry.Key = currentField.GetGloballyUniqueId();
						fieldEntry.IsSearchable = true;
						fieldsEntry.Children.Add(fieldEntry);
					}
				}
				if (fieldsEntry.Children.Count > 0) {
					fieldsEntry.Children.Sort();
					typeEntry.Children.Add(fieldsEntry);
				}
			}

			// Display the properties defined in the current type
			if (properties.Count > 0) {
				Entry propertiesEntry = this.EntryCreator.Create(properties, "Properties", commentsXml, typeEntry);
				propertiesEntry.IsSearchable = false;
				propertiesEntry.Key = typeDef.GetGloballyUniqueId();
				propertiesEntry.SubKey = "Properties";

				foreach (PropertyDef currentProperty in properties) {
					PreEntryAddedEventArgs e = new PreEntryAddedEventArgs(currentProperty);
					this.OnPreEntryAdded(e);
					if (!e.Filter) {
						Entry propertyEntry = this.EntryCreator.Create(currentProperty, currentProperty.Name, commentsXml, propertiesEntry);
						propertyEntry.IsSearchable = true;
						propertyEntry.Key = currentProperty.GetGloballyUniqueId();
						propertiesEntry.Children.Add(propertyEntry);
					}
				}
				if (propertiesEntry.Children.Count > 0) {
					propertiesEntry.Children.Sort();
					typeEntry.Children.Add(propertiesEntry);
				}
			}

			// Display the properties defined in the current type
			if (events.Count > 0) {
				Entry eventsEntry = this.EntryCreator.Create(events, "Events", commentsXml, typeEntry);
				eventsEntry.IsSearchable = false;
				eventsEntry.Key = typeDef.GetGloballyUniqueId();
				eventsEntry.SubKey = "Events";

				foreach (EventDef currentProperty in events) {
					PreEntryAddedEventArgs e = new PreEntryAddedEventArgs(currentProperty);
					this.OnPreEntryAdded(e);
					if (!e.Filter) {
						Entry propertyEntry = this.EntryCreator.Create(currentProperty, currentProperty.Name, commentsXml, eventsEntry);
						propertyEntry.IsSearchable = true;
						propertyEntry.Key = currentProperty.GetGloballyUniqueId();
						eventsEntry.Children.Add(propertyEntry);
					}
				}
				if (eventsEntry.Children.Count > 0) {
					eventsEntry.Children.Sort();
					typeEntry.Children.Add(eventsEntry);
				}
			}
		}

		/// <summary>
		/// Fires before a member is added to the document map.
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected void OnPreEntryAdded(PreEntryAddedEventArgs e) {
			if (this.preEntryAddedEvent != null) {
				this.preEntryAddedEvent(this, e);
			}
		}

		/// <summary>
		/// Event fired before a MemberRef is added to the DocumentMap.
		/// </summary>
		public event EventHandler<PreEntryAddedEventArgs> PreEntryAdded { 
			add { this.preEntryAddedEvent += value;  }
			remove { this.preEntryAddedEvent -= value; }
		}
	}
}