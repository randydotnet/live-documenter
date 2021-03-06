﻿
namespace TheBoxSoftware.Reflection.Comments
{
    using System.Xml;

    public sealed class ReturnsXmlCodeElement : XmlContainerCodeElement
    {
        internal ReturnsXmlCodeElement(XmlNode node)
            : base(XmlCodeElements.Returns)
        {
            IsBlock = true;
            Elements = Parse(node);
        }
    }
}