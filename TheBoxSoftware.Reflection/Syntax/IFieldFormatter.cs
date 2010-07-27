﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheBoxSoftware.Reflection.Syntax {
	public interface IFieldFormatter : IFormatter {
		SyntaxToken GetType(FieldSyntax syntax);
		List<SyntaxToken> GetVisibility(FieldSyntax syntax);
		List<SyntaxToken> Format(FieldSyntax syntax);
	}
}