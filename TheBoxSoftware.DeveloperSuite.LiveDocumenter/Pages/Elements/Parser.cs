using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace TheBoxSoftware.DeveloperSuite.LiveDocumenter.Pages.Elements {
	using TheBoxSoftware.Diagnostics;
	using TheBoxSoftware.Reflection;
	using TheBoxSoftware.Reflection.Comments;
	using TheBoxSoftware.Reflection.Syntax;

	/// <summary>
	/// Parses the text provided in to elements which describe the content
	/// for the current document.
	/// </summary>
	/// <remarks>
	/// <para>The AssemblyDef is required during parsing because it allows us
	/// to store details about the type that is being documented and relate it
	/// to the comments. This is done because every see, seealso etc relates to
	/// a type defined or referenced by that assembly. It will help us resolve
	/// all references when we create links to these elements.</para>
	/// </remarks>
	internal static class Parser {
		internal static string CleanWhitespace(string toClean) {
			return Regex.Replace(toClean, @"\s+", " ").Trim();
		}

		public static Code ParseSyntax(List<SyntaxToken> tokens) {
			Code container = new Code();
			foreach (SyntaxToken current in tokens) {
				switch (current.TokenType) {
					case SyntaxTokens.Text:
						container.Inlines.Add(new Run(current.Content));
						break;
					case SyntaxTokens.Keyword:
						container.Inlines.Add(new Keyword(current.Content));
						break;
				}
			}
			return container;
		}

		/// <summary>
		/// Retrieves all of the see also references from the provided node if
		/// any exist.
		/// </summary>
		/// <param name="nodeToParse">The node to parse</param>
		/// <returns>The list of see also references obtained from the node</returns>
		public static List<SeeAlso> ParseSeeAlsoElements(List<Block> parsedBlocks) {
			List<SeeAlso> list = new List<SeeAlso>();
			foreach (Block current in parsedBlocks) {
				if (current is SeeAlso) {
					list.Add(current as SeeAlso);
				}
			}
			return list;
		}

		public static List<Param> ParseBlockElements(List<Block> parsedBlocks) {
			List<Param> list = new List<Param>();
			foreach (Block current in parsedBlocks) {
				if (current is Param) {
					list.Add(current as Param);
				}
			}
			return list;
		}

		public static List<T> ParseElement<T>(List<Block> parsedBlocks) where T: class {
			List<T> list = new List<T>();
			foreach (Block current in parsedBlocks) {
				if (current is T) {
					list.Add(current as T);
				}
			}
			return list;
		}

		/// <summary>
		/// Loads and parses the XML Code comments for the <paramref name="crefPathToMember"/> specified
		/// element.
		/// </summary>
		/// <param name="assembly">The assembly the member is defined in.</param>
		/// <param name="file">The file containing the xml code comments</param>
		/// <param name="crefPathToMember">The conical name path to the member to get documentation for.</param>
		/// <returns>A List of blocks for the members commentary.</returns>
		public static List<Block> Parse(AssemblyDef assembly, XmlCodeCommentFile file, CRefPath crefPathToMember) {
			XmlCodeComment comment = file.ReadComment(crefPathToMember);
			return Parser.Parse(assembly, comment);
		}

		/// <summary>
		/// Parses the XML Code comments from the <paramref name="comment"/> provided.
		/// </summary>
		/// <param name="assembly">The assembly the member is defined in.</param>
		/// <param name="comment">The parsed XmlCodeComment to parse.</param>
		/// <returns>A List of blocks for the members commentary.</returns>
		public static List<Block> Parse(AssemblyDef assembly, XmlCodeComment comment) {
			List<Block> blocks = new List<Block>();
			if (comment != XmlCodeComment.Empty && comment.Elements.Count > 0) {
				blocks = Parser.Parse(assembly, (XmlContainerCodeElement)comment);
			}
			return blocks;
		}

		private static List<Block> Parse(AssemblyDef assembly, XmlContainerCodeElement container) {
			return Parser.Parse(assembly, container, null);
		}

		private static List<Block> Parse(AssemblyDef assembly, XmlContainerCodeElement container, ParsingSession session) {
			List<Block> blocks = new List<Block>();
			ExceptionList exceptions = new ExceptionList();
			Paragraph inlineContainer = null;
			if(session == null)
				session = new ParsingSession();

			for (int counter = 0; counter < container.Elements.Count; ) {
				if (container.Elements[counter].IsInline) {
					inlineContainer = new Paragraph();
					while (counter < container.Elements.Count && container.Elements[counter].IsInline) {
						inlineContainer.Inlines.Add(Parser.ParseInline(assembly, container.Elements[counter], session));
						counter++;
					}
					blocks.Add(inlineContainer);
					inlineContainer = null;
				}
				else {
					Block block = Parser.ParseBlock(assembly, container.Elements[counter], session);
					if (block != null) {
						if (block is ExceptionEntry) {
							exceptions.Add(block as ExceptionEntry);
						}
						else {
							blocks.Add(block);
						}
					}
					counter++;
				}
			}

			if (exceptions.ExceptionCount > 0) {
				blocks.Add(exceptions);
			}

			return blocks;
		}

		private static Block ParseBlock(AssemblyDef assembly, XmlCodeElement element, ParsingSession session) {
			TraceHelper.WriteLine("parsing-block: e({0})", element.Element.ToString());

			CrefEntryKey crefEntryKey;
			Hyperlink link;
			
			switch (element.Element) {
					// Invalid inline elements
				case XmlCodeElements.C:
				case XmlCodeElements.ParamRef:
				case XmlCodeElements.See:
				case XmlCodeElements.Text:
				case XmlCodeElements.TypeParamRef:
					InvalidOperationException ex = new InvalidOperationException(
						"Found an inline element where a block level element was expected."
						);
					ex.Data["element"] = element;
					throw ex;

				case XmlCodeElements.Include:
					InvalidOperationException includeEx = new InvalidOperationException(
						"Found an include comment element, the c# parser has failed."
						);
					includeEx.Data["element"] = element;
					throw includeEx;

					// End points
				case XmlCodeElements.Code:
					return new Code(element.Text);
				case XmlCodeElements.Exception:
					ExceptionXmlCodeElement exceptionElement = element as ExceptionXmlCodeElement;
					crefEntryKey = new CrefEntryKey(assembly, exceptionElement.Member.ToString());
					link = Parser.CreateHyperlink(crefEntryKey, exceptionElement.Member.TypeName);
					return new ExceptionEntry(link, Parser.Parse(assembly, element as ExceptionXmlCodeElement));
				case XmlCodeElements.Example:
					ExampleXmlCodeElement exampleElement = element as ExampleXmlCodeElement;
					return new Example(Parser.Parse(assembly, exampleElement));
				case XmlCodeElements.Para:
					ParaXmlCodeElement paraElement = element as ParaXmlCodeElement;
					return new Para(Parser.Parse(assembly, paraElement));
				case XmlCodeElements.Remarks:
					return new Remarks(Parser.Parse(assembly, element as RemarksXmlCodeElement));
				case XmlCodeElements.Returns:
					return new Returns(Parser.Parse(assembly, element as ReturnsXmlCodeElement));
				case XmlCodeElements.Summary:
					return new Summary(Parser.Parse(assembly, element as SummaryXmlCodeElement));
				case XmlCodeElements.Value:
					return new Value(Parser.Parse(assembly, element as ValueXmlCodeElement));
				case XmlCodeElements.Param:
					return new Param(((ParamXmlCodeElement)element).Name, Parser.Parse(assembly, element as ParamXmlCodeElement));
				case XmlCodeElements.SeeAlso:
                    // check if the member has been output by visual studio. If the cref is
                    // empty vs did not find it.
                    if (((SeeAlsoXmlCodeElement)element).Member.PathType == CRefTypes.Error) {
                        return new Paragraph();
                    }
					SeeAlsoXmlCodeElement seeAlso = (SeeAlsoXmlCodeElement)element;
					return new SeeAlso(assembly, seeAlso.Member);
				case XmlCodeElements.TypeParam:
					TypeParamXmlCodeElement typeParamElement = (TypeParamXmlCodeElement)element;
					return new TypeParamEntry(typeParamElement.Name, typeParamElement.Text);

				case XmlCodeElements.List:
					ListXmlCodeElement listElement = (ListXmlCodeElement)element;
					return Parser.ParseList(assembly, listElement);

				case XmlCodeElements.Term:
				case XmlCodeElements.Description:
					Section termOrDescription = new Section();
					termOrDescription.Blocks.AddRange(Parser.Parse(assembly, (XmlContainerCodeElement)element));
					return termOrDescription;
					break;

				case XmlCodeElements.Permission:
					//PermissionXmlCodeElement permissionElement = element as PermissionXmlCodeElement;
					//crefEntryKey = new CrefEntryKey(assembly, permissionElement.Member.ToString());
					//link = Parser.CreateHyperlink(crefEntryKey, permissionElement.Member.TypeName);
					//return new ExceptionEntry(link, new Paragraph(new Run(permissionElement.Text)));
					System.Diagnostics.Debug.WriteLine(string.Format("Element {0} not implemented.", element.Element));
					return null;
			}

			throw new Exception("WTF, block parsing error");
		}

		private static Inline ParseInline(AssemblyDef assembly, XmlCodeElement element, ParsingSession session) {
			TraceHelper.WriteLine("parsing-inline: e({0})", element.Element.ToString());

			switch (element.Element) {
					// Invalid elements
				case XmlCodeElements.Code:
				case XmlCodeElements.Example:
				case XmlCodeElements.Exception:
				case XmlCodeElements.List:
				case XmlCodeElements.ListHeader:
				case XmlCodeElements.ListItem:
				case XmlCodeElements.Include:
				case XmlCodeElements.Para:
				case XmlCodeElements.Param:
				case XmlCodeElements.Permission:
				case XmlCodeElements.Remarks:
				case XmlCodeElements.Returns:
				case XmlCodeElements.SeeAlso:
				case XmlCodeElements.Summary:
				case XmlCodeElements.TypeParam:
				case XmlCodeElements.Value:
					InvalidOperationException ex = new InvalidOperationException(
						"Found a block level element where an inline element was expected."
						);
					ex.Data["element"] = element;
					throw ex;

				case XmlCodeElements.B:
					return new Bold(new Run(element.Text));
				case XmlCodeElements.C:
					return new C(element.Text);
				case XmlCodeElements.I:
					return new Italic(new Run(element.Text));
				case XmlCodeElements.ParamRef:
					return new Italic(new Run(element.Text));
				case XmlCodeElements.See:
					// NOTE: Fix this, we currently find the type in our loaded libraries and dont bother to reuse that information.
					// We cant link to elements that dont exist inside our map so we may aswell just show them as text.
					SeeXmlCodeElement seeElement = element as SeeXmlCodeElement;
					CrefEntryKey key = new CrefEntryKey(assembly, seeElement.Member.ToString());

					TraceHelper.Indent();
					TraceHelper.WriteLine("see ({0})", key.CRef);
					TraceHelper.Unindent();

                    // check if the member has been output by visual studio. If the cref is
                    // empty vs did not find it.
                    if (seeElement.Member.PathType == CRefTypes.Error) {
                        return new Run();
                    }

					string displayName = seeElement.Text;
					TheBoxSoftware.Documentation.Entry relatedEntry = LiveDocumentorFile.Singleton.LiveDocument.Find(seeElement.Member);

					if (relatedEntry != null) {
						displayName = relatedEntry.Name;

						switch (seeElement.Member.PathType) {
							// these elements are named and the type of element will
							// not modify how it should be displayed
							case CRefTypes.Field:
							case CRefTypes.Property:
							case CRefTypes.Event:
							case CRefTypes.Namespace:
								break;

							// these could be generic and so will need to modify to
							// a more appropriate display name
							case CRefTypes.Method:
								MethodDef method = relatedEntry.Item as MethodDef;
								if (method != null) {
									displayName = method.GetDisplayName(false);
								}
								break;
							case CRefTypes.Type:
								TypeDef def = relatedEntry.Item as TypeDef;
								if (def != null) {
									displayName = def.GetDisplayName(false);
								}
								break;
						}
						return new See(key, displayName);
					}
					else {
						return new Run(displayName);	// we dont create links to references we dont have in our document
					}					
				case XmlCodeElements.Text:
					return new Run(element.Text);
				case XmlCodeElements.TypeParamRef:
					return new Italic(new Run(element.Text)); 
			}

			throw new Exception("Unknown inline parsing error.");
		}

		private static Hyperlink CreateHyperlink(CrefEntryKey key, string name) {
			Hyperlink link = new Hyperlink(new Run(name));
			link.Tag = key;
			link.Click += new System.Windows.RoutedEventHandler(LinkHelper.Resolve);
			return link;
		}

		/// <summary>
		/// Represents the state or a current parsing session.
		/// </summary>
		/// <remarks>
		/// <para>While parising an XML comments document, it is sometimes necessary
		/// to store and retrieve information about the current element. This
		/// is most visible in list/child element structures. Where it the
		/// child elements need to be structured and entered in to a parent
		/// with knowledge of that parent.</para>
		/// <para>This works as a Queue, so that as child elements add to the list
		/// they can always access there parents and when we unwind everything will
		/// be still access its parents.</para>
		/// </remarks>
		private class ParsingSession : System.Collections.Generic.List<object> {
		}

		/// <summary>
		/// Parses the ListXmlCodeElement in to its correct FlowDocument representation.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="element"></param>
		/// <returns></returns>
		private static Block ParseList(AssemblyDef assembly, ListXmlCodeElement element) {
			Block parsedBlock = null;
			switch (element.ListType) {
				case Reflection.Comments.ListTypes.Bullet:
				case Reflection.Comments.ListTypes.Number:
					List list = new List();
					if (element.ListType == Reflection.Comments.ListTypes.Number) {
						list.InternalList.MarkerStyle = System.Windows.TextMarkerStyle.Decimal;
					}
					else {
						list.InternalList.MarkerStyle = System.Windows.TextMarkerStyle.Disc;
					}

					// only use list items, headers, even if defined, are not used in normal lists
					foreach (ListItemXmlCodeElement listItem in element.Elements.FindAll(e => e is ListItemXmlCodeElement)) {
						ListItem item = new ListItem();
						item.Blocks.AddRange(Parser.Parse(assembly, listItem));
						list.InternalList.ListItems.Add(item);
					}

					parsedBlock = list;
					break;
				case Reflection.Comments.ListTypes.Table:
					SummaryTable table;

					// find the header item and use the titles or use defaults
					ListHeaderXmlCodeElement headerElement = (ListHeaderXmlCodeElement)element.Elements.Find(e => e is ListHeaderXmlCodeElement);
					if (headerElement != null && headerElement.Elements.Count == 2) {
						TermXmlCodeElement termElement = (TermXmlCodeElement)headerElement.Elements.Find(e => e is TermXmlCodeElement);
						DescriptionXmlCodeElement descriptionElement = (DescriptionXmlCodeElement)headerElement.Elements.Find(e => e is DescriptionXmlCodeElement);
						
						Section term = new Section();
						Section description = new Section();
						term.Blocks.AddRange(Parser.Parse(assembly, termElement));
						description.Blocks.AddRange(Parser.Parse(assembly, descriptionElement));

						table = new SummaryTable(term, description, false);
					}
					else {
						table = new SummaryTable("Term", "Description", false);
					}

					foreach (ListItemXmlCodeElement listItem in element.Elements.FindAll(e => e is ListItemXmlCodeElement)) {
						TermXmlCodeElement termElement = (TermXmlCodeElement)listItem.Elements.Find(e => e is TermXmlCodeElement);
						DescriptionXmlCodeElement descriptionElement = (DescriptionXmlCodeElement)listItem.Elements.Find(e => e is DescriptionXmlCodeElement);

						Section term = new Section();
						Section description = new Section();

						if (termElement != null) {
							term.Blocks.AddRange(Parser.Parse(assembly, termElement));
						}
						else if (element.Elements != null && element.Elements.Count > 0) {
							term.Blocks.Add(new Paragraph(new Run(element.Elements[0].Text)));
						}

						if (descriptionElement != null) {
							description.Blocks.AddRange(Parser.Parse(assembly, descriptionElement));
						}
						else if (element.Elements != null && element.Elements.Count > 1) {
							term.Blocks.Add(new Paragraph(new Run(element.Elements[1].Text)));
						}

						table.AddItem(term, description);
					}

					parsedBlock = table;
					break;
			}
			return parsedBlock;
		}
	}
}