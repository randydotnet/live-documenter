﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheBoxSoftware.Reflection.Syntax {
	/// <summary>
	/// Class that interogates a member for the details required
	/// to declate a constant.
	/// </summary>
	public class ConstantSyntax : Syntax {
		private FieldDef field;

		/// <summary>
		/// Initialises a new instance of the ConstantSyntax class.
		/// </summary>
		/// <param name="field">The field to obtain the syntax details for.</param>
		public ConstantSyntax(FieldDef field) {
			this.field = field;
		}

		public string[] GetAttributes() {
			return new string[0];
		}

		/// <summary>
		/// Obtains the visibility of the constant value.
		/// </summary>
		/// <returns>A visibility enumerated value.</returns>
		public Visibility GetVisibility() {
			return this.field.MemberAccess;
		}

		/// <summary>
		/// Obtains the name of type which this field is.
		/// </summary>
		/// <returns></returns>
		public TypeRef GetType() {
			TypeRef returnType = null;

			Signitures.Signiture signiture = this.field.Signiture;
			Signitures.SignitureToken token = signiture.Tokens.Find(
				t => t.TokenType == TheBoxSoftware.Reflection.Signitures.SignitureTokens.ElementType ||
					t.TokenType == TheBoxSoftware.Reflection.Signitures.SignitureTokens.Type
				);
			if (token != null) {
				TypeRef type = (token is Signitures.TypeSignitureToken)
					? ((Signitures.TypeSignitureToken)token).ResolveType(this.field.Assembly, null)
					: ((Signitures.ElementTypeSignitureToken)token).ResolveToken(this.field.Assembly);
				if (type != null) {
					returnType = type;
				}
			}

			return returnType;
		}

		public string GetIdentifier() {
			return this.field.Name;
		}
	}
}
