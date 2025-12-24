using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;

namespace DBApplicationForWork
{
	public partial class MainForm : Form
	{
		Connector connector;

		const string connectionString = "Data Source=SMETANK\\SQLEXPRESS;Initial Catalog=BOX_3;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
		string[] panel_tp_names = new string[] { "Главная", "Отображение"};
		string[] table_names = new string[] { "CartridgeRecords", "PrinterRecords", "ComputerRecords" };
		string[] database_tp_names = new string[] { "Картриджи", "Принтеры", "Компьютеры" };
		string[] dataGrid_names = new string[] { "dgCartridges", "dgPrinters", "dgComputers" };
		string[] panel_btn_names = new string[] { "Новый наряд", "Редактировать", "Изменить статус", "Печать", "Обновить", "Удалить"};
		string[] state_names = new string[] { "", "на исполнении", "на отправку в фирму", "в фирме", "готов", "выдан", "ждёт запчастей"};
		Color[] state_colors = new Color[] { Color.Black, Color.Yellow, Color.SteelBlue, Color.DeepSkyBlue, Color.ForestGreen, Color.Purple, Color.Coral};
		string[] field_names = new string[] 
		{
			"ID",
			"номер наряда",
			"дата поступления",
			"номер заявки",
			"подразделение",
			"наименование",
			"инвенратный номер",
			"заметки",
			"дата передачи в фирму",
			"номер акта фирмы",
			"дата готовности",
			"дата выдачи",
			"статус"
		};
		const string font_name = "Arial";
		const int font_size = 12;

		public MainForm()
		{
			InitializeComponent();
			this.Load += new System.EventHandler(this.MainForm_Load);
		}

//Controls

		void initComponents()
		{
			this.Size = new Size(1000, 700);

//Create TabControl "tcPanel" with TabPages "tpMain" and "tpView"
			TabControl tcPanel = new TabControl();
			tcPanel.Name = "tcPanel";
			tcPanel.Dock = DockStyle.Top;
			tcPanel.Height = 120;
			tcPanel.SizeMode = TabSizeMode.Fixed;
			tcPanel.ItemSize = new Size(300, 20);
			
			TabPage tpPanelMain = new TabPage(panel_tp_names[0]);
			TabPage tpPanelView = new TabPage(panel_tp_names[1]);
			tcPanel.TabPages.Add(tpPanelMain);
			tcPanel.TabPages.Add(tpPanelView);

			this.Controls.Add(tcPanel);

//Create LayoutPanel for "tpPanelMain" and "tpPanelView"
			FlowLayoutPanel flpMain = new FlowLayoutPanel();
			flpMain.Dock = DockStyle.Fill;
			flpMain.AutoScroll = true;
			flpMain.WrapContents = false;
			flpMain.FlowDirection = FlowDirection.LeftToRight;
			flpMain.Padding = new Padding(20);
			tpPanelMain.Controls.Add(flpMain);

			TableLayoutPanel tlpView = new TableLayoutPanel();
			tlpView.Dock = DockStyle.Fill;
			tlpView.AutoScroll = true;
			tlpView.Padding = new Padding(10);
			tlpView.RowCount = 2;
			tlpView.ColumnCount = field_names.Length - 1;
			tlpView.AutoSize = true;
			tpPanelView.Controls.Add(tlpView);

//Create TabControl "tcDataBase" with TabPages "tpCartridges", "tpPrinters" and "tpComputers"
			TabControl tcDataBase = new TabControl();
			tcDataBase.Name = "tcDataBase";
			tcDataBase.Bounds = new Rectangle(0, 120, 985, 540);
			tcDataBase.Anchor = (AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right) | AnchorStyles.Bottom);
			tcDataBase.SizeMode = TabSizeMode.Fixed;
			tcDataBase.ItemSize = new Size (200, 30);
			tcDataBase.Alignment = TabAlignment.Right;
			tcDataBase.SelectedIndexChanged += new EventHandler(tcDataBase_SelectedIndexChanged);

