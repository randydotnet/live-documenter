﻿
namespace TheBoxSoftware.Documentation.Exporting.Rendering
{
    using System;
    using System.Collections.Generic;
    using Reflection.Syntax;
    using Reflection.Comments;
    using Reflection;

    /// <summary>
    /// Renders the <see cref="Entry"/> to XML
    /// </summary>
    public abstract class XmlRenderer : IRenderer<System.Xml.XmlWriter>
    {
        private bool _includeCRefPath;
        private Entry _associatedEntry;
        private Document _document;

        public abstract void Render(System.Xml.XmlWriter writer);

        /// <summary>
        /// Factory method for creating new instances of <see cref="XmlRenderer"/>. Instantiates
        /// the correct renderer forthe specied document map <see cref="Entry"/>.
        /// </summary>
        /// <param name="entry">The entry in the document map to render.</param>
        /// <param name="exporter">The exporter.</param>
        /// <returns>A valid renderer or null.</returns>
        public static XmlRenderer Create(Entry entry, Document document)
        {
            XmlRenderer renderer = null;

            if (document == null)
                throw new ArgumentNullException("document");

            if (entry.Item is ReflectedMember)
            {
                if (entry.Item is TypeDef && string.IsNullOrEmpty(entry.SubKey))
                {
                    renderer = new TypeXmlRenderer(entry);
                }
                else if (entry.Item is MethodDef)
                {
                    renderer = new MethodXmlRenderer(entry);
                }
                else if (entry.Item is FieldDef)
                {
                    renderer = new FieldXmlRenderer(entry);
                }
                else if (entry.Item is PropertyDef)
                {
                    renderer = new PropertyXmlRenderer(entry);
                }
                else if (entry.Item is EventDef)
                {
                    renderer = new EventXmlRenderer(entry);
                }
            }
            else if (entry.Item is KeyValuePair<string, List<TypeDef>>)
            { // namespace
                renderer = new NamespaceXmlRenderer(entry);
            }
            else if (entry.Item is List<PropertyDef> || entry.Item is List<MethodDef> || entry.Item is List<FieldDef> || entry.Item is List<EventDef>)
            {
                renderer = new TypeMembersXmlRenderer(entry);
            }
            else if (entry.Item is EntryTypes)
            {
                EntryTypes type = (EntryTypes)entry.Item;
                switch (type)
                {
                    case EntryTypes.NamespaceContainer:
                        renderer = new NamespaceContainerXmlRenderer(entry);
                        break;
                }
            }

            renderer.Document = document;

            return renderer;
        }

