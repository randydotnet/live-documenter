﻿
/*
 * Signiture defined in: 23.2.9
 * 
 * Can only contain one value ELEMENT_TYPE_PINNED
 */

namespace TheBoxSoftware.Reflection.Signitures
{
    using System.Diagnostics;
    using Core;

    /// <summary>
    /// Represents a constraint in a signiture as a token. Constraints can only be
    /// <see cref="ElementTypes.Pinned"/>.
    /// </summary>
	[DebuggerDisplay("Constraint: {Constraint}")]
    internal sealed class ConstraintSignitureToken : SignitureToken
    {
        private ElementTypes _constraint;

        /// <summary>
        /// Initialises a new instance of the ConstraintSignitureToken from the <paramref name="signiture"/>
        /// at the specified <paramref name="offset"/>.
        /// </summary>
        /// <param name="signiture">The signiture to load from.</param>
        /// <param name="offset">The offset in the signiture.</param>
		public ConstraintSignitureToken(byte[] signiture, Offset offset)
            : base(SignitureTokens.Constraint)
        {
            this.Constraint = (ElementTypes)SignitureToken.GetCompressedValue(signiture, offset);
        }

        /// <summary>
        /// Checks if the entry at <paramref name="offset"/> in the <paramref name="signiture"/>
        /// heap is a token.
        /// </summary>
        /// <param name="signiture">The signiture to check.</param>
        /// <param name="offset">The offset in the signiture.</param>
        /// <returns>True if it is a token else false.</returns>
		public static bool IsToken(byte[] signiture, int offset)
        {
            ElementTypes type = (ElementTypes)SignitureToken.GetCompressedValue(signiture, offset);
            return (type & ElementTypes.Pinned) == ElementTypes.Pinned;
        }

        /// <summary>
        /// Produces a string representation of the constraint token [Constraint: {ElementType}]
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return string.Format("[Constraint: {0}]", this.Constraint.ToString());
        }

        /// <summary>
        /// The constraint this token represents.
        /// </summary>
		public ElementTypes Constraint
        {
            get { return this._constraint; }
            private set { this._constraint = value; }
        }
    }
}