			TabPage tpCartridges = new TabPage(database_tp_names[0]);
			TabPage tpPrinters = new TabPage(database_tp_names[1]);
			TabPage tpComputers = new TabPage(database_tp_names[2]);
			tcDataBase.TabPages.Add(tpCartridges);
			tcDataBase.TabPages.Add(tpPrinters);
			tcDataBase.TabPages.Add(tpComputers);

			this.Controls.Add(tcDataBase);

//Create Buttons for "tcPanel->tpMain"
			addMainButtons(flpMain);

//Create DataGridViewWithFilter for all TabPages in "tcDataBase"
			addDataGridViewWithFilters(tcDataBase);

//Create TextBox and CheckBox for "tcPnale->tpView"
			addViewGroupBoxForDataGridColumnWidth(tlpView);
			addViewCheckBoxes(tlpView);
		}
		void addMainButtons(FlowLayoutPanel flp)
		{
			int btnWidth = 150;
			int btnHeight = 50;

			Button[] buttonsMain = new Button[]
			{
				new Button {Text = panel_btn_names[0], Name = "btnNewOrder", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[1], Name = "btnEditFields", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[2], Name = "btnChangeStates", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[3], Name = "btnPrint", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[4], Name = "btnRefresh", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[5], Name = "btnDelete", Font = new Font(font_name, font_size)}
			};
			for (int i = 0; i < buttonsMain.Length; i++)
			{
				if(i == 5)
				{
					Panel spacer = new Panel();
					spacer.Size = new Size(Screen.PrimaryScreen.Bounds.Width - btnWidth * 7, btnHeight);
					spacer.Visible = true;
					flp.Controls.Add(spacer);
				}
				buttonsMain[i].Size = new Size(btnWidth, btnHeight);
				flp.Controls.Add(buttonsMain[i]);
			}
			buttonsMain[0].Click += new EventHandler(btnMainNewOrder_Click);
			buttonsMain[1].Click += new EventHandler(btnMainEditFileds_Click);
			buttonsMain[2].Click += new EventHandler(btnMainChangeStates_Click);
			buttonsMain[3].Click += new EventHandler(btnMainPrint_Click);
			buttonsMain[4].Click += new EventHandler(btnMainRefresh_Click);
			buttonsMain[5].Click += new EventHandler(btnMainDelete_Click);
		}
		void addDataGridViewWithFilters(TabControl tc)
		{
			DataGridViewWithFilter[] dGrids = new DataGridViewWithFilter[]
			{
				new DataGridViewWithFilter {Name = dataGrid_names[0]},
				new DataGridViewWithFilter {Name = dataGrid_names[1]},
				new DataGridViewWithFilter {Name = dataGrid_names[2]}
			};
			for (int i = 0; i < dGrids.Length; i++)
			{
				tc.TabPages[i].Controls.Add(dGrids[i]);
				dGrids[i].Dock = DockStyle.Fill;
				dGrids[i].AllowUserToAddRows = false;
				dGrids[i].AllowUserToDeleteRows = false;
				dGrids[i].ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
				dGrids[i].RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
				dGrids[i].AllowUserToResizeColumns = false;
				dGrids[i].AllowUserToResizeRows = false;
				dGrids[i].ReadOnly = true;
				dGrids[i].SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dGrids[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
				dGrids[i].CellContextMenuStripNeeded += new DataGridViewCellContextMenuStripNeededEventHandler(dataGridView_CellContextMenuNeeded);
				dGrids[i].CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView_CellFormating);
				dGrids[i].DataSource = connector.SelectRecords(table_names[i]);
				for (int j = 0; j < dGrids[i].ColumnCount; j++)
					dGrids[i].Columns[j].SortMode = DataGridViewColumnSortMode.NotSortable;
				if (dGrids[i].Columns.Count > 0)dGrids[i].Columns[0].Visible = false;
				if (dGrids[i].Rows.Count > 0)
					dGrids[i].FirstDisplayedScrollingRowIndex = dGrids[i].Rows.Count - 1;
			}
		}
		void addViewCheckBoxes(TableLayoutPanel tlp)
		{
			int cbWidth = 150;
			int cbHeight = 20;

			CheckBox[] cbViews = new CheckBox[]
			{
				new CheckBox {Text = field_names[1], Name = "cbView_1"},
				new CheckBox {Text = field_names[2], Name = "cbView_2"},
				new CheckBox {Text = field_names[3], Name = "cbView_3"},
				new CheckBox {Text = field_names[4], Name = "cbView_4"},
				new CheckBox {Text = field_names[5], Name = "cbView_5"},
				new CheckBox {Text = field_names[6], Name = "cbView_6"},
				new CheckBox {Text = field_names[7], Name = "cbView_7"},
				new CheckBox {Text = field_names[8], Name = "cbView_8"},
				new CheckBox {Text = field_names[9], Name = "cbView_9"},
				new CheckBox {Text = field_names[10], Name = "cbView_10"},
				new CheckBox {Text = field_names[11], Name = "cbView_11"},
				new CheckBox {Text = field_names[12], Name = "cbView_12"},
			};
			for (int i = 0;  i < cbViews.Length; i++)
			{
				cbViews[i].Size = new Size(cbWidth, cbHeight);
				cbViews[i].TextAlign = ContentAlignment.TopLeft;
				cbViews[i].CheckState = CheckState.Checked;
				cbViews[i].CheckedChanged += new EventHandler(viewCheckBoxs_CheckChanged);
				tlp.Controls.Add(cbViews[i], i, 0);
			}
		}
		void addViewGroupBoxForDataGridColumnWidth(TableLayoutPanel tlp)
		{
			GroupBox gb = new GroupBox();
			gb.Text = "Ширина столбцов";
			gb.Size = new Size(320, 50);
			FlowLayoutPanel flp = new FlowLayoutPanel();
			flp.Dock = DockStyle.Fill;
			flp.FlowDirection = FlowDirection.LeftToRight;
			RadioButton rb1 = new RadioButton();
			rb1.Name = "rb_fill";
			rb1.Size = new Size(150, 20);
			rb1.Text = "Заполнение";
			rb1.CheckedChanged += new EventHandler(viewRadioButton_CkeckChanged);
			RadioButton rb2 = new RadioButton();
			rb2.Name = "rb_cell";
			rb2.Size = new Size(150, 20);
			rb2.Text = "По ячейкам";
			rb2.CheckedChanged += new EventHandler(viewRadioButton_CkeckChanged);
			gb.Controls.Add(flp);
			flp.Controls.Add(rb1);
			flp.Controls.Add(rb2);
			tlp.Controls.Add(gb, 0, 1);
			tlp.SetColumnSpan(gb, 2);
			rb1.Checked = true;
		}

//ContextMenuStrip

