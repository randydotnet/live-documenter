﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Saxon.Api;
using System.IO;

namespace TheBoxSoftware.Documentation.Exporting {
	using TheBoxSoftware.Reflection;
	using TheBoxSoftware.Reflection.Comments;
	using TheBoxSoftware.Documentation.Exporting.Website;

	public class WebsiteExporter : Exporter {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebsiteExporter"/> class.
		/// </summary>
		/// <param name="document">The document to be exported.</param>
		/// <param name="config">The export configuration.</param>
		public WebsiteExporter(Document document, ExportSettings settings, ExportConfigFile config) 
			: base(document, settings, config) {
		}

		/// <summary>
		/// Exports the full contained documentation.
		/// </summary>
		public override void Export() {
			// we do not need the temp staging folder with this export so write direct from temp to publish.
			try {
				this.PrepareForExport();
				
				// calculate the export steps
				int numberOfSteps = 0;
				numberOfSteps += 1; // toc and index steps
				numberOfSteps += this.Document.Map.Count; // top level entries for recursive export
				numberOfSteps += 1; // output files
				numberOfSteps += ((this.Document.Map.NumberOfEntries / this.XmlExportStep) * 3); // xml export stage
				numberOfSteps += 1; // cleanup files
				
				this.OnExportCalculated(new ExportCalculatedEventArgs(numberOfSteps));
				this.CurrentExportStep = 1;

				// export the document map
				if (!this.IsCancelled) {
					this.OnExportStep(new ExportStepEventArgs("Export as XML...", ++this.CurrentExportStep));
					using (XmlWriter writer = XmlWriter.Create(string.Format("{0}/toc.xml", this.TempDirectory))) {
						Rendering.DocumentMapXmlRenderer map = new Rendering.DocumentMapXmlRenderer(this.Document.Map);
						map.Render(writer);
					}

					// export the index page
					IndexXmlRenderer indexPage = new IndexXmlRenderer(this.Document.Map);
					using (XmlWriter writer = XmlWriter.Create(string.Format("{0}/index.xml", this.TempDirectory))) {
						indexPage.Render(writer);
					}

					// export each of the members
					foreach (Entry current in this.Document.Map) {
						this.RecursiveEntryExport(current);
						this.OnExportStep(new ExportStepEventArgs("Export as XML...", ++this.CurrentExportStep));
						if (this.IsCancelled) break;
					}
					GC.Collect();
				}

				if (!this.IsCancelled) {
					Processor p = new Processor();
					XsltTransformer transform = p.NewXsltCompiler().Compile(this.Config.GetXslt()).Load();
					transform.SetParameter(new QName(new XmlQualifiedName("directory")), new XdmAtomicValue(System.IO.Path.GetFullPath(this.TempDirectory)));

					// set output files
					this.OnExportStep(new ExportStepEventArgs("Saving output files...", ++this.CurrentExportStep));
					this.Config.SaveOutputFilesTo(this.PublishDirectory);

					this.OnExportStep(new ExportStepEventArgs("Transforming XML...", ++this.CurrentExportStep));
					string extension = this.Config.Properties.ContainsKey("extension") ? this.Config.Properties["extension"] : "htm";
					int counter = 0;
					foreach (string current in Directory.GetFiles(this.TempDirectory)) {
						if (current.Substring(this.TempDirectory.Length) == "toc.xml")
							continue;
						using (FileStream fs = File.OpenRead(current)) {
							Serializer s = new Serializer();
							s.SetOutputFile(this.PublishDirectory + Path.GetFileNameWithoutExtension(current) + "." + extension);
							transform.SetInputStream(fs, new Uri(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location), this.TempDirectory));
							transform.Run(s);
							counter++;
							if (counter % this.XmlExportStep == 0) {
								this.OnExportStep(new ExportStepEventArgs("Transforming XML...", this.CurrentExportStep += 3));
							}
						}
						if (this.IsCancelled) break;
					}
				}

				// clean up the temp directory
				this.OnExportStep(new ExportStepEventArgs("Cleaning up", ++this.CurrentExportStep));
				this.Cleanup();
			}
			catch (Exception ex) {
				this.Cleanup(); // attempt to clean up our mess before dying
				ExportException exception = new ExportException(ex.Message, ex);
				this.OnExportException(new ExportExceptionEventArgs(exception));
			}
		}
	}
}