        /// <summary>
        /// Renders the list of defined exceptions for the specified <paramref name="member"/>.
        /// </summary>
        /// <param name="member">The member to render the exceptions for.</param>
        /// <param name="writer">The writer to write the exceptions to.</param>
        /// <param name="comment">The XmlCodeComment to read the exceptions from.</param>
        protected virtual void RenderExceptionBlock(ReflectedMember member, System.Xml.XmlWriter writer, XmlCodeComment comment)
        {
            // output documentation for expected exceptions if they are defined
            if (comment != XmlCodeComment.Empty)
            {
                List<XmlCodeElement> exceptions = comment.Elements.FindAll(node => node is ExceptionXmlCodeElement);
                if (exceptions != null && exceptions.Count > 0)
                {
                    writer.WriteStartElement("exceptions");
                    for (int i = 0; i < exceptions.Count; i++)
                    {
                        ExceptionXmlCodeElement current = (ExceptionXmlCodeElement)exceptions[i];
                        string exceptionName = string.Empty;
                        ReflectedMember found = null;

                        if (current.Member.PathType != CRefTypes.Error)
                        {
                            TypeDef def = member.Assembly.FindType(current.Member.Namespace, current.Member.TypeName);
                            exceptionName = string.Format("{0}.{1}", current.Member.Namespace, current.Member.TypeName);

                            if (def != null)
                            {
                                found = def;
                                switch (current.Member.PathType)
                                {
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
                                        MethodDef method = current.Member.FindIn(def) as MethodDef;
                                        if (method != null)
                                        {
                                            found = method;
                                            exceptionName = method.GetDisplayName(false);
                                        }
                                        break;
                                    case CRefTypes.Type:
                                        exceptionName = def.GetDisplayName(false);
                                        break;
                                }
                            }
                        }

                        writer.WriteStartElement("exception");
                        writer.WriteStartElement("name");
                        if (found != null)
                        {
                            writer.WriteAttributeString("key", found.GetGloballyUniqueId().ToString());
                            writer.WriteAttributeString("cref", current.Member.ToString());
                        }
                        writer.WriteString(exceptionName);
                        writer.WriteEndElement();
                        writer.WriteStartElement("condition");
                        for (int j = 0; j < current.Elements.Count; j++)
                        {
                            this.Serialize(current.Elements[j], writer);
                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Renders the list of defined exceptions for the specified <paramref name="member"/>.
        /// </summary>
        /// <param name="member">The member to render the exceptions for.</param>
        /// <param name="writer">The writer to write the exceptions to.</param>
        /// <param name="comment">The XmlCodeComment to read the exceptions from.</param>
        protected virtual void RenderPermissionBlock(ReflectedMember member, System.Xml.XmlWriter writer, XmlCodeComment comment)
        {
            // output documentation for expected permissions if they are defined
            if (comment != XmlCodeComment.Empty)
            {
                List<XmlCodeElement> permissions = comment.Elements.FindAll(node => node is PermissionXmlCodeElement);
                if (permissions != null && permissions.Count > 0)
                {
                    writer.WriteStartElement("permissions");
                    for (int i = 0; i < permissions.Count; i++)
                    {
                        PermissionXmlCodeElement current = (PermissionXmlCodeElement)permissions[i];
                        string name = string.Empty;
                        ReflectedMember found = null;

                        if (current.Member.PathType != CRefTypes.Error)
                        {
                            TypeDef def = member.Assembly.FindType(current.Member.Namespace, current.Member.TypeName);
                            name = string.Format("{0}.{1}", current.Member.Namespace, current.Member.TypeName);

                            if (def != null)
                            {
                                found = def;
                                switch (current.Member.PathType)
                                {
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
                                        MethodDef method = current.Member.FindIn(def) as MethodDef;
                                        if (method != null)
                                        {
                                            found = method;
                                            name = method.GetDisplayName(false);
                                        }
                                        break;
                                    case CRefTypes.Type:
                                        name = def.GetDisplayName(false);
                                        break;
                                }
                            }
                        }

                        writer.WriteStartElement("permission");
                        writer.WriteStartElement("name");
                        if (found != null)
                        {
                            writer.WriteAttributeString("key", member.GetGloballyUniqueId().ToString());
                            writer.WriteAttributeString("cref", current.Member.ToString());
                        }
                        writer.WriteString(name);
                        writer.WriteEndElement();
                        writer.WriteStartElement("description");
                        for (int j = 0; j < current.Elements.Count; j++)
                        {
                            this.Serialize(current.Elements[j], writer);
                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Renders the generic types in XML.
        /// </summary>
        /// <param name="genericTypes">The generic types to render.</param>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="comment">The XmlCodeComment to read comments from.</param>
        protected virtual void RenderGenericTypeParameters(List<GenericTypeRef> genericTypes, System.Xml.XmlWriter writer, XmlCodeComment comment)
        {
            writer.WriteStartElement("genericparameters");
            for (int i = 0; i < genericTypes.Count; i++)
            {
                writer.WriteStartElement("parameter");
                writer.WriteElementString("name", genericTypes[i].Name);
                writer.WriteStartElement("description");
                // find and output the summary
                if (comment != XmlCodeComment.Empty)
                {
                    XmlCodeElement paramEntry = comment.Elements.Find(currentBlock =>
                        currentBlock is TypeParamXmlCodeElement
                        && ((TypeParamXmlCodeElement)currentBlock).Name == genericTypes[i].Name);
                    if (paramEntry != null)
                    {
                        writer.WriteString(paramEntry.Text);
                    }
                }
                writer.WriteEndElement(); // description
                writer.WriteEndElement(); // parameter
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Renders the syntax block for the specified <paramref name="member"/>.
        /// </summary>
        /// <param name="member">The member to render the syntax for.</param>
        /// <param name="writer">The writer to write the syntax to.</param>
        protected void RenderSyntaxBlocks(ReflectedMember member, System.Xml.XmlWriter writer)
        {
            IFormatter formatter = SyntaxFactory.Create(member, Languages.CSharp);
            if (formatter != null)
            {
                writer.WriteStartElement("syntaxblocks");
                writer.WriteStartElement("syntax");
                writer.WriteAttributeString("language", "C#");

                foreach (SyntaxToken token in formatter.Format())
                {
                    writer.WriteStartElement(token.TokenType.ToString().ToLower());
                    writer.WriteString(token.Content);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Renders a seealso xml block when the specified member has seealso references in the comments.
        /// </summary>
        /// <param name="member">The member to render the block for.</param>
        /// <param name="writer">The writer to write the xml to.</param>
        /// <param name="comment">The associated xml comments.</param>
        protected void RenderSeeAlsoBlock(ReflectedMember member, System.Xml.XmlWriter writer, XmlCodeComment comment)
        {
            if (comment != XmlCodeComment.Empty)
            {
                List<XmlCodeElement> elements = comment.Elements.FindAll(e => e.Element == XmlCodeElements.SeeAlso);
                if (elements != null && elements.Count > 0)
                {
                    writer.WriteStartElement("seealsolist");
                    foreach (SeeAlsoXmlCodeElement current in elements)
                    {
                        string displayName = current.Member.TypeName;
                        Entry entry = this.Document.Find(current.Member);

                        writer.WriteStartElement("seealso");
                        if (entry != null)
                        {
                            displayName = entry.Name;
                            ReflectedMember foundMember = entry.Item as ReflectedMember;
                            if (foundMember != null)
                            {
                                writer.WriteAttributeString("id", foundMember.GetGloballyUniqueId().ToString());
                                this.WriteCref(entry, writer);
                            }
                        }
                        writer.WriteString(displayName);
                        writer.WriteEndElement(); // seealso
                    }
                    writer.WriteEndElement(); // seealsolist
                }
            }
        }

        /// <summary>
        /// Serializes an <see cref="XmlCodeElement"/> to XML.
        /// </summary>
        /// <param name="comment">The XML code comment to serialize.</param>
        /// <param name="writer">The XmlWriter to serialize to.</param>
        protected void Serialize(XmlCodeElement comment, System.Xml.XmlWriter writer)
        {
            if (comment != XmlCodeComment.Empty)
            {
                if (XmlElementRenderer.IsHandled(comment))
                {
                    XmlRenderer renderer = XmlElementRenderer.Create(this, this.AssociatedEntry, comment);
                    renderer.Render(writer);
                }
                else
                {
                    if (comment is XmlContainerCodeElement)
                    {
                        writer.WriteStartElement(comment.Element.ToString().ToLower());
                        foreach (XmlCodeElement element in ((XmlContainerCodeElement)comment).Elements)
                        {
                            this.Serialize(element, writer);
                        }
                        writer.WriteEndElement();
                    }
                    else
                    {
                        writer.WriteStartElement(comment.Element.ToString().ToLower());
                        writer.WriteString(comment.Text);
                        writer.WriteEndElement();
                    }
                }
            }
        }

        // checks if the entry is a ReflectedMember or a namespace and generates the cref attribute
        // fix: change this so it only outputs when I want it to!
        protected void WriteCref(Entry entry, System.Xml.XmlWriter writer)
        {
            // see DocumentMapper to see how entry.Item is populated
            if (entry.Item is ReflectedMember && !(entry.Item is AssemblyDef))
            { // assemblies cant be cref'd
                ReflectedMember member = entry.Item as ReflectedMember;
                CRefPath path = CRefPath.Create(member);
                writer.WriteAttributeString("cref", path.ToString());
            }
            else if (entry.Item is KeyValuePair<string, List<TypeDef>>)
            { // this is a namespace
                writer.WriteAttributeString("cref", "N:" + ((KeyValuePair<string, List<TypeDef>>)entry.Item).Key);
            }
        }

        /// <summary>
        /// Informs the renderer if the crefpath should be included in the XML output.
        /// </summary>
        public bool IncludeCRefPath
        {
            get { return _includeCRefPath; }
            set { _includeCRefPath = value; }
        }

        /// <summary>
        /// The entry to be rendered in XML.
        /// </summary>
        protected Entry AssociatedEntry
        {
            get { return _associatedEntry; }
            set { _associatedEntry = value; }
        }

        /// <summary>
        /// The Document the entry is a member of.
        /// </summary>
        public Document Document
        {
            get { return _document; }
            set { _document = value; }
        }
    }
}