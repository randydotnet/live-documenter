﻿
namespace TheBoxSoftware.Reflection.Signatures
{
    using System.Text;
    using TheBoxSoftware.Diagnostics;
    using TheBoxSoftware.Reflection.Core;

    /// <summary>
    /// Represents a signiture for a type specification as detailed in
    /// section 23.2.14 in ECMA 335.
    /// </summary>
    internal sealed class TypeSpecificationSignature : Signature
    {
        /// <summary>
        /// Instantiates a new instance of the TypeSpecificationSigniture class.
        /// </summary>
        /// <param name="signiture">The actual signiture contents.</param>
        public TypeSpecificationSignature(byte[] signiture)
            : base(Signatures.TypeSpecification)
        {

            TypeToken = new TypeSignatureToken(signiture, 0);
        }

        /// <summary>
        /// Obtains the details of the type.
        /// </summary>
        /// <param name="member">The member to resolve against.</param>
        /// <returns>The details of the type having the specification.</returns>
        public TypeDetails GetTypeDetails(ReflectedMember member)
        {
            return TypeToken.GetTypeDetails(member);
        }

#if TEST
        /// <summary>
        /// Prints tokens to the current TRACE output
        /// </summary>
        public void PrintTokens()
        {
            StringBuilder sb = new StringBuilder();

            foreach (SignitureToken token in this.Type.Tokens)
            {
                switch (token.TokenType)
                {
                    case SignitureTokens.ArrayShape: // non-nested token
                    case SignitureTokens.CallingConvention:
                    case SignitureTokens.Constraint:
                    case SignitureTokens.ElementType:
                    case SignitureTokens.GenericArgumentCount:
                    case SignitureTokens.Count:
                    case SignitureTokens.CustomModifier:
                    case SignitureTokens.GenericParameterCount:
                    case SignitureTokens.Type:
                    case SignitureTokens.TypeDefOrRefEncodedToken:
                    case SignitureTokens.Field:
                    case SignitureTokens.LocalSigniture:
                    case SignitureTokens.Param:
                    case SignitureTokens.ParameterCount:
                    case SignitureTokens.Prolog:
                    case SignitureTokens.Property:
                    case SignitureTokens.ReturnType:
                    case SignitureTokens.Sentinal:                    
                    default:
                        sb.Append(token.ToString()); 
                        break;
                }
            }

            TraceHelper.WriteLine(sb.ToString());
        }
#endif
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[TypeSpec: ");

            foreach(SignatureToken t in Tokens)
            {
                sb.Append(t.ToString());
            }

            sb.Append("] ");

            return sb.ToString();
        }

        public TypeSignatureToken TypeToken { get; set; }
    }
}