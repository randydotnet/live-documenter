﻿
namespace TheBoxSoftware.Reflection.Syntax
{
    using System.Collections.Generic;

    internal interface IEnumerationFormatter : IFormatter
    {
        /// <summary>
        /// Returns a formatted version of the enumeration visibility. Public, Private
        /// etc.
        /// </summary>
        /// <param name="syntax">The enumeration to format.</param>
        /// <returns>
        /// A string representing the visibility of the enumeration. For example C#
        /// implementation would return <c>public</c> for a public enumeration.
        /// </returns>
        List<SyntaxToken> FormatVisibility(EnumSyntax syntax);

        /// <summary>
        /// Returns the fully formatted enumeration.
        /// </summary>
        /// <param name="syntax">The enumeration details to format.</param>
        /// <returns>A formatted enumeration syntax decleration.</returns>
        /// <remarks>
        /// When implementing; this method will orchestrate the calling
        /// and formatting of the individual Format methods.
        /// </remarks>
        SyntaxTokenCollection Format(EnumSyntax syntax);
    }
}