		ContextMenuStrip showMainEditFieldsMenuStrip()
		{
			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Name = "cmsMainEF";
			cms.Font = new Font(font_name, font_size);
			cms.ShowImageMargin = false;
			cms.ShowCheckMargin = false;
			for (int i = 1; i < field_names.Length - 1; i++)
			{
				ToolStripMenuItem item = new ToolStripMenuItem($"Редактировать поле \"{field_names[i]}\"");
				item.Name = $"tsmiMainEF_{i}";
				item.Padding = new Padding(0, 2, 0, 2);
				item.Click += new EventHandler(tsmiEditFields_Click);
				cms.Items.Add(item);
			}
			return cms;
		}
		ContextMenuStrip showMainChangeStatesMenuStrip()
		{
			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Name = "cmsMainCS";
			cms.Font = new Font(font_name, font_size);
			cms.ShowImageMargin = false;
			cms.ShowCheckMargin = false;
			for(int i = 1; i < state_names.Length;  i++)
			{
				ToolStripMenuItem item = new ToolStripMenuItem(state_names[i]);
				item.Name = $"tsmiMainCS_{i}";
				item.BackColor = state_colors[i];
				item.Padding = new Padding(0, 2, 0, 2);
				item.Click += new EventHandler(tsmiChangeStates_Click);
				cms.Items.Add(item);
			}
			return cms;
		}
		ContextMenuStrip showMainPrintMenuStrip()
		{
			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Name = "cmsMainP";
			cms.Font = new Font(font_name, font_size);
			cms.ShowImageMargin = false;
			cms.ShowCheckMargin = false;
			ToolStripMenuItem tsmiMainP1 = new ToolStripMenuItem("Распечатать наряд");
			tsmiMainP1.Name = "tsmiMainP_order";
			tsmiMainP1.Padding = new Padding(0, 2, 0, 2);
			tsmiMainP1.Click += new EventHandler(tsmiPrint_Click);
			cms.Items.Add(tsmiMainP1);
			ToolStripMenuItem tsmiMainP2 = new ToolStripMenuItem("Распечатать акт фирмы");
			tsmiMainP2.Name = "tsmiMainP_act";
			tsmiMainP2.Padding = new Padding(0, 2, 0, 2);
			tsmiMainP2.Click += new EventHandler(tsmiPrint_Click);
			cms.Items.Add(tsmiMainP2);
			return cms;
		}
		ContextMenuStrip showDataBaseMenuStrip()
		{
			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Name = "cmsDataBase";
			cms.Font = new Font(font_name, font_size);
			cms.ShowImageMargin = false;
			cms.ShowCheckMargin = false;
			ToolStripMenuItem editMenu = new ToolStripMenuItem(panel_btn_names[1]);
			for(int i = 1; i < field_names.Length - 1; i++)
			{
				ToolStripMenuItem item = new ToolStripMenuItem(field_names[i]);
				item.Name = $"tsmiEditMenu_{i}";
				item.Click += new EventHandler(tsmiEditFields_Click);
				editMenu.DropDownItems.Add(item);
			}
			ToolStripMenuItem stateMenu = new ToolStripMenuItem(panel_btn_names[2]);
			for (int i = 1; i < state_names.Length; i++)
			{
				ToolStripMenuItem item = new ToolStripMenuItem(state_names[i]);
				item.Name = $"tsmiStateMenu_{i}";
				item.BackColor = state_colors[i];
				item.Click += new EventHandler(tsmiChangeStates_Click);
				stateMenu.DropDownItems.Add(item);
			}
			cms.Items.Add(editMenu);
			cms.Items.Add(stateMenu);
			ToolStripMenuItem refreshMenu = new ToolStripMenuItem(panel_btn_names[4]);
			refreshMenu.Click += new EventHandler(btnMainRefresh_Click);
			cms.Items.Add(refreshMenu);
			foreach (ToolStripItem item in cms.Items)
				item.Padding = new Padding(0, 2, 0, 2);
			return cms;
		}

//Events

