﻿
namespace TheBoxSoftware.Reflection.Comments
{
    using System.Xml;

    public sealed class SeeXmlCodeElement : XmlCodeElement
    {
        internal SeeXmlCodeElement(XmlNode node)
            : base(XmlCodeElements.See)
        {
            if(node.Attributes["cref"] == null) { throw new AttributeRequiredException("cref", XmlCodeElements.See); }

            this.Member = CRefPath.Parse(node.Attributes["cref"].Value);
            switch(this.Member.PathType)
            {
                case CRefTypes.Type:
                    this.Text = this.Member.TypeName;
                    break;
                case CRefTypes.Namespace:
                    this.Text = this.Member.Namespace;
                    break;
                default:
                    this.Text = this.Member.ElementName;
                    break;
            }
            this.IsInline = true;
        }

        /// <summary>
        /// The member this elements points to.
        /// </summary>
        public CRefPath Member { get; set; }
    }
}