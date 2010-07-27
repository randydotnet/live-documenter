﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TheBoxSoftware.DeveloperSuite.LiveDocumenter.Pages.Elements {
	/// <summary>
	/// Interaction logic for SummaryTable.xaml
	/// </summary>
	public partial class SummaryTable : Table {
		private TableRowGroup headerGroup;
		private TableRowGroup itemGroup;
		string firstHeader = "Name";
		string secondHeader = "Description";
		bool showIconColumn = true;
		private bool showSecondColumn = true;

		public SummaryTable() {
			InitializeComponent();
			this.Initilialise();
		}

		public SummaryTable(string firstColumnHeader, string secondColumnHeader, bool showIconColumn) {
			this.InitializeComponent();
			this.firstHeader = firstColumnHeader;
			this.secondHeader = secondColumnHeader;
			this.showIconColumn = false;
			this.Initilialise();
		}

		public SummaryTable(string firstColumnHeader, string secondColumnHeader, bool showIconColumn, bool showSecondColumn) {
			this.InitializeComponent();
			this.firstHeader = firstColumnHeader;
			this.secondHeader = secondColumnHeader;
			this.showIconColumn = showIconColumn;
			this.showSecondColumn = showSecondColumn;
			this.Initilialise();
		}

		private void Initilialise() {
			if (headerGroup == null) {
				headerGroup = new TableRowGroup();
				headerGroup.Style = (Style)this.FindResource("SummaryTableHeader");
				TableRow row = new TableRow();

				//
				if (this.showIconColumn) {
					TableColumn iconColumn = new TableColumn();
					iconColumn.Style = (Style)this.FindResource("SummaryTableIconColumn");
					this.Columns.Add(iconColumn);
				}
				TableColumn nameColumn = new TableColumn();
				nameColumn.Style = (Style)this.FindResource("SummaryTableColumn");
				this.Columns.Add(nameColumn);
				if (this.showSecondColumn) {
					TableColumn descriptionColumn = new TableColumn();
					descriptionColumn.Style = (Style)this.FindResource("SummaryTableColumn");
					this.Columns.Add(descriptionColumn);
				}

				if (this.showIconColumn) {
					TableCell icon = new TableCell(new Paragraph(new Run("")));
					row.Cells.Add(icon);
				}
				row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(firstHeader)))));
				if (this.showSecondColumn) {
					row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(secondHeader)))));
				}
				headerGroup.Rows.Add(row);

				this.RowGroups.Add(headerGroup);

				itemGroup = new TableRowGroup();
				this.RowGroups.Add(itemGroup);
			}
		}

		private BlockUIContainer BuildImage(string path) {
			Image n = new Image();
			n.Source = new BitmapImage(new Uri("pack://application:,,,/" + path, UriKind.Absolute));
			n.Width = 16;
			n.Height = 16;
			return new BlockUIContainer(n);
		}

		public void AddItem(string name, Block block) {
			this.AddItem(name, block, string.Empty);
		}
		public void AddItem(string name, Block block, string icon) {
			TableRow row = new TableRow();
			if (this.showIconColumn) {
				if (string.IsNullOrEmpty(icon)) {
					row.Cells.Add(new TableCell(new Paragraph(new Run(""))));
				}
				else {
					row.Cells.Add(new TableCell(this.BuildImage(icon)));
				}
			}
			row.Cells.Add(new TableCell(new Paragraph(new Run(name))));
			if (this.showSecondColumn) {
				if (block == null) {
					row.Cells.Add(new TableCell());
				}
				else {
					row.Cells.Add(new TableCell(block));
				}
			}
			itemGroup.Rows.Add(row);
		}

		public void AddItem(string name, string description) {
			this.AddItem(name, description, string.Empty);
		}
		public void AddItem(string name, string description, string icon) {
			TableRow row = new TableRow();
			if (this.showIconColumn) {
				if (string.IsNullOrEmpty(icon)) {
					row.Cells.Add(new TableCell(new Paragraph(new Run(""))));
				}
				else {
					row.Cells.Add(new TableCell(this.BuildImage(icon)));
				}
			}
			row.Cells.Add(new TableCell(new Paragraph(new Run(name))));
			if (this.showSecondColumn) {
				row.Cells.Add(new TableCell(new Paragraph(new Run(description))));
			}
			itemGroup.Rows.Add(row);
		}

		public void AddItem(Hyperlink name, Block description) {
			this.AddItem(name, description, string.Empty);
		}
		public void AddItem(Hyperlink name, Block description, string icon) {
			TableRow row = new TableRow();
			if (this.showIconColumn) {
				if (string.IsNullOrEmpty(icon)) {
					row.Cells.Add(new TableCell(new Paragraph(new Run(""))));
				}
				else {
					row.Cells.Add(new TableCell(this.BuildImage(icon)));
				}
			}
			row.Cells.Add(new TableCell(new Paragraph(name)));
			if (this.showSecondColumn) {
				if (description == null) {
					row.Cells.Add(new TableCell());
				}
				else {
					row.Cells.Add(new TableCell(description));
				}
			}
			itemGroup.Rows.Add(row);
		}

		public void AddItem(Hyperlink name, string description) {
			this.AddItem(name, description, string.Empty);
		}
		public void AddItem(Hyperlink name, string description, string icon) {
			TableRow row = new TableRow();
			if (this.showIconColumn) {
				row.Cells.Add(new TableCell(new Paragraph(new Run(""))));
			}
			row.Cells.Add(new TableCell(new Paragraph(name)));
			if (this.showSecondColumn) {
				row.Cells.Add(new TableCell(new Paragraph(new Run(description))));
			}
			itemGroup.Rows.Add(row);
		}
	}
}