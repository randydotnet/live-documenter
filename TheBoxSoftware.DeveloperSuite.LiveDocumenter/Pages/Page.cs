﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace TheBoxSoftware.DeveloperSuite.LiveDocumenter.Pages {
	using TheBoxSoftware.Reflection;
	using TheBoxSoftware.Reflection.Comments;
	using TheBoxSoftware.Reflection.Syntax;
	using TheBoxSoftware.DeveloperSuite.LiveDocumenter.Pages.Elements;

	/// <summary>
	/// Represents a single page in the LiveDocumentor.
	/// </summary>
	public class Page : FlowDocument {
		#region Constructors
		/// <summary>
		/// Initialises a new Page class
		/// </summary>
		public Page() {
			this.Initialise();
		}

		/// <summary>
		/// Initialises a new Page class
		/// </summary>
		/// <param name="title">The title for the page</param>
		public Page(string title)
			: base(new Elements.Header1(title)) {
			this.Initialise();
		}

		/// <summary>
		/// Initialises basic details for the page
		/// </summary>
		private void Initialise() {
			this.Style = null; // (Style)this.FindResource("PageStyle");
			this.OverridesDefaultStyle = true;

			ResourceDictionary dict = new ResourceDictionary();
			Uri uri = new Uri("../Resources/DefaultDocumentationStyle.xaml", UriKind.Relative);
			dict.Source = uri;
			this.Resources.MergedDictionaries.Add(dict);

			this.Style = (Style)this.FindResource("PageStyle");
		}

		/// <summary>
		/// Generates the contents of the page
		/// </summary>
		public virtual void Generate() {
			if (!this.IsGenerated) {
				this.IsGenerated = true;
			}
		}
		#endregion

		#region Methods
		protected Block GetSummaryFor(XmlCodeCommentFile xmlComments, AssemblyDef assembly, string xpath) {
			Block constructorSummary = null;
			XmlCodeComment comment = xmlComments.ReadComment(xpath);
			List<Block> constructorComments = Elements.Parser.Parse(assembly, comment);
			if (constructorComments != null && constructorComments.Count > 0) {
				constructorSummary = constructorComments.First();
			}
			return constructorSummary;
		}

		/// <summary>
		/// Adds the syntax block for the provided <paramref name="member"/>.
		/// </summary>
		/// <param name="member">The member to produce the syntax for.</param>
		protected void AddSyntaxBlock(ReflectedMember member) {
			IFormatter formatter = SyntaxFactory.Create(member, Model.UserApplicationStore.Store.Preferences.Language);
			if (formatter != null) {
				this.Blocks.Add(new Header2("Syntax"));
				Code c = Parser.ParseSyntax(formatter.Format());
				this.Blocks.Add(c);
			}
		}

		/// <summary>
		/// Adds the inheritance tree for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type to parse and display the tree for.</param>
		protected void AddInheritanceTree(TypeDef type) {
			// Add the inheritance tree
			if (type.InheritsFrom != null) {
				List inheritanceList = new List(new Header2("Inherits From"));
				
				// Build a list of parents for the current type
				TypeRef parent = type.InheritsFrom;
				List<Hyperlink> links = new List<Hyperlink>();
				while (parent != null) {
					string displayName = parent.GetDisplayName(true);	// get a default name to display
					TypeDef typeDef = parent as TypeDef;
					if (typeDef != null) {
						displayName = typeDef.GetDisplayName(true);
					}

					Hyperlink link = new Hyperlink(new Run(displayName));
					if (parent.IsExternalReference || typeDef == null) {
						CRefPath parentCrefPath = new CRefPath(parent);
						link.Tag = new CrefEntryKey(parent.Assembly, parentCrefPath.ToString());
					}
					else {
						link.Tag = new EntryKey(Helper.GetUniqueKey(parent.Assembly, typeDef));
					}
					link.Click += new System.Windows.RoutedEventHandler(LinkHelper.Resolve);
					links.Add(link);

					if (parent is TypeDef) {
						parent = ((TypeDef)parent).InheritsFrom;
					}
					else {
						parent = null;
					}
				}

				// Reverse the list and build the inheritance tree
				List lastList = inheritanceList;
				for (int i = links.Count - 1; i >= 0; i--) {
					lastList.AddListItem(links[i]);
					if (i > 0) {
						List nextList = new List();
						lastList.AddChildList(nextList);
						lastList = nextList;
					}
				}

				this.Blocks.Add(inheritanceList);
			}
		}

		/// <summary>
		/// Adds the see also section for the calling element.
		/// </summary>
		/// <param name="parsedBlocks">The parsed comments block for the element.</param>
		/// <remarks>
		/// The <paramref name="pasedBlocks"/> should contain any and all parsed comments
		/// for the calling element. This method will throw the list to the Parser and
		/// then output the seealso comment elements to a list.
		/// </remarks>
		protected void AddSeeAlso(List<Block> parsedBlocks) {
			List<SeeAlso> seeAlsoList = Parser.ParseSeeAlsoElements(parsedBlocks);
			if (seeAlsoList.Count > 0) {
				SeeAlsoList seeAlso = new SeeAlsoList();
				seeAlso.Blocks.Add(new Header2("See Also"));
				System.Windows.Documents.List displayList = new System.Windows.Documents.List();
				foreach (SeeAlso currentSeeAlso in seeAlsoList) {
					displayList.ListItems.Add(new ListItem(new Paragraph(currentSeeAlso.Link)));
				}
				seeAlso.Blocks.Add(displayList);
				this.Blocks.Add(seeAlso);
			}
		}

        protected void AddNamespace(TypeDef representedType) {
            return;
            Paragraph details = new Paragraph();
            Hyperlink namespaceLink = new Hyperlink(new Run(representedType.Namespace));
            namespaceLink.Tag = new CrefEntryKey(representedType.Assembly, "N:" + representedType.Namespace);
            namespaceLink.Click += new System.Windows.RoutedEventHandler(LinkHelper.Resolve);
            details.Inlines.AddRange(new Inline[] {
				  new Bold(new Run("Namespace: ")),
				  namespaceLink
				  });
            this.Blocks.Add(details);
        }
		#endregion

		#region Propeties
		protected bool IsGenerated {
			get;
			set;
		}
		#endregion

		#region Factory Methods
		/// <summary>
		/// Utilises the information provided to create instances of specific Page classes.
		/// </summary>
		/// <param name="forItem">The item to create the Page for</param>
		/// <param name="commentsXml">The comments file where the code comments can be obtained</param>
		/// <param name="mapReference">The DocumentMap to use and to add to</param>
		/// <returns>A new Page instance that can be displayed in the LiveDocumentor</returns>
		public static Page Create(object forItem, XmlCodeCommentFile commentsXml) {
			Page created = null;
			if (forItem is AssemblyDef) {
				created = new AssemblyPage(forItem as AssemblyDef, commentsXml);
			}
			else if (forItem is MethodDef) {
				created = new MethodPage(forItem as MethodDef, commentsXml);
			}
			else if (forItem is List<MethodDef>) {
				created = new TypeMethodsPage(forItem as List<MethodDef>, commentsXml);
			}
			else if (forItem is TypeDef) {
				TypeDef typeDef = forItem as TypeDef;
				if (typeDef.InheritsFrom != null && typeDef.InheritsFrom.GetFullyQualifiedName() == "System.Enum") {
					created = new EnumerationPage(forItem as TypeDef, commentsXml);
				}
				else if (typeDef.IsDelegate) {
					created = new DelegatePage(forItem as TypeDef, commentsXml);
				}
				else {
					created = new TypePage(forItem as TypeDef, commentsXml);
				}
			}
			else if (forItem is KeyValuePair<string, List<TypeDef>>) {
				created = new NamespacePage((KeyValuePair<string, List<TypeDef>>)forItem, commentsXml);
			}
			else if (forItem is List<FieldDef>) {
				created = new TypeFieldsPage(forItem as List<FieldDef>, commentsXml);
			}
			else if (forItem is FieldDef) {
				created = new FieldPage(forItem as FieldDef, commentsXml);
			}
			else if (forItem is List<PropertyDef>) {
				created = new TypePropertiesPage(forItem as List<PropertyDef>, commentsXml);
			}
			else if (forItem is PropertyDef) {
				created = new PropertyPage(forItem as PropertyDef, commentsXml);
			}
			else if (forItem is List<EventDef>) {
				created = new TypeEventsPage(forItem as List<EventDef>, commentsXml);
			}
			else if (forItem is EventDef) {
				created = new EventPage(forItem as EventDef, commentsXml);
			}
			else {
				created = new Page(forItem.ToString());
			}
			return created;
		}

		public static Page Create(object forItem, string type, XmlCodeCommentFile xmlComments) {
			Page created = null;
			switch (type) {
				case "Members":
					created = new TypeMembersPage(forItem as TypeDef, xmlComments);
					break;
				case "Constructors":
					created = new TypeConstructorsPage(forItem as List<MethodDef>, xmlComments);
					break;
				case "Operators":
					created = new TypeOperatorsPage(forItem as List<MethodDef>, xmlComments);
					break;
				case "Component Diagram":
					created = new DeploymentDiagram();
					break;
			}
			return created;
		}
		#endregion
	}
}