		void MainForm_Load(object sender, EventArgs e)
		{
			connector = new Connector(connectionString);
			initComponents();
		}
		void btnMainNewOrder_Click(object sender, EventArgs e)
		{
			TabControl tc = this.Controls.Find("tcDataBase", true).FirstOrDefault() as TabControl;
			if (tc != null)
			{
				InsertRecordsForm form = new InsertRecordsForm(tc.SelectedIndex);
				if (form.ShowDialog() == DialogResult.OK)
					loadDataGridView(); 
			}
		}
		void btnMainEditFileds_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cms = showMainEditFieldsMenuStrip();
			Button btn = (sender as Button);
			cms.Show(btn, new Point(0, btn.Height));
		}
		void btnMainChangeStates_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cms = showMainChangeStatesMenuStrip();
			Button btn = (sender as Button);
			cms.Show(btn, new Point(0, btn.Height));
		}
		void btnMainPrint_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cms = showMainPrintMenuStrip();
			Button btn = (sender as Button);
			cms.Show(btn, new Point(0, btn.Height));
		}
		void btnMainRefresh_Click(object sender, EventArgs e)
		{
			loadDataGridView();
		}
		void btnMainDelete_Click(object sender, EventArgs e)
		{
			List<int> ids = new List<int>();
			TabControl tc = this.Controls.Find("tcDataBase", true).FirstOrDefault() as TabControl;
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tc.SelectedIndex], true).FirstOrDefault() as DataGridViewWithFilter;
			foreach (DataGridViewRow row in dgv.SelectedRows)
				ids.Add(Convert.ToInt32(row.Cells[0].Value));
			if (MessageBox.Show("Вы уверены, что хотите удалить эти записи?", "Внимание!", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
			{
				connector.DeleteCartridgeRecords(tc.SelectedIndex, ids);
				loadDataGridView();
			}
		}
		void dataGridView_CellContextMenuNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
		{
			if (e.RowIndex != -1 && e.ColumnIndex != -1)
			{
				if(!(sender as DataGridViewWithFilter).Rows[e.RowIndex].Selected)
				{
					(sender as DataGridViewWithFilter).ClearSelection();
					(sender as DataGridViewWithFilter).Rows[e.RowIndex].Selected = true;
				}
				showDataBaseMenuStrip().Show(Cursor.Position);
			}
		}
		void tsmiEditFields_Click(object sender, EventArgs e)
		{
			string name = (sender as ToolStripMenuItem).Name;
			TabControl tc = this.Controls.Find("tcDataBase", true).FirstOrDefault() as TabControl;
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tc.SelectedIndex], true).FirstOrDefault() as DataGridViewWithFilter;
			if(!string.IsNullOrEmpty(name))
			{
				List<int> ids = new List<int>();
				int index = Convert.ToInt32(name.Split('_').Last());
				foreach (DataGridViewRow row in dgv.SelectedRows)
					ids.Add(Convert.ToInt32(row.Cells[0].Value));
				string value = dgv.SelectedRows[0].Cells[index].Value.ToString();
				UpdateRecordsForm form = new UpdateRecordsForm(tc.SelectedIndex, index, ids, value);
				if (form.ShowDialog() == DialogResult.OK)
					loadDataGridView();
			}
		}
		void tsmiChangeStates_Click(object sender, EventArgs e)
		{
			string name = (sender as ToolStripMenuItem).Name;
			TabControl tc = this.Controls.Find("tcDataBase", true).FirstOrDefault() as TabControl;
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tc.SelectedIndex], true).FirstOrDefault() as DataGridViewWithFilter;
			if (!string.IsNullOrEmpty(name) && tc != null && dgv != null)
			{
				List<int> ids = new List<int>();
				int index = Convert.ToInt32(name.Split('_').Last());
				foreach (DataGridViewRow row in dgv.SelectedRows)
					ids.Add(Convert.ToInt32(row.Cells[0].Value));
				connector.UpdateRecords(tc.SelectedIndex, 12, $"{index}", ids);
				loadDataGridView();
			}
		}
		void tsmiPrint_Click(object sender, EventArgs e)
		{
			TabControl tc = this.Controls.Find("tcDataBase", true).FirstOrDefault() as TabControl;
			if (true)
			{
				Word.Application wordApp = null;
				Word.Document wordDoc = null;
				string documentPath = string.Empty;
				try
				{
					wordApp = new Word.Application();
					wordApp.Visible = false;

					if ((sender as ToolStripMenuItem).Name == "tsmiMainP_order" && tc.SelectedIndex == 0)
						documentPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\order.docx"));
					else if ((sender as ToolStripMenuItem).Name == "tsmiMainP_order" && tc.SelectedIndex > 0)
						documentPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\order_comp.docx"));
					else if ((sender as ToolStripMenuItem).Name == "tsmiMainP_act")
						documentPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\act.docx"));

					wordDoc = wordApp.Documents.Open(documentPath);

					if ((sender as ToolStripMenuItem).Name == "tsmiMainP_order" && tc.SelectedIndex == 0)
						fillOrderTableForCartridges(tc.SelectedIndex, wordDoc);
					else if ((sender as ToolStripMenuItem).Name == "tsmiMainP_order" && tc.SelectedIndex > 0)
						fillOrderTableForComputersAndPrinters(tc.SelectedIndex, wordDoc);
					else if ((sender as ToolStripMenuItem).Name == "tsmiMainP_act")
						FillActTableForCartridges(tc.SelectedIndex, wordDoc);

					//wordApp.ActiveWindow.View.Type = Word.WdViewType.wdPrintPreview;
					//MessageBox.Show("Нажмите OK для печати после предпросмотра");

					PrintDialog printDialog = new PrintDialog();
					PrinterSettings settings = new PrinterSettings();

					printDialog.PrinterSettings = settings;
					printDialog.AllowSomePages = true;
					printDialog.ShowNetwork = true;

					if (printDialog.ShowDialog() == DialogResult.OK)
					{
						wordDoc.PrintOut();
						MessageBox.Show("Документ отправлен на печать!", "Успех",MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

				}
				catch (Exception ex)
				{
					MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				finally
				{
					if (wordDoc != null)
					{
						wordDoc.Close(false);
						System.Runtime.InteropServices.Marshal.ReleaseComObject(wordDoc);
					}

					if (wordApp != null)
					{
						wordApp.Quit(false);
						System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
					}

					GC.Collect();
					GC.WaitForPendingFinalizers();
				} 
			}
			else
			{
				throw new Exception("Find controls failed");
			}
		}
		void viewCheckBoxs_CheckChanged(object sender, EventArgs e)
		{
			CheckBox cb = sender as CheckBox;
			TabControl tc = this.Controls.Find("tcDataBase", true).FirstOrDefault() as TabControl;
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tc.SelectedIndex], true).FirstOrDefault() as DataGridViewWithFilter;
			int index = Convert.ToInt32(cb.Name.Split('_').Last());
			if (cb != null && tc != null && dgv != null)
			{
				dgv.Columns[index].Visible = cb.Checked;
			}
		}
		void viewRadioButton_CkeckChanged(object sender, EventArgs e)
		{
			RadioButton rb = sender as RadioButton;
			TabControl tc = this.Controls.Find("tcDataBase", true).FirstOrDefault() as TabControl;
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tc.SelectedIndex], true).FirstOrDefault() as DataGridViewWithFilter;
			if (rb != null && tc != null && dgv != null)
			{
				if(rb.Name == "rb_fill") dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
				else if(rb.Name == "rb_cell") dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
			}
		}
		void dataGridView_CellFormating(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex >= 0 && e.RowIndex < (sender as DataGridViewWithFilter).Rows.Count)
			{
				DataGridViewRow row = (sender as DataGridViewWithFilter).Rows[e.RowIndex];
				int index = Array.IndexOf(state_names, (row.DataBoundItem as DataRowView)?[12]?.ToString());
				row.DefaultCellStyle.BackColor = state_colors[index];
			}
		}
		void tcDataBase_SelectedIndexChanged(object sender, EventArgs e)
		{
			TabControl tc = sender as TabControl;
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tc.SelectedIndex], true).FirstOrDefault() as DataGridViewWithFilter;
			if(dgv != null && tc != null)
			{
				for (int i = 1; i < dgv.Columns.Count; i++)
				{
					CheckBox cb = this.Controls.Find($"cbView_{i}", true).FirstOrDefault() as CheckBox;
					if (cb != null)
						cb.Checked = dgv.Columns[i].Visible;
				}
				if (dgv.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.Fill)
					(this.Controls.Find("rb_fill", true).FirstOrDefault() as RadioButton).Checked = true;
				else
					(this.Controls.Find("rb_cell", true).FirstOrDefault() as RadioButton).Checked = true;
			}
		}

