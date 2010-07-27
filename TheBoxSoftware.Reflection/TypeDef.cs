﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheBoxSoftware.Reflection {
	using TheBoxSoftware.Reflection.Core.COFF;

	/// <summary>
	/// Contains the information regarding the construction and elements
	/// of a type reflected from the metadata information. A type definition
	/// is a metadata reflected type which is defined in an assembly.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("Type={ToString()}")]
	public class TypeDef : TypeRef {
		private CodedIndex extends;

		#region Methods
		/// <summary>
		/// Creates a new TypeDef instance based on the provided metadata row.
		/// </summary>
		/// <param name="assembly">The assembly the type was defined in</param>
		/// <param name="metadata">The metadata directory</param>
		/// <param name="row">The metadata row which describes the type</param>
		/// <returns></returns>
		internal static TypeDef CreateFromMetadata(AssemblyDef assembly, MetadataDirectory metadata, TypeDefMetadataTableRow row) {
			MetadataToDefinitionMap map = assembly.File.Map;
			TypeDef typeDef = new TypeDef();

			typeDef.UniqueId = assembly.GetUniqueId();
			typeDef.Name = assembly.StringStream.GetString(row.Name.Value);
			typeDef.Namespace = assembly.StringStream.GetString(row.Namespace.Value);
			typeDef.Assembly = assembly;
			typeDef.extends = row.Extends;
			typeDef.GenericTypes = new List<GenericTypeRef>();
			typeDef.Flags = row.Flags;
			typeDef.IsInterface = (typeDef.Flags & TypeAttributes.ClassSemanticMask) == TypeAttributes.Interface;
			typeDef.Implements = new List<TypeRef>();
			typeDef.IsGeneric = typeDef.Name.Contains("`");	// Should be quicker then checking the genparam table

			MetadataStream metadataStream = metadata.GetMetadataStream();
			int indexOfTypeInMetadataTable = metadataStream.Tables.GetIndexFor(MetadataTables.TypeDef, row) + 1;

			// Load the methods, first calculate the number of methods in this type
			int rowIndex = metadataStream.Tables.GetIndexFor(MetadataTables.TypeDef, row);
			int nextRow = rowIndex < metadataStream.Tables[MetadataTables.TypeDef].Length - 1
				? rowIndex + 1
				: -1;

			// Check if this type is generic and load the generic parameters
			if (true) {
				List<GenericParamMetadataTableRow> genericParameters = metadataStream.Tables.GetGenericParametersFor(
					MetadataTables.TypeDef, rowIndex + 1);
				if (genericParameters.Count > 0) {
					foreach (GenericParamMetadataTableRow genParam in genericParameters) {
						typeDef.GenericTypes.Add(GenericTypeRef.CreateFromMetadata(
							assembly, metadata, genParam
							));
					}
				}
			}

			#region Load Methods
			// The methodIndexes variable is used by subsequent loads of properties and events to,
			// get back to the metedata index for the instantiated definition.
			Dictionary<MethodDef, int> methodIndexes = new Dictionary<MethodDef, int>();
			typeDef.Methods = new List<MethodDef>();
			if (metadataStream.Tables.ContainsKey(MetadataTables.MethodDef)) {
				MetadataRow[] table = metadataStream.Tables[MetadataTables.MethodDef];
				int endOfMethodIndex = table.Length + 1;
				if (nextRow != -1) {
					endOfMethodIndex = ((TypeDefMetadataTableRow)metadataStream.Tables[MetadataTables.TypeDef][nextRow]).MethodList;
				}
				// Now load all the methods between our index and the endOfMethodIndex
				for (int i = row.MethodList; i < endOfMethodIndex; i++) {
					MethodMetadataTableRow methodDefRow = (MethodMetadataTableRow)table[i - 1];
					MethodDef method = MethodDef.CreateFromMetadata(assembly, typeDef, metadata, methodDefRow);
					map.Add(MetadataTables.MethodDef, methodDefRow, method);
					methodIndexes.Add(method, i);
					typeDef.Methods.Add(method);
				}
			}
			#endregion

			TypeDef.LoadProperties(assembly, metadata, map, typeDef, metadataStream, indexOfTypeInMetadataTable);
			TypeDef.LoadEvents(assembly, metadata, map, typeDef, metadataStream, indexOfTypeInMetadataTable);

			#region Load Fields
			typeDef.Fields = new List<FieldDef>();
			if (metadataStream.Tables.ContainsKey(MetadataTables.Field)) {
				MetadataRow[] table = metadataStream.Tables[MetadataTables.Field];
				int endOfFieldIndex = table.Length + 1;
				if (nextRow != -1) {
					endOfFieldIndex = ((TypeDefMetadataTableRow)metadataStream.Tables[MetadataTables.TypeDef][nextRow]).FieldList;
				}
				// Now load all the fields between our index and the endOfFieldIndex				
				for (int i = row.FieldList; i < endOfFieldIndex; i++) {
					FieldMetadataTableRow fieldDefRow = (FieldMetadataTableRow)table[i - 1];
					FieldDef field = FieldDef.CreateFromMetadata(assembly, typeDef, fieldDefRow);
					map.Add(MetadataTables.Field, fieldDefRow, field);
					typeDef.Fields.Add(field);
				}
			}
			#endregion

			return typeDef;
		}
		#endregion

		#region Properties
		/// <summary>
		/// The methods this type contains
		/// </summary>
		public List<MethodDef> Methods { get; set; }

		/// <summary>
		/// The fields this type contains
		/// </summary>
		public List<FieldDef> Fields { get; set; }

		/// <summary>
		/// The events this type contains.
		/// </summary>
		public List<EventDef> Events { get; set; }

		/// <summary>
		/// The properties this type contains.
		/// </summary>
		public List<PropertyDef> Properties { get; set; }

		/// <summary>
		/// Returns a reference to the TypeDef or Ref which this type
		/// inherits from.
		/// </summary>
		public TypeRef InheritsFrom {
			get {
				MetadataStream stream = this.Assembly.File.GetMetadataDirectory().GetMetadataStream();
				MetadataToDefinitionMap map = this.Assembly.File.Map;
				TypeRef inheritsFrom = null;

				if (this.extends.Index != 0) {
					inheritsFrom = map.GetDefinition(this.extends.Table,
						stream.Tables.GetEntryFor(this.extends.Table, this.extends.Index)) as TypeRef;
					// We have to handle type spec based classes, as if they use the parent generic types
					// we need the type to have a reference to its container.
					if (inheritsFrom is TypeSpec) {
						((TypeSpec)inheritsFrom).ImplementingType = this;
					}
				}

				return inheritsFrom;
			}
		}

		/// <summary>
		/// Collection of all the generic types that are relevant for this member, this
		/// includes the types defined in parent and containing classes.
		/// </summary>
		/// <seealso cref="GetGenericTypes"/>
		public List<GenericTypeRef> GenericTypes { get; set; }

		/// <summary>
		/// Indicates if this class is an Interface.
		/// </summary>
		public bool IsInterface { get; set; }

		/// <summary>
		/// Indicates if this class is an enumeration.
		/// </summary>
		public bool IsEnumeration {
			get { return this.InheritsFrom != null && this.InheritsFrom.GetFullyQualifiedName() == "System.Enum"; }
		}

		/// <summary>
		/// Indicates if this class is a delegate
		/// </summary>
		public bool IsDelegate {
			get {
				TypeRef parent = this.InheritsFrom;
				if (parent != null) {
					return (parent.Name == "MulticastDelegate" || parent.Name == "Delegate");
				}
				else return false;
			}
		}

		/// <summary>
		/// Flags defining extra information about the type.
		/// </summary>
		public TypeAttributes Flags { get; set; }

		/// <summary>
		/// Indicates if this type has any members defined
		/// </summary>
		public bool HasMembers {
			get {
				return this.Methods.Count > 0 ||
					this.Fields.Count > 0;
			}
		}

		/// <summary>
		/// Returns the namespace for the current class. This is obtained from the
		/// Types metadata.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When the class <see cref="IsNested"/> the namespace returned is the namespace
		/// of the enclosing type. Where the class is nested multiple times each nested class
		/// is checked until one its containers defines a namespace and that is returned.
		/// </para>
		/// </remarks>
		public override string Namespace {
			get {
				if (this.IsNested) {
					StringBuilder sb = new StringBuilder();
					sb.Append(this.ContainingClass.Namespace);

					List<string> containingClassNames = new List<string>();
					TypeDef currentType = this;
					if (currentType.IsNested)
					{
						currentType = currentType.ContainingClass;
						containingClassNames.Add(currentType.Name);
					}
					for (int i = containingClassNames.Count - 1; i >= 0; i--)
					{
						sb.Append(".");
						sb.Append(containingClassNames[i]);
					}

					return sb.ToString();
				}
				return base.Namespace;
			}
			set {
				base.Namespace = value;
			}
		}

		/// <summary>
		/// Indicates if this class is a nested class, if so the <see cref="ContainingClass"/> property
		/// details its container.
		/// </summary>
		public bool IsNested {
			get { return this.ContainingClass != null; }
		}

		/// <summary>
		/// Indicates if this TypeDef is a structure
		/// </summary>
		public bool IsStructure {
			get { return this.InheritsFrom != null && this.InheritsFrom.GetFullyQualifiedName() == "System.ValueType"; }
		}

		/// <summary>
		/// When this class is a nested class this property will contain the class which
		/// owns this class.
		/// </summary>
		public TypeDef ContainingClass { get; set; }

		/// <summary>
		/// Collection of <see cref="TypeRef"/> instances defining the interfaces this class implements.
		/// </summary>
		public List<TypeRef> Implements { get; set; }
		#endregion

		#region Methods
		/// <summary>
		/// Obtains the list of generic types that are defined and owned only by this member.
		/// </summary>
		/// <returns>A collection of generic types for this member</returns>
		public List<GenericTypeRef> GetGenericTypes() {
			List<GenericTypeRef> parameters = new List<GenericTypeRef>();
			string generic = this.Name.Substring(
				this.Name.Length - 1,
				1);
			int numberOfParams = 0;

			if (int.TryParse(generic, out numberOfParams))
			{
				int index = this.GenericTypes.Count - numberOfParams;
				for (int i = index; i < this.GenericTypes.Count; i++)
				{
					parameters.Add(this.GenericTypes[i]);
				}
			}

			return parameters;
		}

		/// <summary>
		/// Obtains only the fields explicitly defined by the developer of the assembly, the fields
		/// property in this class will also return compiler generated backing fields.
		/// </summary>
		/// <returns>The fields in the type without backing fields</returns>
		public List<FieldDef> GetFields() {
			List<FieldDef> fields = new List<FieldDef>();
			for (int i = 0; i < this.Fields.Count; i++) {
				FieldDef currentField = this.Fields[i];
				if (!currentField.Name.StartsWith("<")) {
					fields.Add(currentField);
				}
			}
			return fields;
		}

		/// <summary>
		/// Obtains only the methods, not property accessors or operand overload methods for this type
		/// </summary>
		/// <returns>A collection of MethodDefs representing the methods for this type</returns>
		public List<MethodDef> GetMethods() {
			List<MethodDef> methods = new List<MethodDef>();
			for(int i = 0; i < this.Methods.Count; i++) {
				// IsSpecialName denotes (or appears to) that the method is a compiler
				// generated get|set for properties. Compiler generated code for linq
				// expressions are not 'special name' so need to be checked for seperately.
				if (!this.Methods[i].IsCompilerGenerated) {
					methods.Add(this.Methods[i]);
				}
			}
			return methods;
		}

		/// <summary>
		/// Returns a collection of constructor methods defined for this type.
		/// </summary>
		/// <returns>A collection of zero or more constructors defined in this TypeDef.</returns>
		public List<MethodDef> GetConstructors() {
			List<MethodDef> methods = new List<MethodDef>();
			for(int i = 0; i < this.Methods.Count; i++) {
				if (this.Methods[i].IsConstructor) {
					methods.Add(this.Methods[i]);
				}
			}
			return methods;
		}

		/// <summary>
		/// Returns a collection of operator methods defined for this type.
		/// </summary>
		/// <returns>A collection of zero or more operators defined in this TypeDef.</returns>
		public List<MethodDef> GetOperators() {
			List<MethodDef> methods = new List<MethodDef>();
			for (int i = 0; i < this.Methods.Count; i++) {
				if (this.Methods[i].IsOperator) {
					methods.Add(this.Methods[i]);
				}
			}
			return methods;
		}

		/// <summary>
		/// Returns a collection of properties that have been defined for this type. Properties
		/// are stored as a series of MethodDef structures, representing the get_ and set_
		/// methods. The PropertyDef wraps these methods up and provides a nice wrapper to
		/// retrieve the names and other details of the property.
		/// </summary>
		/// <returns>A collection of properties defined for the type.</returns>
		public List<PropertyDef> GetProperties() {
			return this.Properties;
		}

		/// <summary>
		/// Obtains a collection of events that are defined in this type.
		/// </summary>
		/// <returns>The collection of events.</returns>
		public List<EventDef> GetEvents() {
			return this.Events;
		}

		/// <summary>
		/// Loads the details of the events defined in this type.
		/// </summary>
		/// <param name="assembly">The assembly this type is defined in.</param>
		/// <param name="metadata">The metadata that defines the type.</param>
		/// <param name="map">The map to store and retrieve related information from.</param>
		/// <param name="typeDef">The type to populate with the loaded events.</param>
		/// <param name="metadataStream">The easy access metadata stream for the metadata.</param>
		/// <param name="indexOfTypeInMetadataTable">The index of the parent type in the metadata table.</param>
		private static void LoadEvents(AssemblyDef assembly, MetadataDirectory metadata, MetadataToDefinitionMap map, TypeDef typeDef, MetadataStream metadataStream, int indexOfTypeInMetadataTable) {
			typeDef.Events = new List<EventDef>();

			// Check if we have a property map and then find the property map for the current type
			// if it exists.
			if (metadataStream.Tables.ContainsKey(MetadataTables.EventMap)) {
				int startEventList = -1;
				int endEventList = -1;

				EventMapMetadataTableRow searchFor = new EventMapMetadataTableRow();
				searchFor.Parent = indexOfTypeInMetadataTable;

				int mapIndex = Array.BinarySearch<MetadataRow>(metadataStream.Tables[MetadataTables.EventMap],
					searchFor,
					new EventMapComparer()
					);
				if (mapIndex >= 0) {
					startEventList = ((EventMapMetadataTableRow)metadataStream.Tables[MetadataTables.EventMap][mapIndex]).EventList;
					if (mapIndex < metadataStream.RowsInPresentTables[MetadataTables.EventMap] - 1) {
						endEventList = ((EventMapMetadataTableRow)metadataStream.Tables[MetadataTables.EventMap][mapIndex + 1]).EventList - 1;
					}
					else {
						endEventList = metadataStream.RowsInPresentTables[MetadataTables.Event];
					}
				}

				// If we have properties we need to load them, instantiate a PropertyDef and relate
				// it to its getter and setter.
				if (startEventList != -1) {
					MetadataRow[] table = metadataStream.Tables[MetadataTables.Event];
					// Now load all the methods between our index and the endOfMethodIndex
					for (int i = startEventList; i <= endEventList; i++) {
						EventMetadataTableRow eventRow = (EventMetadataTableRow)table[i - 1];
						EventDef eventDef = EventDef.CreateFromMetadata(assembly, typeDef, metadata, eventRow);

						// TODO: Find and set the getter and setter for the property.. at some point

						map.Add(MetadataTables.Event, eventRow, eventDef);
						typeDef.Events.Add(eventDef);
					}
				}
			}
		}

		/// <summary>
		/// Loads the details of the properties defined in this type.
		/// </summary>
		/// <param name="assembly">The assembly this type is defined in.</param>
		/// <param name="metadata">The metadata that defines the type.</param>
		/// <param name="map">The map to store and retrieve related information from.</param>
		/// <param name="typeDef">The type to populate with the loaded events.</param>
		/// <param name="metadataStream">The easy access metadata stream for the metadata.</param>
		/// <param name="indexOfTypeInMetadataTable">The index of the parent type in the metadata table.</param>
		private static void LoadProperties(AssemblyDef assembly, MetadataDirectory metadata, MetadataToDefinitionMap map, TypeDef typeDef, MetadataStream metadataStream, int indexOfTypeInMetadataTable) {
			typeDef.Properties = new List<PropertyDef>();
			// Check if we have a property map and then find the property map for the current type
			// if it exists.
			if (metadataStream.Tables.ContainsKey(MetadataTables.PropertyMap)) {
				// TODO: The metadata tables are in order, we should use a sorted search algorithm to
				// find elements we need.
				int startPropertyList = -1;
				int endPropertyList = -1;
				PropertyMapMetadataTableRow searchFor = new PropertyMapMetadataTableRow();
				searchFor.Parent = indexOfTypeInMetadataTable;
				int mapIndex = Array.BinarySearch<MetadataRow>(metadataStream.Tables[MetadataTables.PropertyMap],
					searchFor,
					new PropertyMapComparer()
					);
				if (mapIndex >= 0) {
					startPropertyList = ((PropertyMapMetadataTableRow)metadataStream.Tables[MetadataTables.PropertyMap][mapIndex]).PropertyList;
					if (mapIndex < metadataStream.RowsInPresentTables[MetadataTables.PropertyMap] - 1) {
						endPropertyList = ((PropertyMapMetadataTableRow)metadataStream.Tables[MetadataTables.PropertyMap][mapIndex + 1]).PropertyList - 1;
					}
					else {
						endPropertyList = metadataStream.RowsInPresentTables[MetadataTables.Property];
					}
				}

				// If we have properties we need to load them, instantiate a PropertyDef and relate
				// it to its getter and setter.
				if (startPropertyList != -1) {
					MetadataRow[] table = metadataStream.Tables[MetadataTables.Property];
					MetadataRow[] methodSemantics = metadataStream.Tables[MetadataTables.MethodSemantics];
					// Now load all the methods between our index and the endOfMethodIndex
					for (int i = startPropertyList; i <= endPropertyList; i++) {
						PropertyMetadataTableRow propertyRow = (PropertyMetadataTableRow)table[i - 1];
						PropertyDef property = PropertyDef.CreateFromMetadata(assembly, typeDef, metadata, propertyRow);

						// Get the related getter and setter methods
						for (int j = 0; j < methodSemantics.Length; j++) {
							MethodSemanticsMetadataTableRow semantics = (MethodSemanticsMetadataTableRow)methodSemantics[j];
							CodedIndex index = semantics.Association;
							if (index.Table == MetadataTables.Property && index.Index == i) {
								if (semantics.Semantics == MethodSemanticsAttributes.Setter) {
									property.SetMethod = map.GetDefinition(
										MetadataTables.MethodDef,
										metadataStream.GetEntryFor(MetadataTables.MethodDef, semantics.Method)
										) as MethodDef;
								}
								else if (semantics.Semantics == MethodSemanticsAttributes.Getter) {
									property.GetMethod = map.GetDefinition(
										MetadataTables.MethodDef,
										metadataStream.GetEntryFor(MetadataTables.MethodDef, semantics.Method)
										) as MethodDef;
								}
							}
						}

						map.Add(MetadataTables.Property, propertyRow, property);
						typeDef.Properties.Add(property);
					}
				}
			}
		}

		public PropertyDef FindPropertyByName(string name) {
			return this.GetProperties().First(property => property.Name == name);
		}

		public FieldDef FindFieldByName(string name) {
			return this.Fields.First(field => field.Name == name);
		}

		public MethodDef FindMethodByName(string name) {
			return this.Methods.First(method => method.Name == name);
		}
		#endregion

		#region Internals
		/// <summary>
		/// Internal comparer to enable fast binary searching of the event table.
		/// </summary>
		private class EventMapComparer : IComparer<MetadataRow> {
			public int Compare(MetadataRow x, MetadataRow y) {
				return this.Compare((EventMapMetadataTableRow)x, (EventMapMetadataTableRow)y);
			}
			public int Compare(EventMapMetadataTableRow x, EventMapMetadataTableRow y) {
				if (x.Parent < y.Parent) return -1;
				if (x.Parent == y.Parent) return 0;
				return 1;
			}
		}

		/// <summary>
		/// Internal comparer to enable fast binary searching of the property table.
		/// </summary>
		private class PropertyMapComparer : IComparer<MetadataRow> {
			public int Compare(MetadataRow x, MetadataRow y) {
				return this.Compare((PropertyMapMetadataTableRow)x, (PropertyMapMetadataTableRow)y);
			}
			public int Compare(PropertyMapMetadataTableRow x, PropertyMapMetadataTableRow y) {
				if (x.Parent < y.Parent) return -1;
				if (x.Parent == y.Parent) return 0;
				return 1;
			}
		}
		#endregion
	}
}