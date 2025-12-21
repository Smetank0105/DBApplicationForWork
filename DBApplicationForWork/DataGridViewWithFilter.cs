using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace DBApplicationForWork
{
	public class ColumnFilterClickedEventArg : EventArgs
	{
		public int ColumnIndex { get; private set; }
		public Rectangle ButtonRectangle { get; private set; }
		public ColumnFilterClickedEventArg(int columnIndex, Rectangle btnRect)
		{
			this.ColumnIndex = columnIndex;
			this.ButtonRectangle = btnRect;
		}
	}
	public class DataGridFilterHeader : DataGridViewColumnHeaderCell
	{
		ComboBoxState currentState = ComboBoxState.Normal;
		Point cellLocation;
		Rectangle btnRect;

		public event EventHandler<ColumnFilterClickedEventArg> FilterButtonClicked;
		protected override void OnDataGridViewChanged()
		{
			try
			{
				Padding dropDownPadding = new Padding(0, 0, 20, 0);
				//this.Style.Padding = Padding.Add(this.InheritedStyle.Padding, dropDownPadding);
			}
			catch { }
			base.OnDataGridViewChanged();
		}
		protected override void Paint
			(
			Graphics graphics, 
			Rectangle clipBounds, 
			Rectangle cellBounds, 
			int rowIndex, 
			DataGridViewElementStates dataGridViewElementState, 
			object value, 
			object formattedValue, 
			string errorText, 
			DataGridViewCellStyle cellStyle, 
			DataGridViewAdvancedBorderStyle advancedBorderStyle, 
			DataGridViewPaintParts paintParts
			)
		{
			base.Paint
				(
				graphics, 
				clipBounds, 
				cellBounds, 
				rowIndex, 
				dataGridViewElementState, 
				value, 
				formattedValue, 
				errorText, 
				cellStyle, 
				advancedBorderStyle, 
				paintParts
				);
			int width = 20;
			btnRect = new Rectangle(cellBounds.X + cellBounds.Width - width, cellBounds.Y, width, cellBounds.Height );
			cellLocation = cellBounds.Location;
			ComboBoxRenderer.DrawDropDownButton(graphics, btnRect, currentState);
		}
		private bool IsMouseOverButton(Point e)
		{
			Point p = new Point(e.X + cellLocation.X, e.Y + cellLocation.Y);
			if(p.X >= btnRect.X && p.X <= btnRect.X + btnRect.Width && p.Y >= btnRect.Y && p.Y <= btnRect.Y + btnRect.Height )
			{
				return true;
			}
			return false;
		}
		protected virtual void OnFilterButtonClicked()
		{
			if (this.FilterButtonClicked != null)
				this.FilterButtonClicked(this, new ColumnFilterClickedEventArg(this.ColumnIndex, this.btnRect));
		}
		protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
		{
			if (this.IsMouseOverButton(e.Location))
				currentState = ComboBoxState.Pressed;
			base.OnMouseDown(e);
		}
		protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
		{
			if(this.IsMouseOverButton(e.Location))
			{
				currentState = ComboBoxState.Normal;
				this.OnFilterButtonClicked();
			}
			base.OnMouseUp(e);
		}
	}
	class FilterStatus
	{
		public string columnName {  get; set; }
		public string valueString { get; set; }
		public bool check {  get; set; }
	}
	public class DataGridViewWithFilter : DataGridView
	{
		List<FilterStatus> Filter = new List<FilterStatus>();
		TextBox textBox = new TextBox();
		CheckedListBox checkBox = new CheckedListBox();
		Button btnApply = new Button();
		Button btnClear = new Button();
		ToolStripDropDown popup = new ToolStripDropDown();

		string strFilter = "";
		string btnApplyTxt = "Apply";
		string btnClearTxt = "Clear filters";
		string checkAlltxt = "All";

		private int columnIndex {  get; set; }

		protected override bool DoubleBuffered { get => true; }
		protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
		{
			DataGridFilterHeader header = new DataGridFilterHeader();
			header.FilterButtonClicked += new EventHandler<ColumnFilterClickedEventArg>(header_FilterButtonClicked);
			e.Column.HeaderCell = header;
			base.OnColumnAdded(e);
		}

		void header_FilterButtonClicked(object sender, ColumnFilterClickedEventArg e)
		{
			int widthTool = GetWidthColumn(e.ColumnIndex) + 50;
			if (widthTool < 110) widthTool = 110;

			columnIndex = e.ColumnIndex;

			textBox.Clear();
			checkBox.Items.Clear();

			textBox.Size = new System.Drawing.Size(widthTool, 30);
			textBox.TextChanged -= textBox_TextChanged;
			textBox.TextChanged += textBox_TextChanged;

			checkBox.ItemCheck -= checkBox_ItemCheck;
			checkBox.ItemCheck += checkBox_ItemCheck;
			checkBox.CheckOnClick = true;
			GetChkFilter();

			checkBox.MaximumSize = new System.Drawing.Size(widthTool, GetHeightTable() - 120);
			checkBox.Size = new System.Drawing.Size(widthTool, (checkBox.Items.Count + 1) * 18);

			btnApply.Text = btnApplyTxt;
			btnApply.Size = new System.Drawing.Size(widthTool, 30);
			btnApply.Click -= btnApply_Click;
			btnApply.Click += btnApply_Click;

			btnClear.Text = btnClearTxt;
			btnClear.Size = new System.Drawing.Size(widthTool, 30);
			btnClear.Click -= btnClear_Click;
			btnClear.Click += btnClear_Click;

			popup.Items.Clear();
			popup.AutoSize = true;
			popup.Margin = Padding.Empty;
			popup.Padding = Padding.Empty;

			ToolStripControlHost host1 = new ToolStripControlHost(textBox);
			host1.Margin = Padding.Empty;
			host1.Padding = Padding.Empty;
			host1.AutoSize = false;
			host1.Size = textBox.Size;

			ToolStripControlHost host2 = new ToolStripControlHost(checkBox);
			host2.Margin = Padding.Empty;
			host2.Padding = Padding.Empty;
			host2.AutoSize = false;
			host2.Size = checkBox.Size;

			ToolStripControlHost host3 = new ToolStripControlHost(btnApply);
			host3.Margin = Padding.Empty;
			host3.Padding = Padding.Empty;
			host3.AutoSize = false;
			host3.Size = btnApply.Size;

			ToolStripControlHost host4 = new ToolStripControlHost(btnClear);
			host4.Margin = Padding.Empty;
			host4.Padding = Padding.Empty;
			host4.AutoSize = false;
			host4.Size = btnClear.Size;

			popup.Items.Add(host1);
			popup.Items.Add(host2);
			popup.Items.Add(host3);
			popup.Items.Add(host4);

			popup.Show(this, e.ButtonRectangle.X - GetWidthColumn(e.ColumnIndex), e.ButtonRectangle.Bottom);
			host1.Focus();
		}

		void checkBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if(e.Index == 0)
			{
				if(e.NewValue == CheckState.Checked)
				{
					for(int i = 1; i < checkBox.Items.Count; i++)
						checkBox.SetItemChecked(i, true);
				}
				else
				{
					for(int i = 1; i < checkBox.Items.Count; i++)
						checkBox.SetItemChecked(i, false);
				}
			}
		}
		void btnClear_Click(object sender, EventArgs e)
		{
			Filter.Clear();
			strFilter = "";
			ApplyFilter();
			popup.Close();
		}
		void textBox_TextChanged(object sender, EventArgs e)
		{
			GetChkFilter();
		}
		void btnApply_Click(object sender, EventArgs e)
		{
			strFilter = "";
			SaveChkFilter();
			ApplyFilter();
			popup.Close();
		}
		private List<string> GetDataColumns(int e)
		{
			List<string> ValueCellList = new List<string>();
			string Value;

			foreach (DataGridViewRow row in this.Rows)
			{
				Value = row.Cells[e].Value.ToString();
				if (!ValueCellList.Contains(Value))
					ValueCellList.Add(row.Cells[e].Value.ToString());
			}
			return ValueCellList;
		}
		private int GetHeightTable()
		{
			return this.Height;
		}
		private int GetWidthColumn(int e)
		{
			return this.Columns[e].Width;
		}
		private void SaveChkFilter()
		{
			string col = this.Columns[columnIndex].Name;
			string itemChk;
			bool statChk;

			Filter.RemoveAll(x => x.columnName == col);

			for(int i = 1; i <checkBox.Items.Count; i++)
			{
				itemChk = checkBox.Items[i].ToString();
				statChk = checkBox.GetItemChecked(i);
				Filter.Add(new FilterStatus() { columnName = col, valueString = itemChk, check = statChk });
			}
		}
		private void GetChkFilter()
		{
			List<FilterStatus> ChkList = new List<FilterStatus>();
			List<FilterStatus> ChkListSort = new List<FilterStatus>();
			
			checkBox.Items.Clear();

			foreach(FilterStatus i in Filter)
			{
				if (this.Columns[columnIndex].Name == i.columnName && i.valueString.IndexOf(textBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
					ChkList.Add(new FilterStatus() { columnName = "", valueString = i.valueString, check = i.check });
			}
			foreach(string ValueCell in GetDataColumns(columnIndex))
			{
				if (ValueCell.IndexOf(textBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					int index = ChkList.FindIndex(item => item.valueString == ValueCell);
					if (index == -1)
						ChkList.Add(new FilterStatus { valueString = ValueCell, check = true });
				}
			}
			checkBox.Items.Add(checkAlltxt, CheckState.Indeterminate);

			switch (this.Columns[columnIndex].ValueType.ToString())
			{
				case "System.Int32":
					ChkListSort = ChkList.OrderBy(x => Int32.Parse(x.valueString)).ToList();
					foreach(FilterStatus i in ChkListSort)
					{
						if(i.check == true)
							checkBox.Items.Add(i.valueString, CheckState.Checked);
						else
							checkBox.Items.Add(i.valueString, CheckState.Unchecked);
					}
					break;
				case "System.DateTime":
					ChkListSort = ChkList.OrderBy(x=> DateTime.Parse(x.valueString)).ToList();
					foreach(FilterStatus i in ChkListSort)
					{
						if (i.check == true)
							checkBox.Items.Add(i.valueString, CheckState.Checked);
						else
							checkBox.Items.Add(i.valueString, CheckState.Unchecked);
					}
					break;
				default:
					ChkListSort = ChkList.OrderBy(x=>x.valueString).ToList();
					foreach(FilterStatus i in ChkListSort)
					{
						if (i.check == true)
							checkBox.Items.Add(i.valueString, CheckState.Checked);
						else
							checkBox.Items.Add(i.valueString, CheckState.Unchecked);
					}
					break;
			}
		}
		private void ApplyFilter()
		{
			foreach(FilterStatus i in Filter)
			{
				if(i.check == false)
				{
					if (strFilter.Length == 0)
						strFilter = strFilter + ("[" + i.columnName + "] <> '" + i.valueString + "' ");
					else
						strFilter = strFilter + (" AND [" + i.columnName + "] <> '" + i.valueString + "' ");
				}
			}
			(this.DataSource as DataTable).DefaultView.RowFilter = strFilter;
		}
	}
}