//Functions
		
		void loadDataGridView()
		{
			TabControl tc = this.Controls.Find("tcDataBase", true).FirstOrDefault() as TabControl;
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tc.SelectedIndex], true).FirstOrDefault() as DataGridViewWithFilter;
			if (tc != null && dgv != null)
			{
				dgv.DataSource = connector.SelectRecords(table_names[tc.SelectedIndex]);
			}
			if (dgv.Rows.Count > 0)
			{
				dgv.FirstDisplayedScrollingRowIndex = dgv.Rows.Count - 1;
			}
		}
		void fillOrderTableForCartridges(int tabpage_index, Word.Document wordDoc)
		{
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tabpage_index], true).FirstOrDefault() as DataGridViewWithFilter;
			if (dgv != null)
			{
				if (wordDoc.Tables.Count > 1)
				{
					Word.Table table1 = wordDoc.Tables[1];
					Word.Table table2 = wordDoc.Tables[2];

					table1.Cell(1, 3).Range.Text = dgv.CurrentRow.Cells[1].Value.ToString();
					DateTime date = Convert.ToDateTime(dgv.CurrentRow.Cells[2].Value);
					table1.Cell(1, 5).Range.Text = date.Day.ToString();
					table1.Cell(1, 7).Range.Text = date.Month.ToString();
					table1.Cell(1, 8).Range.Text = date.Year.ToString();
					table1.Cell(5, 2).Range.Text = dgv.CurrentRow.Cells[4].Value.ToString();

					DataTable dt = connector.GetOrderInfo(tabpage_index, dgv.CurrentRow.Cells[1].Value.ToString(), date.ToString("yyyy-MM-dd"));
					for (int i = 0; i < dt.Rows.Count; i++)
					{
						int rowNumber = i + 3;
						if (rowNumber <= table2.Rows.Count)
						{
							table2.Cell(rowNumber, 2).Range.Text = dt.Rows[i][0].ToString();
							table2.Cell(rowNumber, 3).Range.Text = dt.Rows[i][1].ToString();
						}
					}
				}
				else
				{
					throw new Exception("В документе не найдена таблица");
				} 
			}
			else
			{
				throw new Exception("Find controls failed");
			}
		}
		void fillOrderTableForComputersAndPrinters(int tabpage_index, Word.Document wordDoc)
		{
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tabpage_index], true).FirstOrDefault() as DataGridViewWithFilter;
			if (dgv != null)
			{
				if (wordDoc.Tables.Count > 1)
				{
					Word.Table table1 = wordDoc.Tables[1];
					Word.Table table2 = wordDoc.Tables[2];

					table1.Cell(1, 3).Range.Text = dgv.CurrentRow.Cells[1].Value.ToString();
					DateTime date = Convert.ToDateTime(dgv.CurrentRow.Cells[2].Value);
					table1.Cell(1, 5).Range.Text = date.Day.ToString();
					table1.Cell(1, 7).Range.Text = date.Month.ToString();
					table1.Cell(1, 8).Range.Text = date.Year.ToString();
					if (dgv.CurrentRow.Cells[3].Value.ToString() == "отсутствует")
						table1.Cell(3, 2).Range.Text = "";
					else
						table1.Cell(3, 2).Range.Text = dgv.CurrentRow.Cells[3].Value.ToString();
					table1.Cell(5, 2).Range.Text = dgv.CurrentRow.Cells[4].Value.ToString();
					table2.Cell(3, 1).Range.Text = dgv.CurrentRow.Cells[6].Value.ToString();
					table2.Cell(3, 4).Range.Text = dgv.CurrentRow.Cells[5].Value.ToString();
				}
				else
				{
					throw new Exception("В документе не найдена таблица");
				} 
			}
			else
			{
				throw new Exception("Find controls failed");
			}
		}
		void FillActTableForCartridges(int tabpage_index, Word.Document wordDoc)
		{
			DataGridViewWithFilter dgv = this.Controls.Find(dataGrid_names[tabpage_index], true).FirstOrDefault() as DataGridViewWithFilter;
			if (dgv != null)
			{
				string act_number = dgv.CurrentRow.Cells[9].Value.ToString();
				if (!string.IsNullOrWhiteSpace(act_number))
				{
					if (wordDoc.Tables.Count > 1)
					{
						Word.Table table = wordDoc.Tables[2];
						DataTable dt = connector.GetActInfo(tabpage_index, act_number);
						for (int i = 0; i < dt.Rows.Count; i++)
						{
							int rowNumber = i + 2;
							if (rowNumber <= table.Rows.Count)
							{
								table.Cell(rowNumber, 2).Range.Text = dt.Rows[i][0].ToString();
								table.Cell(rowNumber, 3).Range.Text = dt.Rows[i][1].ToString();
								table.Cell(rowNumber, 4).Range.Text = dt.Rows[i][2].ToString();
								table.Cell(rowNumber, 5).Range.Text = dt.Rows[i][3].ToString();
							}
						}
					}
					else
					{
						throw new Exception("В документе не найдена таблица");
					}  
				}
				else
				{
					throw new Exception("В выделенной строке номер акта отсуствует");
				}
			}
			else
			{
				throw new Exception("Find controls failed");
			}
		}
	}
}
