﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheBoxSoftware.DeveloperSuite.PEViewer.Model.MetadataWrappers {
	using TheBoxSoftware.Reflection.Core.COFF;
	using TheBoxSoftware.Reflection.Core;

	internal class ParamMetadataWrapper {
		public ParamMetadataWrapper(MetadataStream file, List<MetadataRow> methods) {
			this.Items = new List<ParamEntry>();
			int counter = 1;
			foreach (ParamMetadataTableRow current in methods) {
				this.Items.Add(new ParamEntry(counter++, file.OwningFile.GetMetadataDirectory(), current));
			}
		}
		public List<ParamEntry> Items { get; set; }
		public class ParamEntry {
			public ParamEntry(int index, MetadataDirectory directory, ParamMetadataTableRow row) {
				this.Index = index;
				this.FileOffset = string.Format("0x{0:x}", row.FileOffset);
				this.Name = ((StringStream)directory.Streams[Streams.StringStream]).GetString(row.Name.Value);
				this.Sequence = row.Sequence.ToString();
				this.Flags = string.Format("0x{0:x}", row.Flags);
			}

			public int Index { get; set; }
			public string FileOffset { get; set; }
			public string Name { get; set; }
			public string Sequence { get; set; }
			public string Flags { get; set; }
		}
	}
}
