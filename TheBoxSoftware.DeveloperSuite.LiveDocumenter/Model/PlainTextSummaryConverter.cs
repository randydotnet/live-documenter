﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheBoxSoftware.DeveloperSuite.LiveDocumenter.Model {
	using TheBoxSoftware.Reflection;
	using TheBoxSoftware.Reflection.Comments;

	/// <summary>
	/// Converts the summary code comments for the specified member in to
	/// a plain string.
	/// </summary>
	/// <seealso cref="TheBoxSoftware.Reflection.Comments.XmlCodeCommentFile" />
	internal sealed class PlainTextSummaryConverter {
        /// <summary>
        /// Converts the 
        /// </summary>
        /// <param name="assembly">The assembly associated with the member being documented.</param>
        /// <param name="file">The xml comment file to read the member comments.</param>
        /// <param name="crefPathToMember">The CRef path to the Member.</param>
        /// <returns>A string containing the documentation.</returns>
		public static string Convert(AssemblyDef assembly, XmlCodeCommentFile file, CRefPath crefPathToMember) {
			StringBuilder text = new StringBuilder();
			XmlCodeComment comment = file.ReadComment(crefPathToMember);

			if (comment != XmlCodeComment.Empty) {
				SummaryXmlCodeElement summary = (SummaryXmlCodeElement)comment.Elements.Find(o => o is SummaryXmlCodeElement);
				foreach (XmlCodeElement current in summary.Elements) {
					PlainTextSummaryConverter.ConvertElement(assembly, current, text);
				}
			}

			return text.ToString();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="element"></param>
        /// <param name="text"></param>
		private static void ConvertElement(AssemblyDef assembly, XmlCodeElement element, StringBuilder text) {

			switch (element.Element) {
				case XmlCodeElements.Text:
					text.Append(element.Text);
					break;

				default:
					break;
			}
		}
	}
}