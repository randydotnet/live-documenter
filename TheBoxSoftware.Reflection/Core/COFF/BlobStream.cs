﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheBoxSoftware.Reflection.Core.COFF {
	/// <summary>
	/// The blob stream controls access to signitures contained in the pe/coff file.
	/// </summary>
	public sealed class BlobStream : Stream {
		private byte[] streamContents;
		/// <summary>
		/// Reference to file which owns the stream, this is stored to make it available to
		/// the signitures.
		/// </summary>
		private PeCoffFile owningFile;

		/// <summary>
		/// Initialises a new instance of the BlobStream class
		/// </summary>
		/// <param name="file">The file the stream should be read from</param>
		/// <param name="address">The start address of the blob stream</param>
		/// <param name="size">The size of the stream</param>
		internal BlobStream(PeCoffFile file, int address, int size) {
			this.owningFile = file;
			// Read and store the underlying data for this stream
			this.streamContents = new byte[size];
			int endAddress = address + size;
			byte[] fileContents = file.FileContents;
			for (int i = address; i < endAddress; i++) {
				this.streamContents[i - address] = fileContents[i];
			}
		}

		/// <summary>
		/// Retrives a parsed <see cref="Reflection.Signitures.Signiture"/> for the specified
		/// <paramref name="startOffset"/> and <paramref name="signiture"/> type.
		/// </summary>
		/// <param name="startOffset">The start of the signiture in the stream.</param>
		/// <param name="signiture">The type of signiture to parse.</param>
		/// <returns>The parsed signiture.</returns>
		public Reflection.Signitures.Signiture GetSigniture(int startOffset, Reflection.Signitures.Signitures signiture) {
			return Reflection.Signitures.Signiture.Create(
				this.streamContents, 
				startOffset, 
				this.owningFile, 
				signiture);
		}

		/// <summary>
		/// Obtains the contents of the signiture as a byte array.
		/// </summary>
		/// <param name="startOffset">The start offset of the signiture in the stream.</param>
		/// <returns>The contents of the signiture as a byte array.</returns>
		public byte[] GetSignitureContents(int startOffset) {
			byte length = this.streamContents[startOffset++];
			byte[] signitureContents = new byte[length];
			for (int i = startOffset; i < startOffset + length; i++) {
				signitureContents[i - startOffset] = this.streamContents[i];
			}
			return signitureContents;
		}
	}
}
